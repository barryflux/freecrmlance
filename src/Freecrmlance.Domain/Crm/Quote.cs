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

    public void UpdateDraft(string number, IEnumerable<(string Description, decimal Quantity, decimal UnitPrice)> newLines)
    {
        if (Status != QuoteStatus.Draft) throw new InvalidOperationException("Only draft quotes can be edited.");
        if (string.IsNullOrWhiteSpace(number)) throw new ArgumentException("Quote number is required.", nameof(number));

        var replacementLines = newLines
            .Select(line => new QuoteLine(Id, line.Description, line.Quantity, line.UnitPrice))
            .ToList();

        Number = number.Trim();
        lines.Clear();
        lines.AddRange(replacementLines);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkSent()
    {
        if (Status != QuoteStatus.Draft) throw new InvalidOperationException("Only draft quotes can be marked as sent.");
        ChangeStatus(QuoteStatus.Sent);
    }

    public void Accept()
    {
        if (Status != QuoteStatus.Sent) throw new InvalidOperationException("Only sent quotes can be accepted.");
        ChangeStatus(QuoteStatus.Accepted);
    }

    public void Reject()
    {
        if (Status != QuoteStatus.Sent) throw new InvalidOperationException("Only sent quotes can be rejected.");
        ChangeStatus(QuoteStatus.Rejected);
    }

    private void ChangeStatus(QuoteStatus status)
    {
        Status = status;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
