using System.ComponentModel.DataAnnotations;

namespace Freecrmlance.Web.Models.Quotes;

public sealed class QuoteFormViewModel
{
    [Required, StringLength(50)]
    public string Number { get; set; } = string.Empty;

    public List<QuoteLineViewModel> Lines { get; set; } = [new()];
}

public sealed class QuoteLineViewModel
{
    [Required, StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.0001", "99999999999999")]
    public decimal Quantity { get; set; } = 1;

    [Range(typeof(decimal), "0", "99999999999999")]
    public decimal UnitPrice { get; set; }
}
