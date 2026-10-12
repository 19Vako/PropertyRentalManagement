using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyRental.Domain.Entities;

namespace PropertyRental.Infrastructure.Data.Configurations;

public sealed class ApplicationStatusConfiguration : IEntityTypeConfiguration<ApplicationStatusRecord>
{
    public void Configure(EntityTypeBuilder<ApplicationStatusRecord> builder)
    {
        builder.ToTable("ApplicationStatus", "dbo");
        builder.HasKey(status => status.Id);
        builder.Property(status => status.Id).HasColumnType("tinyint").ValueGeneratedNever();
        builder.Property(status => status.Name).HasMaxLength(40).IsRequired();
        builder.HasIndex(status => status.Name).IsUnique();
        builder.HasData(
            new ApplicationStatusRecord { Id = 1, Name = "Draft" },
            new ApplicationStatusRecord { Id = 2, Name = "Submitted" },
            new ApplicationStatusRecord { Id = 3, Name = "UnderReview" },
            new ApplicationStatusRecord { Id = 4, Name = "ReturnedForCorrection" },
            new ApplicationStatusRecord { Id = 5, Name = "Approved" },
            new ApplicationStatusRecord { Id = 6, Name = "Rejected" },
            new ApplicationStatusRecord { Id = 7, Name = "Withdrawn" });
    }
}