using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PropertyRental.Domain.Entities;
using PropertyRental.Domain.Enums;
using PropertyRental.Domain.Identity;
using PropertyRental.Infrastructure.Data;
using PropertyRental.Infrastructure.Identity;
using PropertyRental.Web.Models;

namespace PropertyRental.Web.Services;

public sealed class LeaseManagementService(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager) : ILeaseManagementService
{
    public async Task CreateForApprovedApplicationAsync(
        RentalApplication application,
        string managerUserId,
        string reviewNotes,
        DateTime createdUtc,
        CancellationToken cancellationToken = default)
    {
        await RequireManagerIdAsync(managerUserId);
        if (dbContext.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("Lease creation must run inside the application approval transaction.");
        }

        if (application.StatusId != (byte)ApplicationStatus.UnderReview || application.Unit is null)
        {
            throw new ApplicationWorkflowException("Only an application under review with a loaded unit can create a lease.");
        }

        var unit = application.Unit;
        var today = DateOnly.FromDateTime(createdUtc);
        var startDate = application.DesiredMoveInDate.GetValueOrDefault(today);
        if (startDate < today)
        {
            startDate = today;
        }

        if (!unit.Property.IsActive
            || unit.Status != UnitStatus.Available
            || (unit.AvailableFrom.HasValue && unit.AvailableFrom.Value > startDate))
        {
            throw new ApplicationWorkflowException("The unit is no longer available for approval.");
        }

        if (unit.MonthlyRent < 0 || startDate.AddMonths(12) <= startDate)
        {
            throw new ApplicationWorkflowException("The unit lease terms are invalid.");
        }

        if (await dbContext.Leases.AnyAsync(lease =>
                lease.UnitId == unit.Id && lease.StatusId == (byte)LeaseStatus.Active,
                cancellationToken))
        {
            throw new ApplicationWorkflowException("The unit already has an active lease.");
        }

        if (await dbContext.Leases.AnyAsync(
                lease => lease.ApplicationId == application.Id,
                cancellationToken))
        {
            throw new ApplicationWorkflowException("A lease already exists for this application.");
        }

        var notes = NormalizeReviewNotes(reviewNotes);
        application.StatusId = (byte)ApplicationStatus.Approved;
        application.UpdatedUtc = createdUtc;
        unit.Status = UnitStatus.Leased;
        dbContext.Leases.Add(new Lease
        {
            ApplicationId = application.Id,
            TenantUserId = application.ApplicantUserId,
            UnitId = unit.Id,
            StatusId = (byte)LeaseStatus.Active,
            StartDate = startDate,
            EndDate = startDate.AddMonths(12),
            MonthlyRent = unit.MonthlyRent,
            CreatedUtc = createdUtc
        });
        dbContext.ApplicationReviews.Add(new ApplicationReview
        {
            ApplicationId = application.Id,
            ReviewerUserId = managerUserId,
            Decision = ReviewDecision.Approved,
            Notes = notes,
            CreatedUtc = createdUtc
        });
        application.Activities.Add(new RentalApplicationActivity
        {
            ActorUserId = managerUserId,
            Type = RentalApplicationActivityType.Approved,
            Notes = notes,
            CreatedUtc = createdUtc
        });
    }

    public async Task<IReadOnlyList<LeaseListItemViewModel>> GetLeasesAsync(
        ClaimsPrincipal user,
        LeaseStatus? status,
        string? search,
        CancellationToken cancellationToken = default)
    {
        RequireManager(user);
        var query = WithLeaseIncludes(dbContext.Leases.AsNoTracking());

        if (status.HasValue)
        {
            query = query.Where(lease => lease.StatusId == (byte)status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(lease => lease.Unit.UnitNumber.Contains(term)
                || lease.Unit.Property.Name.Contains(term)
                || lease.Unit.Property.City.Contains(term)
                || lease.TenantUserId.Contains(term)
                || dbContext.Users.Any(tenant => tenant.Id == lease.TenantUserId
                    && ((tenant.Email != null && tenant.Email.Contains(term))
                        || tenant.FirstName.Contains(term)
                        || tenant.LastName.Contains(term))));
        }

        var leases = await query.OrderBy(lease => lease.StatusId)
            .ThenBy(lease => lease.EndDate)
            .ToListAsync(cancellationToken);
        return await AddTenantDetailsAsync(leases, cancellationToken);
    }

    public async Task<LeaseDetailsViewModel?> GetLeaseAsync(
        ClaimsPrincipal user,
        int leaseId,
        CancellationToken cancellationToken = default)
    {
        RequireManager(user);
        var lease = await WithLeaseIncludes(dbContext.Leases.AsNoTracking())
            .FirstOrDefaultAsync(item => item.Id == leaseId, cancellationToken);
        if (lease is null)
        {
            return null;
        }

        var tenant = await dbContext.Users.AsNoTracking()
            .Where(item => item.Id == lease.TenantUserId)
            .Select(item => new { item.FirstName, item.LastName, item.Email })
            .FirstOrDefaultAsync(cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return new LeaseDetailsViewModel
        {
            Lease = lease,
            TenantName = tenant is null ? lease.TenantUserId : $"{tenant.FirstName} {tenant.LastName}".Trim(),
            TenantEmail = tenant?.Email ?? string.Empty,
            CanTerminate = lease.StatusId == (byte)LeaseStatus.Active && lease.EndDate >= today,
            CanMarkExpired = lease.StatusId == (byte)LeaseStatus.Active && lease.EndDate < today
        };
    }

    public async Task<bool> TerminateAsync(
        ClaimsPrincipal user,
        int leaseId,
        CancellationToken cancellationToken = default)
    {
        RequireManager(user);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var lease = await dbContext.Leases
            .Include(item => item.Unit)
            .FirstOrDefaultAsync(item => item.Id == leaseId, cancellationToken);
        if (lease is null)
        {
            return false;
        }

        if (lease.StatusId != (byte)LeaseStatus.Active)
        {
            throw new ApplicationWorkflowException("Only active leases can be terminated.");
        }

        if (lease.EndDate < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ApplicationWorkflowException("This lease has passed its end date; mark it expired instead.");
        }

        var now = DateTime.UtcNow;
        lease.StatusId = (byte)LeaseStatus.Terminated;
        lease.ClosedUtc = now;
        if (!await HasOtherActiveLeaseAsync(lease.UnitId, lease.Id, cancellationToken))
        {
            lease.Unit.Status = UnitStatus.Available;
            lease.Unit.AvailableFrom = DateOnly.FromDateTime(now);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<bool> MarkExpiredAsync(
        ClaimsPrincipal user,
        int leaseId,
        CancellationToken cancellationToken = default)
    {
        RequireManager(user);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var lease = await dbContext.Leases
            .Include(item => item.Unit)
            .FirstOrDefaultAsync(item => item.Id == leaseId, cancellationToken);
        if (lease is null)
        {
            return false;
        }

        if (lease.StatusId != (byte)LeaseStatus.Active
            || lease.EndDate >= DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ApplicationWorkflowException("Only active leases past their end date can be marked expired.");
        }

        var now = DateTime.UtcNow;
        lease.StatusId = (byte)LeaseStatus.Expired;
        lease.ClosedUtc = now;
        if (!await HasOtherActiveLeaseAsync(lease.UnitId, lease.Id, cancellationToken))
        {
            lease.Unit.Status = UnitStatus.Available;
            lease.Unit.AvailableFrom = DateOnly.FromDateTime(now);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    private async Task<bool> HasOtherActiveLeaseAsync(
        int unitId,
        int currentLeaseId,
        CancellationToken cancellationToken) =>
        await dbContext.Leases.AnyAsync(lease => lease.UnitId == unitId
            && lease.Id != currentLeaseId
            && lease.StatusId == (byte)LeaseStatus.Active, cancellationToken);

    private async Task<IReadOnlyList<LeaseListItemViewModel>> AddTenantDetailsAsync(
        IReadOnlyList<Lease> leases,
        CancellationToken cancellationToken)
    {
        var userIds = leases.Select(lease => lease.TenantUserId).Distinct().ToArray();
        var tenants = await dbContext.Users.AsNoTracking()
            .Where(user => userIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, cancellationToken);

        return leases.Select(lease =>
        {
            tenants.TryGetValue(lease.TenantUserId, out var tenant);
            return new LeaseListItemViewModel
            {
                Lease = lease,
                TenantName = tenant is null ? lease.TenantUserId : $"{tenant.FirstName} {tenant.LastName}".Trim(),
                TenantEmail = tenant?.Email ?? string.Empty
            };
        }).ToArray();
    }

    private static IQueryable<Lease> WithLeaseIncludes(IQueryable<Lease> query) =>
        query.Include(lease => lease.Unit)
            .ThenInclude(unit => unit.Property)
            .Include(lease => lease.Status);

    private static void RequireManager(ClaimsPrincipal user)
    {
        if (!user.IsInRole(RoleNames.PropertyManager)
            || string.IsNullOrWhiteSpace(user.FindFirstValue(ClaimTypes.NameIdentifier)))
        {
            throw new UnauthorizedAccessException("Property Manager access is required.");
        }
    }

    private async Task RequireManagerIdAsync(string managerUserId)
    {
        var manager = await userManager.FindByIdAsync(managerUserId);
        if (manager is null || !await userManager.IsInRoleAsync(manager, RoleNames.PropertyManager))
        {
            throw new UnauthorizedAccessException("Property Manager role is required to create a lease.");
        }
    }

    private static string NormalizeReviewNotes(string notes)
    {
        var value = notes.Trim();
        if (value.Length == 0 || value.Length > 2000)
        {
            throw new ApplicationWorkflowException("Review notes are required and must not exceed 2000 characters.");
        }

        return value;
    }
}