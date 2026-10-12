using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyRental.Domain.Entities;
using PropertyRental.Infrastructure.Identity;

namespace PropertyRental.Infrastructure.Data.Configurations;

public sealed class ApplicationReviewConfiguration : IEntityTypeConfiguration<ApplicationReview>
{
    public void Configure(EntityTypeBuilder<ApplicationReview> builder)
    {
        builder.ToTable("ApplicationReviews", "dbo");
        builder.HasKey(review => review.Id);
        builder.Property(review => review.Id).UseIdentityColumn();
        builder.Property(review => review.ReviewerUserId).HasMaxLength(450).IsRequired();
        builder.Property(review => review.Decision).HasConversion<byte>().HasColumnType("tinyint").IsRequired();
        builder.Property(review => review.Notes).HasMaxLength(2000).IsRequired();
        builder.Property(review => review.CreatedUtc).HasColumnType("datetime2").IsRequired();
        builder.HasIndex(review => new { review.ApplicationId, review.CreatedUtc })
            .IsDescending(false, true);
        builder.HasIndex(review => review.ReviewerUserId);
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(review => review.ReviewerUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}