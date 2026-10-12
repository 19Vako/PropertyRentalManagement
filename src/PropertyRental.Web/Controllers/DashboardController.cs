using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyRental.Domain.Identity;
using PropertyRental.Web.Models;
using PropertyRental.Web.Services;

namespace PropertyRental.Web.Controllers;

[Authorize]
public sealed class DashboardController(
    IManagerDashboardService managerDashboardService,
    IRentalApplicationWorkflowService rentalApplicationWorkflowService) : Controller
{
    [Authorize(Policy = "ApplicantOnly")]
    public async Task<IActionResult> Applicant(
        ApplicantDashboardViewModel filters,
        CancellationToken cancellationToken)
    {
        if (filters.MoveInFrom.HasValue && filters.MoveInTo.HasValue
            && filters.MoveInFrom.Value > filters.MoveInTo.Value)
        {
            ModelState.AddModelError(nameof(filters.MoveInTo), "End date must be on or after start date.");
        }

        var dashboard = await rentalApplicationWorkflowService.GetApplicantDashboardAsync(
            User,
            filters,
            cancellationToken);
        return View(dashboard);
    }

    [Authorize(Policy = "PropertyManagerOnly")]
    public async Task<IActionResult> Manager(
        ManagerDashboardViewModel filters,
        CancellationToken cancellationToken)
    {
        var dashboard = await managerDashboardService.GetDashboardAsync(User, filters, cancellationToken);
        return View(dashboard);
    }
}
