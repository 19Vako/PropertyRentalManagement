using PropertyRental.Domain.Enums;

namespace PropertyRental.Domain.Entities;

public sealed class Unit
{
    public int Id { get; set; }

    public int PropertyId { get; set; }

    public string UnitNumber { get; set; } = string.Empty;

    public decimal MonthlyRent { get; set; }

    public byte Bedrooms { get; set; }

    public decimal Bathrooms { get; set; }

    public UnitStatus Status { get; set; }

    public DateOnly? AvailableFrom { get; set; }

    public DateTime CreatedUtc { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Property Property { get; set; } = null!;

    public ICollection<RentalApplication> RentalApplications { get; set; } = new List<RentalApplication>();

    public ICollection<Lease> Leases { get; set; } = new List<Lease>();
}