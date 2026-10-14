using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PropertyRental.Domain.Identity;
using PropertyRental.Infrastructure.Identity;

namespace PropertyRental.Web.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IConfiguration configuration) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, model.Password, false);
        if (!result.Succeeded)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var roles = await userManager.GetRolesAsync(user);
        var token = GenerateJwtToken(user, roles);

        return Ok(new
        {
            token,
            email = user.Email,
            firstName = user.FirstName,
            lastName = user.LastName,
            roles
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var role = ResolveRole(model.Role);
        if (role is null)
        {
            return BadRequest(new
            {
                message = $"Invalid role. Allowed roles are: {string.Join(", ", AllowedRoles)}."
            });
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            CreatedUtc = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }


        await userManager.AddToRoleAsync(user, role);
        var roles = await userManager.GetRolesAsync(user);
        var token = GenerateJwtToken(user, roles);

        return Ok(new
        {
            token,
            email = user.Email,
            firstName = user.FirstName,
            lastName = user.LastName,
            roles
        });
    }

    private static readonly string[] AllowedRoles = [RoleNames.Applicant, RoleNames.PropertyManager];

    /// <summary>
    /// Normalizes the requested role. Accepts "PropertyManager" or "Property Manager" (case-insensitive)
    /// for the manager role, and "Applicant" for the applicant role. Null/empty defaults to Applicant.
    /// Returns null when an unknown role is supplied.
    /// </summary>
    private static string? ResolveRole(string? requestedRole)
    {
        if (string.IsNullOrWhiteSpace(requestedRole))
        {
            return RoleNames.Applicant;
        }

        var normalized = requestedRole.Trim();

        if (string.Equals(normalized, RoleNames.PropertyManager, StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalized, "PropertyManager", StringComparison.OrdinalIgnoreCase))
        {
            return RoleNames.PropertyManager;
        }

        if (string.Equals(normalized, RoleNames.Applicant, StringComparison.OrdinalIgnoreCase))
        {
            return RoleNames.Applicant;
        }

        return null;
    }

    private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured.");
        var key = Encoding.UTF8.GetBytes(secret);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(double.TryParse(jwtSettings["ExpiryHours"], out var hours) ? hours : 72),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

public record LoginDto(string Email, string Password);
public record RegisterDto(string Email, string Password, string FirstName, string LastName, string? Role = null);