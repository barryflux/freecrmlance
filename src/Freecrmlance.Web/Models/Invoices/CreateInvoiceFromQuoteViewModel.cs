using System.ComponentModel.DataAnnotations;

namespace Freecrmlance.Web.Models.Invoices;

public sealed class CreateInvoiceFromQuoteViewModel
{
    [Required, StringLength(50)]
    public string Number { get; set; } = string.Empty;
}
