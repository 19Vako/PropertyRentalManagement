using System.ComponentModel.DataAnnotations;

namespace PropertyRental.Web.Models;

public sealed class ReviewActionViewModel
{
    public int ApplicationId { get; set; }

    [Required, StringLength(2000)]
    public string Notes { get; set; } = string.Empty;
}