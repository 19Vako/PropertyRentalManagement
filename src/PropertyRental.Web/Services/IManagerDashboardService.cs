using System.Security.Claims;
using PropertyRental.Web.Models;

namespace PropertyRental.Web.Services;

public interface IManagerDashboardService
{
    Task<ManagerDashboardViewModel> GetDashboardAsync(
        ClaimsPrincipal user,
        ManagerDashboardViewModel filters,
        CancellationToken cancellationToken = default);
}