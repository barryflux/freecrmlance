namespace Freecrmlance.Domain.Billing;

public sealed class Invoice
{
    private readonly List<InvoiceLine> lines = [];

    private Invoice() { }

    public Invoice(Guid workspaceId, Guid customerId, string number, Guid? sourceQuoteId = null)
    {
        if (workspaceId == Guid.Empty) throw new ArgumentException("Workspace is required.", nameof(workspaceId));
        if (customerId == Guid.Empty) throw new ArgumentException("Customer is required.", nameof(customerId));
        if (string.IsNullOrWhiteSpace(number)) throw new ArgumentException("Invoice number is required.", nameof(number));
        if (sourceQuoteId == Guid.Empty) throw new ArgumentException("Source quote cannot be empty.", nameof(sourceQuoteId));

        Id = Guid.NewGuid();
        WorkspaceId = workspaceId;
        CustomerId = customerId;
        Number = number.Trim();
        SourceQuoteId = sourceQuoteId;
        Status = InvoiceStatus.Draft;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid WorkspaceId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid? SourceQuoteId { get; private set; }
    public string Number { get; private set; } = string.Empty;
    public InvoiceStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<InvoiceLine> Lines => lines;
    public decimal Total => lines.Sum(line => line.Total);

    public void AddLine(string description, decimal quantity, decimal unitPrice)
    {
        lines.Add(new InvoiceLine(Id, description, quantity, unitPrice));
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
