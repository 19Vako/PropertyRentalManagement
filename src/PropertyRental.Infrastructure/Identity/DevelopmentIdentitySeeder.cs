using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PropertyRental.Domain.Identity;

namespace PropertyRental.Infrastructure.Identity;

public static class DevelopmentIdentitySeeder
{
    private static readonly (string Name, string Id)[] Roles =
    [
        (RoleNames.Applicant, "a75369a6-5193-4e5a-92bd-d91c26c00001"),
        (RoleNames.PropertyManager, "a75369a6-5193-4e5a-92bd-d91c26c00002")
    ];

    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var roleDefinition in Roles)
        {
            if (!await roleManager.RoleExistsAsync(roleDefinition.Name))
            {
                var result = await roleManager.CreateAsync(new IdentityRole
                {
                    Id = roleDefinition.Id,
                    Name = roleDefinition.Name,
                    NormalizedName = roleDefinition.Name.ToUpperInvariant()
                });
                EnsureSucceeded(result, $"Could not create role '{roleDefinition.Name}'.");
            }
        }

        await SeedUserAsync(
            userManager,
            configuration["DevelopmentSeed:ApplicantPassword"],
            "applicant.demo@example.test",
            "Demo",
            "Applicant",
            RoleNames.Applicant);

        await SeedUserAsync(
            userManager,
            configuration["DevelopmentSeed:ManagerPassword"],
            "manager.demo@example.test",
            "Demo",
            "Manager",
            RoleNames.PropertyManager);
    }

    private static async Task SeedUserAsync(
        UserManager<ApplicationUser> userManager,
        string? password,
        string email,
        string firstName,
        string lastName,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return;
            }

            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName,
                CreatedUtc = DateTime.UtcNow
            };

            var createResult = await userManager.CreateAsync(user, password);
            EnsureSucceeded(createResult, $"Could not create development user '{email}'.");
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            var roleResult = await userManager.AddToRoleAsync(user, role);
            EnsureSucceeded(roleResult, $"Could not add '{email}' to role '{role}'.");
        }
    }

    private static void EnsureSucceeded(IdentityResult result, string message)
    {
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"{message} {errors}");
        }
    }
}