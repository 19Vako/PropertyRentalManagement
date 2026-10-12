using PropertyRental.Domain.Entities;
using PropertyRental.Domain.Enums;

namespace PropertyRental.Web.Services;

public interface IPropertyManagementService
{
    Task<IReadOnlyList<Property>> GetPropertiesAsync(
        string managerUserId,
        string? search,
        bool? isActive,
        CancellationToken cancellationToken = default);

    Task<Property?> GetPropertyAsync(string managerUserId, int propertyId, CancellationToken cancellationToken = default);

    Task<Property> CreatePropertyAsync(Property property, string managerUserId, CancellationToken cancellationToken = default);

    Task<bool> UpdatePropertyAsync(Property property, string managerUserId, CancellationToken cancellationToken = default);

    Task<DeleteResult> DeletePropertyAsync(string managerUserId, int propertyId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Unit>> GetUnitsAsync(
        string managerUserId,
        int? propertyId,
        string? search,
        UnitStatus? status,
        CancellationToken cancellationToken = default);

    Task<Unit?> GetUnitAsync(string managerUserId, int unitId, CancellationToken cancellationToken = default);

    Task<Unit?> GetUnitForPropertyAsync(string managerUserId, int propertyId, int unitId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Unit>> GetAvailableUnitsAsync(DateOnly moveInDate, CancellationToken cancellationToken = default);

    Task<bool> IsAvailableForApplicationAsync(int unitId, DateOnly moveInDate, CancellationToken cancellationToken = default);

    Task<Unit> CreateUnitAsync(Unit unit, string managerUserId, CancellationToken cancellationToken = default);

    Task<bool> UpdateUnitAsync(Unit unit, string managerUserId, CancellationToken cancellationToken = default);

    Task<DeleteResult> DeleteUnitAsync(string managerUserId, int unitId, CancellationToken cancellationToken = default);
}

public enum DeleteResult
{
    NotFound,
    HasDependents,
    Deleted
}

public sealed class UnitAvailabilityConflictException(string message) : InvalidOperationException(message);