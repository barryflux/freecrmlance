using System.ComponentModel.DataAnnotations;

namespace Freecrmlance.Web.Models.Invoices;

public sealed class EditInvoiceViewModel
{
    [Required, StringLength(50)]
    [Display(Name = "Draft reference")]
    public string DraftReference { get; set; } = string.Empty;

    [Display(Name = "Service / supply date")]
    [DataType(DataType.Date)]
    public DateTime? ServiceDate { get; set; }

    [Display(Name = "Due date")]
    [DataType(DataType.Date)]
    public DateTime? DueDate { get; set; }

    [StringLength(100)]
    [Display(Name = "Purchase order / reference")]
    public string? PurchaseOrderReference { get; set; }

    [StringLength(500)]
    [Display(Name = "VAT exemption mention")]
    public string? VatExemptionMention { get; set; }

    [StringLength(500)]
    [Display(Name = "Payment terms")]
    public string? PaymentTerms { get; set; }

    [StringLength(500)]
    [Display(Name = "Early-payment discount terms")]
    public string? EarlyPaymentDiscountTerms { get; set; }

    [StringLength(500)]
    [Display(Name = "Late-payment penalty terms")]
    public string? LatePaymentPenaltyTerms { get; set; }

    [Range(typeof(decimal), "0", "99999999999999", ParseLimitsInInvariantCulture = true)]
    [Display(Name = "Recovery-cost indemnity")]
    public decimal? RecoveryCostIndemnity { get; set; }

    public List<InvoiceLineViewModel> Lines { get; set; } = [new()];
}
