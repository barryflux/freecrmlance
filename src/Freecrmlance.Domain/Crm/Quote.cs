namespace Freecrmlance.Domain.Crm;

public sealed class Quote
{
    private readonly List<QuoteLine> lines = [];

    private Quote() { }

    public Quote(Guid workspaceId, Guid customerId, string number)
    {
        if (workspaceId == Guid.Empty) throw new ArgumentException("Workspace is required.", nameof(workspaceId));
        if (customerId == Guid.Empty) throw new ArgumentException("Customer is required.", nameof(customerId));
        if (string.IsNullOrWhiteSpace(number)) throw new ArgumentException("Quote number is required.", nameof(number));

        Id = Guid.NewGuid();
        WorkspaceId = workspaceId;
        CustomerId = customerId;
        Number = number.Trim();
        Status = QuoteStatus.Draft;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid WorkspaceId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string Number { get; private set; } = string.Empty;
    public QuoteStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<QuoteLine> Lines => lines;
    public decimal Total => lines.Sum(line => line.Total);

    public void AddLine(string description, decimal quantity, decimal unitPrice)
    {
        lines.Add(new QuoteLine(Id, description, quantity, unitPrice));
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkSent() => ChangeStatus(QuoteStatus.Sent);
    public void Accept() => ChangeStatus(QuoteStatus.Accepted);
    public void Reject() => ChangeStatus(QuoteStatus.Rejected);

    private void ChangeStatus(QuoteStatus status)
    {
        Status = status;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
