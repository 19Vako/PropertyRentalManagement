using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyRental.Domain.Entities;
using PropertyRental.Infrastructure.Identity;

namespace PropertyRental.Infrastructure.Data.Configurations;

public sealed class LeaseConfiguration : IEntityTypeConfiguration<Lease>
{
    public void Configure(EntityTypeBuilder<Lease> builder)
    {
        builder.ToTable("Leases", "dbo", table =>
        {
            table.HasCheckConstraint("CK_Leases_DateRange", "[EndDate] > [StartDate]");
            table.HasCheckConstraint("CK_Leases_MonthlyRent_NonNegative", "[MonthlyRent] >= 0");
        });
        builder.HasKey(lease => lease.Id);
        builder.Property(lease => lease.Id).UseIdentityColumn();
        builder.Property(lease => lease.TenantUserId).HasMaxLength(450).IsRequired();
        builder.Property(lease => lease.StatusId).HasColumnType("tinyint").IsRequired();
        builder.Property(lease => lease.StartDate).HasColumnType("date").IsRequired();
        builder.Property(lease => lease.EndDate).HasColumnType("date").IsRequired();
        builder.Property(lease => lease.MonthlyRent).HasPrecision(12, 2).IsRequired();
        builder.Property(lease => lease.CreatedUtc).HasColumnType("datetime2").IsRequired();
        builder.Property(lease => lease.ClosedUtc).HasColumnType("datetime2");
        builder.Property(lease => lease.RowVersion).IsRowVersion();
        builder.HasIndex(lease => lease.ApplicationId).IsUnique();
        builder.HasIndex(lease => new { lease.TenantUserId, lease.StatusId });
        builder.HasIndex(lease => lease.StatusId);
        builder.HasIndex(lease => lease.UnitId, "IX_Leases_UnitId");
        builder.HasIndex(lease => lease.UnitId, "UX_Leases_ActiveUnit")
            .IsUnique()
            .HasFilter("[StatusId] = 1");
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(lease => lease.TenantUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(lease => lease.Status)
            .WithMany(status => status.Leases)
            .HasForeignKey(lease => lease.StatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}