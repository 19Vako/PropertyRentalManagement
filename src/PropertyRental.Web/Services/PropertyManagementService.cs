using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using PropertyRental.Domain.Entities;
using PropertyRental.Domain.Enums;
using PropertyRental.Domain.Identity;
using PropertyRental.Infrastructure.Data;
using PropertyRental.Infrastructure.Identity;

namespace PropertyRental.Web.Services;

public sealed class PropertyManagementService(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager) : IPropertyManagementService
{
    public async Task<IReadOnlyList<Property>> GetPropertiesAsync(
        string managerUserId,
        string? search,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        await RequirePropertyManagerAsync(managerUserId);
        IQueryable<Property> query = dbContext.Properties.AsNoTracking()
            .Include(property => property.Units);

        if (isActive.HasValue)
        {
            query = query.Where(property => property.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(property => property.Name.Contains(term)
                || property.City.Contains(term)
                || property.StreetAddress.Contains(term));
        }

        return await query.OrderBy(property => property.Name).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Property>> GetAllActivePropertiesAsync(
        string? search,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Property> query = dbContext.Properties.AsNoTracking()
            .Include(property => property.Units)
            .Where(property => property.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(property => property.Name.Contains(term)
                || property.City.Contains(term)
                || property.StreetAddress.Contains(term));
        }

        return await query.OrderBy(property => property.Name).ToListAsync(cancellationToken);
    }

    public async Task<Property?> GetPropertyAsync(
        string managerUserId,
        int propertyId,
        CancellationToken cancellationToken = default)
    {
        await RequirePropertyManagerAsync(managerUserId);
        return await dbContext.Properties.AsNoTracking()
            .Include(property => property.Units)
            .FirstOrDefaultAsync(property => property.Id == propertyId, cancellationToken);
    }

    public async Task<Property> CreatePropertyAsync(
        Property property,
        string managerUserId,
        CancellationToken cancellationToken = default)
    {
        await RequirePropertyManagerAsync(managerUserId);
        property.ManagerUserId = managerUserId;
        property.CreatedUtc = DateTime.UtcNow;
        property.CountryCode = property.CountryCode.Trim().ToUpperInvariant();
        dbContext.Properties.Add(property);
        await dbContext.SaveChangesAsync(cancellationToken);
        return property;
    }

    public async Task<bool> UpdatePropertyAsync(
        Property values,
        string managerUserId,
        CancellationToken cancellationToken = default)
    {
        await RequirePropertyManagerAsync(managerUserId);
        var property = await dbContext.Properties.FirstOrDefaultAsync(
            item => item.Id == values.Id,
            cancellationToken);
        if (property is null)
        {
            return false;
        }

        property.Name = values.Name;
        property.StreetAddress = values.StreetAddress;
        property.City = values.City;
        property.Region = values.Region;
        property.PostalCode = values.PostalCode;
        property.CountryCode = values.CountryCode.Trim().ToUpperInvariant();
        property.Description = values.Description;
        property.IsActive = values.IsActive;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<DeleteResult> DeletePropertyAsync(
        string managerUserId,
        int propertyId,
        CancellationToken cancellationToken = default)
    {
        await RequirePropertyManagerAsync(managerUserId);
        var property = await dbContext.Properties.FirstOrDefaultAsync(
            item => item.Id == propertyId,
            cancellationToken);
        if (property is null)
        {
            return DeleteResult.NotFound;
        }

        if (await dbContext.Units.AnyAsync(unit => unit.PropertyId == propertyId, cancellationToken))
        {
            return DeleteResult.HasDependents;
        }

        dbContext.Properties.Remove(property);
        await dbContext.SaveChangesAsync(cancellationToken);
        return DeleteResult.Deleted;
    }

    public async Task<IReadOnlyList<Unit>> GetUnitsAsync(
        string managerUserId,
        int? propertyId,
        string? search,
        UnitStatus? status,
        CancellationToken cancellationToken = default)
    {
        await RequirePropertyManagerAsync(managerUserId);
        IQueryable<Unit> query = dbContext.Units.AsNoTracking()
            .Include(unit => unit.Property);

        if (propertyId.HasValue)
        {
            query = query.Where(unit => unit.PropertyId == propertyId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(unit => unit.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(unit => unit.UnitNumber.Contains(term)
                || unit.Property.Name.Contains(term)
                || unit.Property.City.Contains(term));
        }

        return await query.OrderBy(unit => unit.Property.Name)
            .ThenBy(unit => unit.UnitNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<Unit?> GetUnitAsync(
        string managerUserId,
        int unitId,
        CancellationToken cancellationToken = default)
    {
        await RequirePropertyManagerAsync(managerUserId);
        return await dbContext.Units.AsNoTracking()
            .Include(unit => unit.Property)
            .FirstOrDefaultAsync(unit => unit.Id == unitId, cancellationToken);
    }

    public async Task<Unit?> GetUnitForPropertyAsync(
        string managerUserId,
        int propertyId,
        int unitId,
        CancellationToken cancellationToken = default)
    {
        await RequirePropertyManagerAsync(managerUserId);
        return await dbContext.Units.AsNoTracking()
            .Include(unit => unit.Property)
            .FirstOrDefaultAsync(
                unit => unit.Id == unitId
                    && unit.PropertyId == propertyId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Unit>> GetAvailableUnitsAsync(
        DateOnly moveInDate,
        CancellationToken cancellationToken = default) =>
        await dbContext.Units.AsNoTracking()
            .Include(unit => unit.Property)
            .Where(unit => unit.Property.IsActive
                && unit.Status == UnitStatus.Available
                && (!unit.AvailableFrom.HasValue || unit.AvailableFrom.Value <= moveInDate)
                && !dbContext.Leases.Any(lease => lease.UnitId == unit.Id && lease.StatusId == 1))
            .OrderBy(unit => unit.Property.Name)
            .ThenBy(unit => unit.MonthlyRent)
            .ToListAsync(cancellationToken);

    public Task<bool> IsAvailableForApplicationAsync(
        int unitId,
        DateOnly moveInDate,
        CancellationToken cancellationToken = default) =>
        dbContext.Units.AnyAsync(unit => unit.Id == unitId
            && unit.Property.IsActive
            && unit.Status == UnitStatus.Available
            && (!unit.AvailableFrom.HasValue || unit.AvailableFrom.Value <= moveInDate)
            && !dbContext.Leases.Any(lease => lease.UnitId == unit.Id && lease.StatusId == 1), cancellationToken);

    public async Task<Unit> CreateUnitAsync(
        Unit unit,
        string managerUserId,
        CancellationToken cancellationToken = default)
    {
        await RequirePropertyManagerAsync(managerUserId);
        var propertyExists = await dbContext.Properties.AnyAsync(
            property => property.Id == unit.PropertyId,
            cancellationToken);
        if (!propertyExists)
        {
            throw new InvalidOperationException("The selected property is unavailable.");
        }

        unit.CreatedUtc = DateTime.UtcNow;
        dbContext.Units.Add(unit);
        await dbContext.SaveChangesAsync(cancellationToken);
        return unit;
    }

    public async Task<bool> UpdateUnitAsync(
        Unit values,
        string managerUserId,
        CancellationToken cancellationToken = default)
    {
        await RequirePropertyManagerAsync(managerUserId);
        var unit = await dbContext.Units.FirstOrDefaultAsync(
            item => item.Id == values.Id,
            cancellationToken);
        if (unit is null)
        {
            return false;
        }

        if (values.Status == UnitStatus.Available
            && await dbContext.Leases.AnyAsync(
                lease => lease.UnitId == values.Id && lease.StatusId == 1,
                cancellationToken))
        {
            throw new UnitAvailabilityConflictException(
                "A unit with an active lease cannot be marked available.");
        }

        unit.UnitNumber = values.UnitNumber;
        unit.MonthlyRent = values.MonthlyRent;
        unit.Bedrooms = values.Bedrooms;
        unit.Bathrooms = values.Bathrooms;
        unit.Status = values.Status;
        unit.AvailableFrom = values.AvailableFrom;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<DeleteResult> DeleteUnitAsync(
        string managerUserId,
        int unitId,
        CancellationToken cancellationToken = default)
    {
        await RequirePropertyManagerAsync(managerUserId);
        var unit = await dbContext.Units.FirstOrDefaultAsync(
            item => item.Id == unitId,
            cancellationToken);
        if (unit is null)
        {
            return DeleteResult.NotFound;
        }

        if (await dbContext.RentalApplications.AnyAsync(application => application.UnitId == unitId, cancellationToken)
            || await dbContext.Leases.AnyAsync(lease => lease.UnitId == unitId, cancellationToken))
        {
            return DeleteResult.HasDependents;
        }

        dbContext.Units.Remove(unit);
        await dbContext.SaveChangesAsync(cancellationToken);
        return DeleteResult.Deleted;
    }

    private async Task RequirePropertyManagerAsync(string managerUserId)
    {
        var manager = await userManager.FindByIdAsync(managerUserId);
        if (manager is null || !await userManager.IsInRoleAsync(manager, RoleNames.PropertyManager))
        {
            throw new UnauthorizedAccessException("Property Manager role is required for Property and Unit operations.");
        }
    }
}