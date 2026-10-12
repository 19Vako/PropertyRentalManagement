using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyRental.Domain.Entities;

namespace PropertyRental.Infrastructure.Data.Configurations;

public sealed class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        builder.ToTable("Units", "dbo", table =>
        {
            table.HasCheckConstraint("CK_Units_MonthlyRent_NonNegative", "[MonthlyRent] >= 0");
            table.HasCheckConstraint("CK_Units_Bathrooms_NonNegative", "[Bathrooms] >= 0");
        });
        builder.HasKey(unit => unit.Id);
        builder.Property(unit => unit.Id).UseIdentityColumn();
        builder.Property(unit => unit.UnitNumber).HasMaxLength(30).IsRequired();
        builder.Property(unit => unit.MonthlyRent).HasPrecision(12, 2).IsRequired();
        builder.Property(unit => unit.Bedrooms).HasColumnType("tinyint").IsRequired();
        builder.Property(unit => unit.Bathrooms).HasPrecision(3, 1).IsRequired();
        builder.Property(unit => unit.Status).HasConversion<byte>().HasColumnType("tinyint").IsRequired();
        builder.Property(unit => unit.AvailableFrom).HasColumnType("date");
        builder.Property(unit => unit.CreatedUtc).HasColumnType("datetime2").IsRequired();
        builder.Property(unit => unit.RowVersion).IsRowVersion();
        builder.HasIndex(unit => new { unit.PropertyId, unit.UnitNumber }).IsUnique();
        builder.HasIndex(unit => new { unit.Status, unit.AvailableFrom });
        builder.HasMany(unit => unit.RentalApplications)
            .WithOne(application => application.Unit)
            .HasForeignKey(application => application.UnitId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(unit => unit.Leases)
            .WithOne(lease => lease.Unit)
            .HasForeignKey(lease => lease.UnitId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}