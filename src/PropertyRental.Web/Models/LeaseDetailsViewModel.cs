using PropertyRental.Domain.Entities;

namespace PropertyRental.Web.Models;

public sealed class LeaseDetailsViewModel
{
    public required Lease Lease { get; init; }

    public string TenantName { get; init; } = string.Empty;

    public string TenantEmail { get; init; } = string.Empty;

    public bool CanTerminate { get; init; }

    public bool CanMarkExpired { get; init; }
}