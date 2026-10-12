using PropertyRental.Domain.Enums;

namespace PropertyRental.Domain.Entities;

public sealed class ApplicationReview
{
    public int Id { get; set; }

    public int ApplicationId { get; set; }

    public string ReviewerUserId { get; set; } = string.Empty;

    public ReviewDecision Decision { get; set; }

    public string Notes { get; set; } = string.Empty;

    public DateTime CreatedUtc { get; set; }

    public RentalApplication Application { get; set; } = null!;
}