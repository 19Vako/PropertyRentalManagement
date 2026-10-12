using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PropertyRental.Domain.Entities;
using PropertyRental.Domain.Enums;
using PropertyRental.Domain.Identity;
using PropertyRental.Web.Models;
using PropertyRental.Web.Services;

namespace PropertyRental.Web.Controllers;

[Authorize]
public sealed class UnitsController(IPropertyManagementService propertyService) : Controller
{
    [Authorize(Policy = "PropertyManagerOnly")]
    public async Task<IActionResult> Index(
        int? propertyId,
        string? search,
        UnitStatus? status,
        CancellationToken cancellationToken)
    {
        var units = await propertyService.GetUnitsAsync(
            CurrentUserId(), propertyId, search, status, cancellationToken);
        ViewData["Search"] = search;
        ViewData["PropertyId"] = propertyId;
        ViewData["Status"] = status;
        return View(units);
    }

    [Authorize(Policy = "ApplicantOnly")]
    public async Task<IActionResult> Available(DateOnly? moveInDate, CancellationToken cancellationToken)
    {
        var date = moveInDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        ViewData["MoveInDate"] = date;
        var units = await propertyService.GetAvailableUnitsAsync(date, cancellationToken);
        return View(units);
    }

    [Authorize(Policy = "PropertyManagerOnly")]
    public async Task<IActionResult> Create(int propertyId, CancellationToken cancellationToken)
    {
        var property = await propertyService.GetPropertyAsync(CurrentUserId(), propertyId, cancellationToken);
        if (property is null || !property.IsActive)
        {
            return NotFound();
        }

        return View("Form", new UnitFormViewModel { PropertyId = property.Id, PropertyName = property.Name });
    }

    [Authorize(Policy = "PropertyManagerOnly")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UnitFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulatePropertyNameAsync(model.PropertyId, cancellationToken);
            return View("Form", model);
        }

        try
        {
            var unit = await propertyService.CreateUnitAsync(Map(model), CurrentUserId(), cancellationToken);
            TempData["StatusMessage"] = "Unit created.";
            return RedirectToAction(nameof(Details), new { id = unit.Id });
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [Authorize(Policy = "PropertyManagerOnly")]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var unit = await propertyService.GetUnitAsync(CurrentUserId(), id, cancellationToken);
        return unit is null ? NotFound() : View(unit);
    }

    [Authorize(Policy = "PropertyManagerOnly")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var unit = await propertyService.GetUnitAsync(CurrentUserId(), id, cancellationToken);
        return unit is null ? NotFound() : View("Form", Map(unit));
    }

    [Authorize(Policy = "PropertyManagerOnly")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UnitFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await PopulatePropertyNameAsync(model.PropertyId, cancellationToken);
            return View("Form", model);
        }

        try
        {
            if (!await propertyService.UpdateUnitAsync(Map(model), CurrentUserId(), cancellationToken))
            {
                return NotFound();
            }
        }
        catch (UnitAvailabilityConflictException exception)
        {
            ModelState.AddModelError(nameof(model.Status), exception.Message);
            await PopulatePropertyNameAsync(model.PropertyId, cancellationToken);
            return View("Form", model);
        }

        TempData["StatusMessage"] = "Unit updated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Policy = "PropertyManagerOnly")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var unit = await propertyService.GetUnitAsync(CurrentUserId(), id, cancellationToken);
        return unit is null ? NotFound() : View(unit);
    }

    [Authorize(Policy = "PropertyManagerOnly")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        var result = await propertyService.DeleteUnitAsync(CurrentUserId(), id, cancellationToken);
        if (result == DeleteResult.NotFound)
        {
            return NotFound();
        }

        if (result == DeleteResult.HasDependents)
        {
            TempData["ErrorMessage"] = "A unit with applications or leases cannot be deleted; change its status instead.";
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["StatusMessage"] = "Unit deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulatePropertyNameAsync(int propertyId, CancellationToken cancellationToken)
    {
        var property = await propertyService.GetPropertyAsync(CurrentUserId(), propertyId, cancellationToken);
        ViewData["PropertyName"] = property?.Name ?? string.Empty;
    }

    private string CurrentUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("Authenticated user has no identifier.");

    private static Unit Map(UnitFormViewModel model) => new()
    {
        Id = model.Id,
        PropertyId = model.PropertyId,
        UnitNumber = model.UnitNumber.Trim(),
        MonthlyRent = model.MonthlyRent,
        Bedrooms = model.Bedrooms,
        Bathrooms = model.Bathrooms,
        Status = model.Status,
        AvailableFrom = model.AvailableFrom
    };

    private static UnitFormViewModel Map(Unit unit) => new()
    {
        Id = unit.Id,
        PropertyId = unit.PropertyId,
        PropertyName = unit.Property.Name,
        UnitNumber = unit.UnitNumber,
        MonthlyRent = unit.MonthlyRent,
        Bedrooms = unit.Bedrooms,
        Bathrooms = unit.Bathrooms,
        Status = unit.Status,
        AvailableFrom = unit.AvailableFrom
    };
}