using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PropertyRental.Domain.Entities;
using PropertyRental.Domain.Identity;
using PropertyRental.Infrastructure.Data;

namespace PropertyRental.Web.Services;

public sealed class ApplicationVisibilityService(ApplicationDbContext dbContext)
    : IApplicationVisibilityService
{
    public async Task<IReadOnlyList<RentalApplication>> GetVisibleApplicationsAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.RentalApplications
            .AsNoTracking()
            .Include(application => application.Unit)
                .ThenInclude(unit => unit.Property)
            .Include(application => application.Status)
            .AsQueryable();

        if (user.IsInRole(RoleNames.PropertyManager))
        {
            return await query
                .OrderByDescending(application => application.CreatedUtc)
                .ToListAsync(cancellationToken);
        }

        if (user.IsInRole(RoleNames.Applicant))
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new UnauthorizedAccessException("The authenticated applicant has no user identifier.");
            }

            return await query
                .Where(application => application.ApplicantUserId == userId)
                .OrderByDescending(application => application.CreatedUtc)
                .ToListAsync(cancellationToken);
        }

        throw new UnauthorizedAccessException("The user does not have an application access role.");
    }
}