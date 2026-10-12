namespace PropertyRental.Domain.Entities;

public sealed class RentalApplication
{
    public int Id { get; set; }

    public string ApplicantUserId { get; set; } = string.Empty;

    public int UnitId { get; set; }

    public byte StatusId { get; set; }

    public DateOnly? DesiredMoveInDate { get; set; }

    public byte HouseholdSize { get; set; }

    public decimal? MonthlyIncome { get; set; }

    public string? ApplicantMessage { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime? SubmittedUtc { get; set; }

    public DateTime UpdatedUtc { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Unit Unit { get; set; } = null!;

    public ApplicationStatusRecord Status { get; set; } = null!;

    public ICollection<ApplicationReview> ApplicationReviews { get; set; } = new List<ApplicationReview>();

    public ICollection<RentalApplicationActivity> Activities { get; set; } = new List<RentalApplicationActivity>();

    public Lease? Lease { get; set; }
}