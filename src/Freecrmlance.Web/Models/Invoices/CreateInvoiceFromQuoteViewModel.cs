using System.ComponentModel.DataAnnotations;

namespace Freecrmlance.Web.Models.Invoices;

public sealed class CreateInvoiceFromQuoteViewModel
{
    [Required, StringLength(50)]
    [Display(Name = "Draft reference")]
    public string DraftReference { get; set; } = string.Empty;
}
