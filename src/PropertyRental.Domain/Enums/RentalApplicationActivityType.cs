namespace PropertyRental.Domain.Enums;

public enum RentalApplicationActivityType : byte
{
    DraftCreated = 1,
    DraftUpdated = 2,
    Submitted = 3,
    ReviewStarted = 4,
    ReturnedForCorrection = 5,
    Resubmitted = 6,
    Withdrawn = 7,
    Approved = 8,
    Rejected = 9
}