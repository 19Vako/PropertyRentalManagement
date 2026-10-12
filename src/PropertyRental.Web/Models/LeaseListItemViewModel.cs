using PropertyRental.Domain.Entities;

namespace PropertyRental.Web.Models;

public sealed class LeaseListItemViewModel
{
    public required Lease Lease { get; init; }

    public string TenantName { get; init; } = string.Empty;

    public string TenantEmail { get; init; } = string.Empty;
}