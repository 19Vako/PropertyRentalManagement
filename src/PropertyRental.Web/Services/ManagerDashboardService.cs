using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PropertyRental.Domain.Entities;
using PropertyRental.Domain.Enums;
using PropertyRental.Domain.Identity;
using PropertyRental.Infrastructure.Data;
using PropertyRental.Web.Models;

namespace PropertyRental.Web.Services;

public sealed class ManagerDashboardService(ApplicationDbContext dbContext) : IManagerDashboardService
{
    public async Task<ManagerDashboardViewModel> GetDashboardAsync(
        ClaimsPrincipal user,
        ManagerDashboardViewModel filters,
        CancellationToken cancellationToken = default)
    {
        if (!user.IsInRole(RoleNames.PropertyManager)
            || string.IsNullOrWhiteSpace(user.FindFirstValue(ClaimTypes.NameIdentifier)))
        {
            throw new UnauthorizedAccessException("Property Manager access is required.");
        }

        var applications = dbContext.RentalApplications.AsNoTracking();
        filters.PendingCount = await applications.CountAsync(application =>
            application.StatusId == (byte)ApplicationStatus.Submitted
            || application.StatusId == (byte)ApplicationStatus.UnderReview, cancellationToken);
        filters.ApprovedCount = await applications.CountAsync(
            application => application.StatusId == (byte)ApplicationStatus.Approved, cancellationToken);
        filters.RejectedCount = await applications.CountAsync(
            application => application.StatusId == (byte)ApplicationStatus.Rejected, cancellationToken);
        filters.ReturnedCount = await applications.CountAsync(
            application => application.StatusId == (byte)ApplicationStatus.ReturnedForCorrection, cancellationToken);
        filters.ActiveLeaseCount = await dbContext.Leases.CountAsync(
            lease => lease.StatusId == (byte)LeaseStatus.Active, cancellationToken);
        filters.ActivePropertyCount = await dbContext.Properties.CountAsync(
            property => property.IsActive, cancellationToken);
        filters.AvailableUnitCount = await dbContext.Units.CountAsync(unit =>
            unit.Property.IsActive
            && unit.Status == UnitStatus.Available
            && !dbContext.Leases.Any(lease => lease.UnitId == unit.Id
                && lease.StatusId == (byte)LeaseStatus.Active), cancellationToken);

        filters.Properties = await dbContext.Properties.AsNoTracking()
            .OrderBy(property => property.Name)
            .Select(property => new ManagerPropertyOption(property.Id, property.Name, property.City))
            .ToListAsync(cancellationToken);

        var unitsQuery = dbContext.Units.AsNoTracking().AsQueryable();

        if (filters.PropertyId.HasValue)
        {
            unitsQuery = unitsQuery.Where(unit => unit.PropertyId == filters.PropertyId.Value);
        }

        filters.Units = await unitsQuery
            .OrderBy(unit => unit.Property.Name)
            .ThenBy(unit => unit.UnitNumber)
            .Select(unit => new ManagerUnitOption(
                unit.Id, unit.PropertyId, unit.Property.Name, unit.UnitNumber))
            .ToListAsync(cancellationToken);

        IQueryable<RentalApplication> query = applications
            .Include(application => application.Unit)
                .ThenInclude(unit => unit.Property)
            .Include(application => application.Status);

        if (filters.Status.HasValue && Enum.IsDefined(filters.Status.Value))
        {
            query = query.Where(application => application.StatusId == (byte)filters.Status.Value);
        }

        if (filters.PropertyId.HasValue)
        {
            query = query.Where(application => application.Unit.PropertyId == filters.PropertyId.Value);
        }

        if (filters.UnitId.HasValue)
        {
            query = query.Where(application => application.UnitId == filters.UnitId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filters.Applicant))
        {
            var applicantTerm = filters.Applicant.Trim();
            query = query.Where(application => application.ApplicantUserId.Contains(applicantTerm)
                || dbContext.Users.Any(applicant => applicant.Id == application.ApplicantUserId
                    && ((applicant.Email != null && applicant.Email.Contains(applicantTerm))
                        || applicant.FirstName.Contains(applicantTerm)
                        || applicant.LastName.Contains(applicantTerm))));
        }

        filters.TotalCount = await query.CountAsync(cancellationToken);
        filters.Page = Math.Clamp(filters.Page, 1, filters.TotalPages);

        query = filters.SortBy switch
        {
            "updated" => query.OrderByDescending(application => application.UpdatedUtc)
                .ThenByDescending(application => application.Id),
            "submitted" => query.OrderByDescending(application => application.SubmittedUtc)
                .ThenByDescending(application => application.Id),
            _ => query.OrderByDescending(application => application.CreatedUtc)
                .ThenByDescending(application => application.Id)
        };

        filters.Applications = await query
            .Skip((filters.Page - 1) * ManagerDashboardViewModel.PageSize)
            .Take(ManagerDashboardViewModel.PageSize)
            .ToListAsync(cancellationToken);

        return filters;
    }
}