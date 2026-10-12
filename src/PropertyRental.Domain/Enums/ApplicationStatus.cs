namespace PropertyRental.Domain.Enums;

public enum ApplicationStatus : byte
{
    Draft = 1,
    Submitted = 2,
    UnderReview = 3,
    ReturnedForCorrection = 4,
    Approved = 5,
    Rejected = 6,
    Withdrawn = 7
}