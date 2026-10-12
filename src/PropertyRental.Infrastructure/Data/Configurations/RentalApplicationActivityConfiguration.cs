using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyRental.Domain.Entities;
using PropertyRental.Infrastructure.Identity;

namespace PropertyRental.Infrastructure.Data.Configurations;

public sealed class RentalApplicationActivityConfiguration : IEntityTypeConfiguration<RentalApplicationActivity>
{
    public void Configure(EntityTypeBuilder<RentalApplicationActivity> builder)
    {
        builder.ToTable("RentalApplicationActivities", "dbo");
        builder.HasKey(activity => activity.Id);
        builder.Property(activity => activity.Id).UseIdentityColumn();
        builder.Property(activity => activity.ActorUserId).HasMaxLength(450).IsRequired();
        builder.Property(activity => activity.Type).HasConversion<byte>().HasColumnType("tinyint").IsRequired();
        builder.Property(activity => activity.Notes).HasMaxLength(2000);
        builder.Property(activity => activity.CreatedUtc).HasColumnType("datetime2").IsRequired();
        builder.HasIndex(activity => new { activity.ApplicationId, activity.CreatedUtc });
        builder.HasIndex(activity => activity.ActorUserId);
        builder.HasOne(activity => activity.Application)
            .WithMany(application => application.Activities)
            .HasForeignKey(activity => activity.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(activity => activity.ActorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}