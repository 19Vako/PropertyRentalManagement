using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PropertyRental.Domain.Identity;
using PropertyRental.Infrastructure.Identity;

namespace PropertyRental.Web.Areas.Identity.Pages.Account;

public sealed class RegisterModel(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = new ApplicationUser
        {
            UserName = Input.Email,
            Email = Input.Email,
            FirstName = Input.FirstName,
            LastName = Input.LastName,
            CreatedUtc = DateTime.UtcNow
        };

        var createResult = await userManager.CreateAsync(user, Input.Password);
        if (!createResult.Succeeded)
        {
            AddErrors(createResult);
            return Page();
        }

        var roleResult = await userManager.AddToRoleAsync(user, RoleNames.Applicant);
        if (!roleResult.Succeeded)
        {
            AddErrors(roleResult);
            await userManager.DeleteAsync(user);
            return Page();
        }

        await signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToAction("Applicant", "Dashboard");
    }

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    public sealed class InputModel
    {
        [Required, StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 8), DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required, Compare(nameof(Password)), DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}