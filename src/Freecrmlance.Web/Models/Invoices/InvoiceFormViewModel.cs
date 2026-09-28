using System.ComponentModel.DataAnnotations;

namespace Freecrmlance.Web.Models.Invoices;

public sealed class InvoiceFormViewModel
{
    [Required, StringLength(50)]
    public string Number { get; set; } = string.Empty;

    public List<InvoiceLineViewModel> Lines { get; set; } = [new()];
}

public sealed class InvoiceLineViewModel
{
    [Required, StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.0001", "99999999999999", ErrorMessage = "Quantity must be positive.")]
    public decimal Quantity { get; set; } = 1;

    [Range(typeof(decimal), "0", "99999999999999", ErrorMessage = "Unit price cannot be negative.")]
    public decimal UnitPrice { get; set; }
}
