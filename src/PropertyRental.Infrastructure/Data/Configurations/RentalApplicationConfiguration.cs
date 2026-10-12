using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyRental.Domain.Entities;
using PropertyRental.Infrastructure.Identity;

namespace PropertyRental.Infrastructure.Data.Configurations;

public sealed class RentalApplicationConfiguration : IEntityTypeConfiguration<RentalApplication>
{
    public void Configure(EntityTypeBuilder<RentalApplication> builder)
    {
        builder.ToTable("RentalApplications", "dbo", table =>
        {
            table.HasCheckConstraint("CK_RentalApplications_HouseholdSize_Positive", "[HouseholdSize] > 0");
            table.HasCheckConstraint("CK_RentalApplications_MonthlyIncome_NonNegative", "[MonthlyIncome] IS NULL OR [MonthlyIncome] >= 0");
        });
        builder.HasKey(application => application.Id);
        builder.Property(application => application.Id).UseIdentityColumn();
        builder.Property(application => application.ApplicantUserId).HasMaxLength(450).IsRequired();
        builder.Property(application => application.StatusId).HasColumnType("tinyint").IsRequired();
        builder.Property(application => application.DesiredMoveInDate).HasColumnType("date");
        builder.Property(application => application.HouseholdSize).HasColumnType("tinyint").IsRequired();
        builder.Property(application => application.MonthlyIncome).HasPrecision(12, 2);
        builder.Property(application => application.ApplicantMessage).HasMaxLength(2000);
        builder.Property(application => application.CreatedUtc).HasColumnType("datetime2").IsRequired();
        builder.Property(application => application.SubmittedUtc).HasColumnType("datetime2");
        builder.Property(application => application.UpdatedUtc).HasColumnType("datetime2").IsRequired();
        builder.Property(application => application.RowVersion).IsRowVersion();
        builder.HasIndex(application => new { application.ApplicantUserId, application.CreatedUtc })
            .IsDescending(false, true);
        builder.HasIndex(application => new { application.StatusId, application.CreatedUtc });
        builder.HasIndex(application => new { application.UnitId, application.StatusId });
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(application => application.ApplicantUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(application => application.Status)
            .WithMany(status => status.RentalApplications)
            .HasForeignKey(application => application.StatusId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(application => application.Lease)
            .WithOne(lease => lease.Application)
            .HasForeignKey<Lease>(lease => lease.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}