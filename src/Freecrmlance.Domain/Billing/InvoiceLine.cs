namespace Freecrmlance.Domain.Billing;

public sealed class InvoiceLine
{
    private InvoiceLine() { }

    public InvoiceLine(Guid invoiceId, string description, decimal quantity, decimal unitPrice, decimal vatRate = 0m)
    {
        if (invoiceId == Guid.Empty) throw new ArgumentException("Invoice is required.", nameof(invoiceId));
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description is required.", nameof(description));
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");
        if (unitPrice < 0) throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price cannot be negative.");
        if (vatRate < 0 || vatRate > 100) throw new ArgumentOutOfRangeException(nameof(vatRate), "VAT rate must be between 0 and 100.");

        Id = Guid.NewGuid(); InvoiceId = invoiceId; Description = description.Trim();
        Quantity = quantity; UnitPrice = unitPrice; VatRate = vatRate;
    }

    public Guid Id { get; private set; }
    public Guid InvoiceId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal VatRate { get; private set; }
    public decimal TotalExcludingTax => decimal.Round(Quantity * UnitPrice, 2, MidpointRounding.AwayFromZero);
    public decimal VatAmount => decimal.Round(TotalExcludingTax * VatRate / 100m, 2, MidpointRounding.AwayFromZero);
    public decimal TotalIncludingTax => TotalExcludingTax + VatAmount;
    public decimal Total => TotalIncludingTax;
}
