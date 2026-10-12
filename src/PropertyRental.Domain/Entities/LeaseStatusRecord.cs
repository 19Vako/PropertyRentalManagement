namespace PropertyRental.Domain.Entities;

public sealed class LeaseStatusRecord
{
    public byte Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<Lease> Leases { get; set; } = new List<Lease>();
}