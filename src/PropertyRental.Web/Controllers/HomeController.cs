using Microsoft.AspNetCore.Mvc;
using PropertyRental.Domain.Identity;

namespace PropertyRental.Web.Controllers;

public sealed class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.IsInRole(RoleNames.PropertyManager))
        {
            return RedirectToAction("Manager", "Dashboard");
        }

        if (User.IsInRole(RoleNames.Applicant))
        {
            return RedirectToAction("Applicant", "Dashboard");
        }

        return View();
    }

    public IActionResult AccessDenied() => View();

    public IActionResult Error() => View();
}
