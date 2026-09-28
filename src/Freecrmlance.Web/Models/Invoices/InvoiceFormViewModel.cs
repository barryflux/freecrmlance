using System.ComponentModel.DataAnnotations;

namespace Freecrmlance.Web.Models.Invoices;

public sealed class InvoiceFormViewModel
{
    [Required, StringLength(50)]
    [Display(Name = "Draft reference")]
    public string DraftReference { get; set; } = string.Empty;

    public List<InvoiceLineViewModel> Lines { get; set; } = [new()];
}

public sealed class InvoiceLineViewModel
{
    [Required, StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.0001", "99999999999999", ParseLimitsInInvariantCulture = true, ErrorMessage = "Quantity must be positive.")]
    public decimal Quantity { get; set; } = 1;

    [Range(typeof(decimal), "0", "99999999999999", ParseLimitsInInvariantCulture = true, ErrorMessage = "Unit price cannot be negative.")]
    public decimal UnitPrice { get; set; }

    [Range(typeof(decimal), "0", "100", ParseLimitsInInvariantCulture = true, ErrorMessage = "VAT rate must be between 0 and 100.")]
    public decimal VatRate { get; set; } = 20m;
}
