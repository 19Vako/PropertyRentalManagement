using PropertyRental.Domain.Entities;

namespace PropertyRental.Web.Models;

public sealed class RentalApplicationDetailsViewModel
{
    public required RentalApplication Application { get; init; }

    public bool IsManager { get; init; }

    public bool CanEdit { get; init; }

    public bool CanSubmit { get; init; }

    public bool CanWithdraw { get; init; }

    public bool CanStartReview { get; init; }

    public bool CanApprove { get; init; }

    public bool CanReject { get; init; }

    public bool CanReturnForCorrection { get; init; }
}