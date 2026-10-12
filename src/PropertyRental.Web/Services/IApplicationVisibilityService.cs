using System.Security.Claims;
using PropertyRental.Domain.Entities;

namespace PropertyRental.Web.Services;

public interface IApplicationVisibilityService
{
    Task<IReadOnlyList<RentalApplication>> GetVisibleApplicationsAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default);
}