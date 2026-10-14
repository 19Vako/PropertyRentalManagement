using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyRental.Domain.Entities;
using PropertyRental.Domain.Identity;
using PropertyRental.Web.Models;
using PropertyRental.Web.Services;

namespace PropertyRental.Web.Controllers;

[Authorize(Policy = "PropertyManagerOnly")]
public sealed class PropertiesController(IPropertyManagementService propertyService) : Controller
{
    // --- WEB (MVC) METHODS ---

    public async Task<IActionResult> Index(string? search, bool? isActive, CancellationToken cancellationToken)
    {
        var properties = await propertyService.GetPropertiesAsync(
            CurrentUserId(), search, isActive, cancellationToken);
        ViewData["Search"] = search;
        ViewData["IsActive"] = isActive;
        return View(properties);
    }

    public IActionResult Create() => View("Form", new PropertyFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PropertyFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        var property = await propertyService.CreatePropertyAsync(Map(model), CurrentUserId(), cancellationToken);
        TempData["StatusMessage"] = "Property created.";
        return RedirectToAction(nameof(Details), new { id = property.Id });
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var property = await propertyService.GetPropertyAsync(CurrentUserId(), id, cancellationToken);
        return property is null ? NotFound() : View(property);
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var property = await propertyService.GetPropertyAsync(CurrentUserId(), id, cancellationToken);
        return property is null ? NotFound() : View("Form", Map(property));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PropertyFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        if (!await propertyService.UpdatePropertyAsync(Map(model), CurrentUserId(), cancellationToken))
        {
            return NotFound();
        }

        TempData["StatusMessage"] = "Property updated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var property = await propertyService.GetPropertyAsync(CurrentUserId(), id, cancellationToken);
        return property is null ? NotFound() : View(property);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        var result = await propertyService.DeletePropertyAsync(CurrentUserId(), id, cancellationToken);
        if (result == DeleteResult.NotFound)
        {
            return NotFound();
        }

        if (result == DeleteResult.HasDependents)
        {
            TempData["ErrorMessage"] = "Remove or archive this property's units before deleting it.";
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["StatusMessage"] = "Property deleted.";
        return RedirectToAction(nameof(Index));
    }

    // --- MOBILE REST API METHODS (JWT / JSON) ---

    [HttpGet("api/properties")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetPropertiesApi(
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        // Managers browse their own properties; applicants browse all active properties.
        if (User.IsInRole(RoleNames.PropertyManager))
        {
            var own = await propertyService.GetPropertiesAsync(
                CurrentUserId(), search, isActive, cancellationToken);
            return Ok(own);
        }

        var properties = await propertyService.GetAllActivePropertiesAsync(search, cancellationToken);
        return Ok(properties);
    }

    [HttpGet("api/properties/{id:int}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetPropertyApi(int id, CancellationToken cancellationToken)
    {
        Property? property;
        if (User.IsInRole(RoleNames.PropertyManager))
        {
            property = await propertyService.GetPropertyAsync(CurrentUserId(), id, cancellationToken);
        }
        else
        {
            var browsable = await propertyService.GetAllActivePropertiesAsync(null, cancellationToken);
            property = browsable.FirstOrDefault(item => item.Id == id);
        }

        return property is null ? NotFound(new { message = "Property not found." }) : Ok(property);
    }

    [HttpGet("api/properties/{id:int}/units")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "PropertyManagerOnly")]
    public async Task<IActionResult> GetPropertyUnitsApi(int id, CancellationToken cancellationToken)
    {
        var units = await propertyService.GetUnitsAsync(
            CurrentUserId(), id, search: null, status: null, cancellationToken);
        return Ok(units);
    }

    [HttpPost("api/properties")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "PropertyManagerOnly")]
    public async Task<IActionResult> CreatePropertyApi(
        [FromBody] PropertyFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var property = await propertyService.CreatePropertyAsync(Map(model), CurrentUserId(), cancellationToken);
        return CreatedAtAction(nameof(GetPropertyApi), new { id = property.Id }, property);
    }

    [HttpPut("api/properties/{id:int}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "PropertyManagerOnly")]
    public async Task<IActionResult> UpdatePropertyApi(
        int id,
        [FromBody] PropertyFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return BadRequest(new { message = "Id mismatch." });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!await propertyService.UpdatePropertyAsync(Map(model), CurrentUserId(), cancellationToken))
        {
            return NotFound(new { message = "Property not found." });
        }

        return NoContent();
    }

    [HttpDelete("api/properties/{id:int}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "PropertyManagerOnly")]
    public async Task<IActionResult> DeletePropertyApi(int id, CancellationToken cancellationToken)
    {
        var result = await propertyService.DeletePropertyAsync(CurrentUserId(), id, cancellationToken);
        if (result == DeleteResult.NotFound)
        {
            return NotFound(new { message = "Property not found." });
        }

        if (result == DeleteResult.HasDependents)
        {
            return Conflict(new { message = "Remove or archive this property's units before deleting it." });
        }

        return Ok(new { message = "Property deleted." });
    }

    // --- HELPERS ---

    private string CurrentUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("Authenticated manager has no user identifier.");

    private static Property Map(PropertyFormViewModel model) => new()
    {
        Id = model.Id,
        Name = model.Name.Trim(),
        StreetAddress = model.StreetAddress.Trim(),
        City = model.City.Trim(),
        Region = model.Region.Trim(),
        PostalCode = model.PostalCode.Trim(),
        CountryCode = model.CountryCode.Trim().ToUpperInvariant(),
        Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),
        IsActive = model.IsActive
    };

    private static PropertyFormViewModel Map(Property property) => new()
    {
        Id = property.Id,
        Name = property.Name,
        StreetAddress = property.StreetAddress,
        City = property.City,
        Region = property.Region,
        PostalCode = property.PostalCode,
        CountryCode = property.CountryCode,
        Description = property.Description,
        IsActive = property.IsActive
    };
}