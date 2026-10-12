namespace PropertyRental.Domain.Entities;

public sealed class Lease
{
    public int Id { get; set; }

    public int ApplicationId { get; set; }

    public string TenantUserId { get; set; } = string.Empty;

    public int UnitId { get; set; }

    public byte StatusId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public decimal MonthlyRent { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime? ClosedUtc { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public RentalApplication Application { get; set; } = null!;

    public Unit Unit { get; set; } = null!;

    public LeaseStatusRecord Status { get; set; } = null!;
}