using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyRental.Domain.Enums;
using PropertyRental.Web.Services;

namespace PropertyRental.Web.Controllers;

[Authorize(Policy = "PropertyManagerOnly")]
public sealed class LeasesController(ILeaseManagementService leaseManagementService) : Controller
{
    public async Task<IActionResult> Index(string? status, string? search, CancellationToken cancellationToken)
    {
        LeaseStatus? statusFilter = LeaseStatus.Active;
        if (string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            statusFilter = null;
        }
        else if (!string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<LeaseStatus>(status, ignoreCase: true, out var parsedStatus))
        {
            statusFilter = parsedStatus;
        }

        var leases = await leaseManagementService.GetLeasesAsync(User, statusFilter, search, cancellationToken);
        ViewData["StatusFilter"] = status ?? "active";
        ViewData["Search"] = search;
        return View(leases);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var lease = await leaseManagementService.GetLeaseAsync(User, id, cancellationToken);
        return lease is null ? NotFound() : View(lease);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Terminate(int id, CancellationToken cancellationToken)
    {
        try
        {
            if (!await leaseManagementService.TerminateAsync(User, id, cancellationToken))
            {
                return NotFound();
            }
            TempData["StatusMessage"] = "Lease terminated; the unit is now available.";
        }
        catch (ApplicationWorkflowException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkExpired(int id, CancellationToken cancellationToken)
    {
        try
        {
            if (!await leaseManagementService.MarkExpiredAsync(User, id, cancellationToken))
            {
                return NotFound();
            }
            TempData["StatusMessage"] = "Lease marked expired; the unit is now available.";
        }
        catch (ApplicationWorkflowException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}