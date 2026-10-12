using System.ComponentModel.DataAnnotations;

namespace PropertyRental.Web.Models;

public sealed class RentalApplicationFormViewModel
{
    public int Id { get; set; }

    public int UnitId { get; set; }

    public string UnitLabel { get; set; } = string.Empty;

    [Required, DataType(DataType.Date)]
    public DateOnly? DesiredMoveInDate { get; set; }

    [Range(1, byte.MaxValue)]
    public byte HouseholdSize { get; set; } = 1;

    [Range(typeof(decimal), "0", "9999999999.99")]
    public decimal? MonthlyIncome { get; set; }

    [StringLength(2000)]
    public string? ApplicantMessage { get; set; }
}