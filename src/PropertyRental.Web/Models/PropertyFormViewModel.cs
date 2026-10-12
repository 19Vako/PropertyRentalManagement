using System.ComponentModel.DataAnnotations;

namespace PropertyRental.Web.Models;

public sealed class PropertyFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string StreetAddress { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Region { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string PostalCode { get; set; } = string.Empty;

    [Required, RegularExpression("^[A-Za-z]{2}$", ErrorMessage = "Enter a two-letter country code.")]
    public string CountryCode { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}