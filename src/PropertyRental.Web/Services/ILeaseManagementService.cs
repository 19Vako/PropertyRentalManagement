using System.Security.Claims;
using PropertyRental.Domain.Entities;
using PropertyRental.Domain.Enums;
using PropertyRental.Web.Models;

namespace PropertyRental.Web.Services;

public interface ILeaseManagementService
{
    Task CreateForApprovedApplicationAsync(
        RentalApplication application,
        string managerUserId,
        string reviewNotes,
        DateTime createdUtc,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LeaseListItemViewModel>> GetLeasesAsync(
        ClaimsPrincipal user,
        LeaseStatus? status,
        string? search,
        CancellationToken cancellationToken = default);

    Task<LeaseDetailsViewModel?> GetLeaseAsync(
        ClaimsPrincipal user,
        int leaseId,
        CancellationToken cancellationToken = default);

    Task<bool> TerminateAsync(
        ClaimsPrincipal user,
        int leaseId,
        CancellationToken cancellationToken = default);

    Task<bool> MarkExpiredAsync(
        ClaimsPrincipal user,
        int leaseId,
        CancellationToken cancellationToken = default);
}