using Microsoft.AspNetCore.Authentication.JwtBearer;
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
    // --- WEB (MVC) METHODS ---

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

    // --- MOBILE REST API METHODS (JWT / JSON) ---

    [HttpGet("api/dashboard/manager")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "PropertyManagerOnly")]
    public async Task<IActionResult> ManagerApi(
        [FromQuery] ManagerDashboardViewModel filters,
        CancellationToken cancellationToken)
    {
        var dashboard = await managerDashboardService.GetDashboardAsync(User, filters, cancellationToken);
        return Ok(dashboard);
    }

    [HttpGet("api/dashboard/applicant")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "ApplicantOnly")]
    public async Task<IActionResult> ApplicantApi(
        [FromQuery] ApplicantDashboardViewModel filters,
        CancellationToken cancellationToken)
    {
        if (filters.MoveInFrom.HasValue && filters.MoveInTo.HasValue
            && filters.MoveInFrom.Value > filters.MoveInTo.Value)
        {
            return BadRequest(new { message = "End date must be on or after start date." });
        }

        var dashboard = await rentalApplicationWorkflowService.GetApplicantDashboardAsync(
            User,
            filters,
            cancellationToken);
        return Ok(dashboard);
    }
}