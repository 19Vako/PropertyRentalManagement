using PropertyRental.Domain.Entities;
using PropertyRental.Domain.Enums;

namespace PropertyRental.Web.Models;

public sealed class ManagerDashboardViewModel
{
    public ApplicationStatus? Status { get; set; }

    public int? PropertyId { get; set; }

    public int? UnitId { get; set; }

    public string? Applicant { get; set; }

    public string SortBy { get; set; } = "submitted";

    public int Page { get; set; } = 1;

    public const int PageSize = 25;

    public int TotalCount { get; set; }

    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));

    public IReadOnlyList<RentalApplication> Applications { get; set; } = [];

    public IReadOnlyList<ManagerPropertyOption> Properties { get; set; } = [];

    public IReadOnlyList<ManagerUnitOption> Units { get; set; } = [];

    public int PendingCount { get; set; }

    public int ApprovedCount { get; set; }

    public int RejectedCount { get; set; }

    public int ReturnedCount { get; set; }

    public int ActiveLeaseCount { get; set; }

    public int ActivePropertyCount { get; set; }

    public int AvailableUnitCount { get; set; }
}

public sealed record ManagerPropertyOption(int Id, string Name, string City);

public sealed record ManagerUnitOption(int Id, int PropertyId, string PropertyName, string UnitNumber);