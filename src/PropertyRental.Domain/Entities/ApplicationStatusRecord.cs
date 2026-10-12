namespace PropertyRental.Domain.Entities;

public sealed class ApplicationStatusRecord
{
    public byte Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<RentalApplication> RentalApplications { get; set; } = new List<RentalApplication>();
}