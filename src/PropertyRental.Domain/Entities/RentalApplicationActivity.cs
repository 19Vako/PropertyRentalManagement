using PropertyRental.Domain.Enums;

namespace PropertyRental.Domain.Entities;

public sealed class RentalApplicationActivity
{
    public int Id { get; set; }

    public int ApplicationId { get; set; }

    public string ActorUserId { get; set; } = string.Empty;

    public RentalApplicationActivityType Type { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedUtc { get; set; }

    public RentalApplication Application { get; set; } = null!;
}