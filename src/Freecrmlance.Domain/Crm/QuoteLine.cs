namespace Freecrmlance.Domain.Crm;

public sealed class QuoteLine
{
    private QuoteLine() { }

    public QuoteLine(Guid quoteId, string description, decimal quantity, decimal unitPrice)
    {
        if (quoteId == Guid.Empty) throw new ArgumentException("Quote is required.", nameof(quoteId));
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description is required.", nameof(description));
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");
        if (unitPrice < 0) throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price cannot be negative.");

        Id = Guid.NewGuid();
        QuoteId = quoteId;
        Description = description.Trim();
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public Guid Id { get; private set; }
    public Guid QuoteId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Total => Quantity * UnitPrice;
}
