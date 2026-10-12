using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyRental.Infrastructure.Identity;
using Property = PropertyRental.Domain.Entities.Property;

namespace PropertyRental.Infrastructure.Data.Configurations;

public sealed class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.ToTable("Properties", "dbo");
        builder.HasKey(property => property.Id);
        builder.Property(property => property.Id).UseIdentityColumn();
        builder.Property(property => property.ManagerUserId).HasMaxLength(450).IsRequired();
        builder.Property(property => property.Name).HasMaxLength(150).IsRequired();
        builder.Property(property => property.StreetAddress).HasMaxLength(200).IsRequired();
        builder.Property(property => property.City).HasMaxLength(100).IsRequired();
        builder.Property(property => property.Region).HasMaxLength(100).IsRequired();
        builder.Property(property => property.PostalCode).HasMaxLength(20).IsRequired();
        builder.Property(property => property.CountryCode).HasColumnType("char(2)").IsRequired();
        builder.Property(property => property.Description).HasMaxLength(2000);
        builder.Property(property => property.CreatedUtc).HasColumnType("datetime2").IsRequired();
        builder.Property(property => property.IsActive).IsRequired();
        builder.HasIndex(property => new { property.ManagerUserId, property.IsActive });
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(property => property.ManagerUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(property => property.Units)
            .WithOne(unit => unit.Property)
            .HasForeignKey(unit => unit.PropertyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}