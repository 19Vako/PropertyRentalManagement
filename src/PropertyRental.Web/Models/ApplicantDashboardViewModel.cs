using PropertyRental.Domain.Entities;
using PropertyRental.Domain.Enums;

namespace PropertyRental.Web.Models;

public sealed class ApplicantDashboardViewModel
{
    public ApplicationStatus? Status { get; set; }

    public DateOnly? MoveInFrom { get; set; }

    public DateOnly? MoveInTo { get; set; }

    public int? PropertyId { get; set; }

    public int? UnitId { get; set; }

    public IReadOnlyList<RentalApplication> Applications { get; set; } = [];

    public IReadOnlyList<ApplicantPropertyOption> Properties { get; set; } = [];

    public IReadOnlyList<ApplicantUnitOption> Units { get; set; } = [];
}

public sealed record ApplicantPropertyOption(int Id, string Name, string City);

public sealed record ApplicantUnitOption(int Id, int PropertyId, string UnitNumber);