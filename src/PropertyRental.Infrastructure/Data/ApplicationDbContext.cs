using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PropertyRental.Domain.Entities;
using PropertyRental.Infrastructure.Identity;

namespace PropertyRental.Infrastructure.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Property> Properties => Set<Property>();

    public DbSet<Unit> Units => Set<Unit>();

    public DbSet<RentalApplication> RentalApplications => Set<RentalApplication>();

    public DbSet<ApplicationStatusRecord> ApplicationStatuses => Set<ApplicationStatusRecord>();

    public DbSet<ApplicationReview> ApplicationReviews => Set<ApplicationReview>();

    public DbSet<RentalApplicationActivity> RentalApplicationActivities => Set<RentalApplicationActivity>();

    public DbSet<Lease> Leases => Set<Lease>();

    public DbSet<LeaseStatusRecord> LeaseStatuses => Set<LeaseStatusRecord>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
