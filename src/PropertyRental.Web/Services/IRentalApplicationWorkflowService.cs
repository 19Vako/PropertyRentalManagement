using System.Security.Claims;
using PropertyRental.Domain.Entities;
using PropertyRental.Domain.Enums;
using PropertyRental.Web.Models;

namespace PropertyRental.Web.Services;

public interface IRentalApplicationWorkflowService
{
    Task<IReadOnlyList<RentalApplication>> GetMyApplicationsAsync(
        ClaimsPrincipal user,
        ApplicationStatus? status,
        string? search,
        CancellationToken cancellationToken = default);

    Task<ApplicantDashboardViewModel> GetApplicantDashboardAsync(
        ClaimsPrincipal user,
        ApplicantDashboardViewModel filters,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RentalApplication>> GetReviewQueueAsync(
        ClaimsPrincipal user,
        ApplicationStatus? status,
        string? search,
        CancellationToken cancellationToken = default);

    Task<RentalApplication?> GetMyApplicationAsync(
        ClaimsPrincipal user,
        int applicationId,
        CancellationToken cancellationToken = default);

    Task<RentalApplication?> GetApplicationForReviewAsync(
        ClaimsPrincipal user,
        int applicationId,
        CancellationToken cancellationToken = default);

    Task<RentalApplication> CreateDraftAsync(
        ClaimsPrincipal user,
        RentalApplicationFormViewModel model,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateDraftAsync(
        ClaimsPrincipal user,
        int applicationId,
        RentalApplicationFormViewModel model,
        CancellationToken cancellationToken = default);

    Task<bool> SubmitAsync(ClaimsPrincipal user, int applicationId, CancellationToken cancellationToken = default);

    Task<bool> WithdrawAsync(ClaimsPrincipal user, int applicationId, CancellationToken cancellationToken = default);

    Task<bool> StartReviewAsync(ClaimsPrincipal user, int applicationId, CancellationToken cancellationToken = default);

    Task<bool> ApproveAsync(
        ClaimsPrincipal user,
        int applicationId,
        string notes,
        CancellationToken cancellationToken = default);

    Task<bool> RejectAsync(
        ClaimsPrincipal user,
        int applicationId,
        string notes,
        CancellationToken cancellationToken = default);

    Task<bool> ReturnForCorrectionAsync(
        ClaimsPrincipal user,
        int applicationId,
        string notes,
        CancellationToken cancellationToken = default);
}