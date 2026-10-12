using System.Security.Claims;
using System.Data;
using Microsoft.EntityFrameworkCore;
using PropertyRental.Domain.Entities;
using PropertyRental.Domain.Enums;
using PropertyRental.Domain.Identity;
using PropertyRental.Infrastructure.Data;
using PropertyRental.Web.Models;

namespace PropertyRental.Web.Services;

public sealed class RentalApplicationWorkflowService(
    ApplicationDbContext dbContext,
    IPropertyManagementService propertyManagementService,
    ILeaseManagementService leaseManagementService) : IRentalApplicationWorkflowService
{
    private static readonly byte[] OpenStatuses =
    [
        (byte)ApplicationStatus.Draft,
        (byte)ApplicationStatus.Submitted,
        (byte)ApplicationStatus.UnderReview,
        (byte)ApplicationStatus.ReturnedForCorrection
    ];

    public async Task<IReadOnlyList<RentalApplication>> GetMyApplicationsAsync(
        ClaimsPrincipal user,
        ApplicationStatus? status,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var applicantId = RequireRoleAndUserId(user, RoleNames.Applicant);
        var query = WithListIncludes(dbContext.RentalApplications.AsNoTracking())
            .Where(application => application.ApplicantUserId == applicantId);

        if (status.HasValue)
        {
            query = query.Where(application => application.StatusId == (byte)status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(application => application.Unit.UnitNumber.Contains(term)
                || application.Unit.Property.Name.Contains(term)
                || application.Unit.Property.City.Contains(term));
        }

        return await query.OrderByDescending(application => application.UpdatedUtc)
            .ToListAsync(cancellationToken);
    }






    public async Task<ApplicantDashboardViewModel> GetApplicantDashboardAsync(
        ClaimsPrincipal user,
        ApplicantDashboardViewModel filters,
        CancellationToken cancellationToken = default)
    {
        var applicantId = RequireRoleAndUserId(user, RoleNames.Applicant);
        var applicantApplications = dbContext.RentalApplications.AsNoTracking()
            .Where(application => application.ApplicantUserId == applicantId);

        var appData = await applicantApplications
            .Select(application => new
            {
                application.UnitId,
                PropertyId = application.Unit.PropertyId,
                PropertyName = application.Unit.Property.Name,
                PropertyCity = application.Unit.Property.City,
                application.Unit.UnitNumber
            })
            .ToListAsync(cancellationToken);

        filters.Properties = appData
            .Select(x => new ApplicantPropertyOption(x.PropertyId, x.PropertyName, x.PropertyCity))
            .Distinct()
            .OrderBy(property => property.Name)
            .ThenBy(property => property.City)
            .ToList();

        filters.Units = appData
            .Select(x => new ApplicantUnitOption(x.UnitId, x.PropertyId, x.UnitNumber))
            .Distinct()
            .OrderBy(unit => unit.UnitNumber)
            .ToList();

        var query = WithListIncludes(applicantApplications);
        if (filters.Status.HasValue)
        {
            query = query.Where(application => application.StatusId == (byte)filters.Status.Value);
        }

        if (filters.MoveInFrom.HasValue)
        {
            query = query.Where(application => application.DesiredMoveInDate.HasValue
                && application.DesiredMoveInDate.Value >= filters.MoveInFrom.Value);
        }

        if (filters.MoveInTo.HasValue)
        {
            query = query.Where(application => application.DesiredMoveInDate.HasValue
                && application.DesiredMoveInDate.Value <= filters.MoveInTo.Value);
        }

        if (filters.PropertyId.HasValue)
        {
            query = query.Where(application => application.Unit.PropertyId == filters.PropertyId.Value);
        }

        if (filters.UnitId.HasValue)
        {
            query = query.Where(application => application.UnitId == filters.UnitId.Value);
        }

        filters.Applications = await query
            .OrderByDescending(application => application.UpdatedUtc)
            .ToListAsync(cancellationToken);
        return filters;
    }



    public async Task<IReadOnlyList<RentalApplication>> GetReviewQueueAsync(
        ClaimsPrincipal user,
        ApplicationStatus? status,
        string? search,
        CancellationToken cancellationToken = default)
    {
        RequireRoleAndUserId(user, RoleNames.PropertyManager);
        var query = WithListIncludes(dbContext.RentalApplications.AsNoTracking());

        if (status.HasValue)
        {
            query = query.Where(application => application.StatusId == (byte)status.Value);
        }
        else
        {
            query = query.Where(application => application.StatusId == (byte)ApplicationStatus.Submitted
                || application.StatusId == (byte)ApplicationStatus.UnderReview);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(application => application.Unit.UnitNumber.Contains(term)
                || application.Unit.Property.Name.Contains(term)
                || application.Unit.Property.City.Contains(term)
                || application.ApplicantUserId.Contains(term));
        }

        return await query.OrderBy(application => application.StatusId)
            .ThenBy(application => application.SubmittedUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<RentalApplication?> GetMyApplicationAsync(
        ClaimsPrincipal user,
        int applicationId,
        CancellationToken cancellationToken = default)
    {
        var applicantId = RequireRoleAndUserId(user, RoleNames.Applicant);
        return await WithDetails(dbContext.RentalApplications.AsNoTracking())
            .FirstOrDefaultAsync(
                application => application.Id == applicationId && application.ApplicantUserId == applicantId,
                cancellationToken);
    }

    public async Task<RentalApplication?> GetApplicationForReviewAsync(
        ClaimsPrincipal user,
        int applicationId,
        CancellationToken cancellationToken = default)
    {
        RequireRoleAndUserId(user, RoleNames.PropertyManager);
        return await WithDetails(dbContext.RentalApplications.AsNoTracking())
            .FirstOrDefaultAsync(application => application.Id == applicationId, cancellationToken);
    }

    public async Task<RentalApplication> CreateDraftAsync(
        ClaimsPrincipal user,
        RentalApplicationFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        var applicantId = RequireRoleAndUserId(user, RoleNames.Applicant);
        ValidateApplicantFields(model);
        var desiredMoveInDate = model.DesiredMoveInDate!.Value;
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        if (!await propertyManagementService.IsAvailableForApplicationAsync(
                model.UnitId, desiredMoveInDate, cancellationToken))
        {
            throw new ApplicationWorkflowException("This unit is no longer available for the requested move-in date.");
        }

        var duplicateExists = await dbContext.RentalApplications.AnyAsync(application =>
            application.ApplicantUserId == applicantId
            && application.UnitId == model.UnitId
            && OpenStatuses.Contains(application.StatusId), cancellationToken);
        if (duplicateExists)
        {
            throw new ApplicationWorkflowException("You already have an open application for this unit.");
        }

        var now = DateTime.UtcNow;
        var application = new RentalApplication
        {
            ApplicantUserId = applicantId,
            UnitId = model.UnitId,
            StatusId = (byte)ApplicationStatus.Draft,
            DesiredMoveInDate = desiredMoveInDate,
            HouseholdSize = model.HouseholdSize,
            MonthlyIncome = model.MonthlyIncome,
            ApplicantMessage = NormalizeNotes(model.ApplicantMessage),
            CreatedUtc = now,
            UpdatedUtc = now
        };
        application.Activities.Add(CreateActivity(
            applicantId, RentalApplicationActivityType.DraftCreated, now));
        dbContext.RentalApplications.Add(application);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return application;
    }

    public async Task<bool> UpdateDraftAsync(
        ClaimsPrincipal user,
        int applicationId,
        RentalApplicationFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        var applicantId = RequireRoleAndUserId(user, RoleNames.Applicant);
        ValidateApplicantFields(model);
        var application = await dbContext.RentalApplications.FirstOrDefaultAsync(
            item => item.Id == applicationId && item.ApplicantUserId == applicantId,
            cancellationToken);
        if (application is null)
        {
            return false;
        }

        if (!CanApplicantEdit(application.StatusId))
        {
            throw new ApplicationWorkflowException("Only draft or returned applications can be edited.");
        }

        application.DesiredMoveInDate = model.DesiredMoveInDate;
        application.HouseholdSize = model.HouseholdSize;
        application.MonthlyIncome = model.MonthlyIncome;
        application.ApplicantMessage = NormalizeNotes(model.ApplicantMessage);
        application.UpdatedUtc = DateTime.UtcNow;
        application.Activities.Add(CreateActivity(
            applicantId, RentalApplicationActivityType.DraftUpdated, application.UpdatedUtc));
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> SubmitAsync(
        ClaimsPrincipal user,
        int applicationId,
        CancellationToken cancellationToken = default)
    {
        var applicantId = RequireRoleAndUserId(user, RoleNames.Applicant);
        var application = await dbContext.RentalApplications.FirstOrDefaultAsync(
            item => item.Id == applicationId && item.ApplicantUserId == applicantId,
            cancellationToken);
        if (application is null)
        {
            return false;
        }

        var isResubmission = application.StatusId == (byte)ApplicationStatus.ReturnedForCorrection;
        if (application.StatusId != (byte)ApplicationStatus.Draft && !isResubmission)
        {
            throw new ApplicationWorkflowException("Only drafts or returned applications can be submitted.");
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        if (!application.DesiredMoveInDate.HasValue
            || !await propertyManagementService.IsAvailableForApplicationAsync(
                application.UnitId, application.DesiredMoveInDate.Value, cancellationToken))
        {
            throw new ApplicationWorkflowException("This unit is no longer available for the requested move-in date.");
        }

        var now = DateTime.UtcNow;
        application.StatusId = (byte)ApplicationStatus.Submitted;
        application.SubmittedUtc = now;
        application.UpdatedUtc = now;
        application.Activities.Add(CreateActivity(
            applicantId,
            isResubmission ? RentalApplicationActivityType.Resubmitted : RentalApplicationActivityType.Submitted,
            now));
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<bool> WithdrawAsync(
        ClaimsPrincipal user,
        int applicationId,
        CancellationToken cancellationToken = default)
    {
        var applicantId = RequireRoleAndUserId(user, RoleNames.Applicant);
        var application = await dbContext.RentalApplications.FirstOrDefaultAsync(
            item => item.Id == applicationId && item.ApplicantUserId == applicantId,
            cancellationToken);
        if (application is null)
        {
            return false;
        }

        if (application.StatusId != (byte)ApplicationStatus.Draft
            && application.StatusId != (byte)ApplicationStatus.Submitted
            && application.StatusId != (byte)ApplicationStatus.UnderReview
            && application.StatusId != (byte)ApplicationStatus.ReturnedForCorrection)
        {
            throw new ApplicationWorkflowException("This application can no longer be withdrawn.");
        }

        var now = DateTime.UtcNow;
        application.StatusId = (byte)ApplicationStatus.Withdrawn;
        application.UpdatedUtc = now;
        application.Activities.Add(CreateActivity(applicantId, RentalApplicationActivityType.Withdrawn, now));
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> StartReviewAsync(
        ClaimsPrincipal user,
        int applicationId,
        CancellationToken cancellationToken = default)
    {
        var managerId = RequireRoleAndUserId(user, RoleNames.PropertyManager);
        var application = await dbContext.RentalApplications
            .Include(item => item.Unit)
                .ThenInclude(unit => unit.Property)
            .FirstOrDefaultAsync(item => item.Id == applicationId, cancellationToken);
        if (application is null)
        {
            return false;
        }

        if (application.StatusId != (byte)ApplicationStatus.Submitted)
        {
            throw new ApplicationWorkflowException("Only submitted applications can enter review.");
        }

        var now = DateTime.UtcNow;
        application.StatusId = (byte)ApplicationStatus.UnderReview;
        application.UpdatedUtc = now;
        application.Activities.Add(CreateActivity(managerId, RentalApplicationActivityType.ReviewStarted, now));
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ApproveAsync(
        ClaimsPrincipal user,
        int applicationId,
        string notes,
        CancellationToken cancellationToken = default)
    {
        var managerId = RequireRoleAndUserId(user, RoleNames.PropertyManager);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);

        var application = await dbContext.RentalApplications
            .Include(item => item.Unit)
                .ThenInclude(unit => unit.Property)
            .FirstOrDefaultAsync(item => item.Id == applicationId, cancellationToken);
        if (application is null)
        {
            return false;
        }

        if (application.StatusId != (byte)ApplicationStatus.UnderReview)
        {
            throw new ApplicationWorkflowException("Only applications under review can be approved.");
        }

        var now = DateTime.UtcNow;
        await leaseManagementService.CreateForApprovedApplicationAsync(
            application, managerId, notes, now, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RejectAsync(
        ClaimsPrincipal user,
        int applicationId,
        string notes,
        CancellationToken cancellationToken = default)
    {
        var managerId = RequireRoleAndUserId(user, RoleNames.PropertyManager);
        var normalizedNotes = NormalizeReviewNotes(notes);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);

        var application = await dbContext.RentalApplications.FirstOrDefaultAsync(
            item => item.Id == applicationId, cancellationToken);
        if (application is null)
        {
            return false;
        }

        if (application.StatusId != (byte)ApplicationStatus.UnderReview)
        {
            throw new ApplicationWorkflowException("Only applications under review can be rejected.");
        }

        var now = DateTime.UtcNow;
        application.StatusId = (byte)ApplicationStatus.Rejected;
        application.UpdatedUtc = now;
        dbContext.ApplicationReviews.Add(new ApplicationReview
        {
            ApplicationId = application.Id,
            ReviewerUserId = managerId,
            Decision = ReviewDecision.Rejected,
            Notes = normalizedNotes,
            CreatedUtc = now
        });
        application.Activities.Add(CreateActivity(
            managerId, RentalApplicationActivityType.Rejected, now, normalizedNotes));

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ReturnForCorrectionAsync(
        ClaimsPrincipal user,
        int applicationId,
        string notes,
        CancellationToken cancellationToken = default)
    {
        var managerId = RequireRoleAndUserId(user, RoleNames.PropertyManager);
        var normalizedNotes = notes.Trim();
        if (normalizedNotes.Length == 0 || normalizedNotes.Length > 2000)
        {
            throw new ApplicationWorkflowException("Review notes are required and must not exceed 2000 characters.");
        }

        var application = await dbContext.RentalApplications.FirstOrDefaultAsync(
            item => item.Id == applicationId, cancellationToken);
        if (application is null)
        {
            return false;
        }

        if (application.StatusId != (byte)ApplicationStatus.Submitted
            && application.StatusId != (byte)ApplicationStatus.UnderReview)
        {
            throw new ApplicationWorkflowException("Only submitted applications can be returned for correction.");
        }

        var now = DateTime.UtcNow;
        application.StatusId = (byte)ApplicationStatus.ReturnedForCorrection;
        application.UpdatedUtc = now;
        dbContext.ApplicationReviews.Add(new ApplicationReview
        {
            ApplicationId = applicationId,
            ReviewerUserId = managerId,
            Decision = ReviewDecision.ReturnedForCorrection,
            Notes = normalizedNotes,
            CreatedUtc = now
        });
        application.Activities.Add(CreateActivity(
            managerId, RentalApplicationActivityType.ReturnedForCorrection, now, normalizedNotes));
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static IQueryable<RentalApplication> WithListIncludes(IQueryable<RentalApplication> query) =>
        query.Include(application => application.Unit)
            .ThenInclude(unit => unit.Property)
            .Include(application => application.Status);

    private static IQueryable<RentalApplication> WithDetails(IQueryable<RentalApplication> query) =>
        WithListIncludes(query)
            .Include(application => application.Lease)
                .ThenInclude(lease => lease!.Status)
            .Include(application => application.ApplicationReviews.OrderByDescending(review => review.CreatedUtc))
            .Include(application => application.Activities.OrderByDescending(activity => activity.CreatedUtc));

    private static bool CanApplicantEdit(byte statusId) =>
        statusId == (byte)ApplicationStatus.Draft
        || statusId == (byte)ApplicationStatus.ReturnedForCorrection;

    private static RentalApplicationActivity CreateActivity(
        string actorUserId,
        RentalApplicationActivityType type,
        DateTime createdUtc,
        string? notes = null) => new()
    {
        ActorUserId = actorUserId,
        Type = type,
        CreatedUtc = createdUtc,
        Notes = notes
    };

    private static void ValidateApplicantFields(RentalApplicationFormViewModel model)
    {
        if (!model.DesiredMoveInDate.HasValue
            || model.DesiredMoveInDate.Value < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ApplicationWorkflowException("Move-in date must be today or later.");
        }

        if (model.HouseholdSize == 0)
        {
            throw new ApplicationWorkflowException("Household size must be at least one.");
        }

        if (model.MonthlyIncome < 0 || model.ApplicantMessage?.Length > 2000)
        {
            throw new ApplicationWorkflowException("Income cannot be negative and message cannot exceed 2000 characters.");
        }
    }

    private static string RequireRoleAndUserId(ClaimsPrincipal user, string role)
    {
        if (!user.IsInRole(role))
        {
            throw new UnauthorizedAccessException($"Role '{role}' is required for this application operation.");
        }

        return user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Authenticated user has no identifier.");
    }

    private static string? NormalizeNotes(string? notes) =>
        string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();

    private static string NormalizeReviewNotes(string notes)
    {
        var normalizedNotes = notes.Trim();
        if (normalizedNotes.Length == 0 || normalizedNotes.Length > 2000)
        {
            throw new ApplicationWorkflowException("Review notes are required and must not exceed 2000 characters.");
        }

        return normalizedNotes;
    }
}

public sealed class ApplicationWorkflowException(string message) : InvalidOperationException(message);