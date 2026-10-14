using System.ComponentModel.DataAnnotations;
using PropertyRental.Domain.Enums;

namespace PropertyRental.Web.Models;

public sealed class UnitFormViewModel
{
    public int Id { get; set; }

    [Range(1, int.MaxValue)]
    public int PropertyId { get; set; }

    public string PropertyName { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string UnitNumber { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "9999999999.99")]
    public decimal MonthlyRent { get; set; }

    [Range(0, byte.MaxValue)]
    public byte Bedrooms { get; set; }

     
    [Range(typeof(decimal), "0", "99.9")]
    public decimal? Bathrooms { get; set; }

     
    [EnumDataType(typeof(UnitStatus))]
    public UnitStatus? Status { get; set; }

    [DataType(DataType.Date)]
    public DateOnly? AvailableFrom { get; set; }

 
    public string UnitTypeId { get; set; } = string.Empty;
}