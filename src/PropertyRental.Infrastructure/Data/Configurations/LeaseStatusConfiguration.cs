using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyRental.Domain.Entities;

namespace PropertyRental.Infrastructure.Data.Configurations;

public sealed class LeaseStatusConfiguration : IEntityTypeConfiguration<LeaseStatusRecord>
{
    public void Configure(EntityTypeBuilder<LeaseStatusRecord> builder)
    {
        builder.ToTable("LeaseStatus", "dbo");
        builder.HasKey(status => status.Id);
        builder.Property(status => status.Id).HasColumnType("tinyint").ValueGeneratedNever();
        builder.Property(status => status.Name).HasMaxLength(40).IsRequired();
        builder.HasIndex(status => status.Name).IsUnique();
        builder.HasData(
            new LeaseStatusRecord { Id = 1, Name = "Active" },
            new LeaseStatusRecord { Id = 2, Name = "Expired" },
            new LeaseStatusRecord { Id = 3, Name = "Terminated" });
    }
}