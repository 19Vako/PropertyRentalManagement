namespace PropertyRental.Domain.Entities;

public sealed class Property
{
    public int Id { get; set; }

    public string ManagerUserId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string StreetAddress { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string Region { get; set; } = string.Empty;

    public string PostalCode { get; set; } = string.Empty;

    public string CountryCode { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime CreatedUtc { get; set; }

    public bool IsActive { get; set; }

    public ICollection<Unit> Units { get; set; } = new List<Unit>();
}