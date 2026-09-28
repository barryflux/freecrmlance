namespace Freecrmlance.Domain.Crm;

public sealed class QuoteShare
{
    private QuoteShare() { }

    public QuoteShare(Guid workspaceId, Guid customerId, Guid quoteId, string tokenHash)
    {
        if (workspaceId == Guid.Empty) throw new ArgumentException("Workspace is required.", nameof(workspaceId));
        if (customerId == Guid.Empty) throw new ArgumentException("Customer is required.", nameof(customerId));
        if (quoteId == Guid.Empty) throw new ArgumentException("Quote is required.", nameof(quoteId));
        if (string.IsNullOrWhiteSpace(tokenHash)) throw new ArgumentException("Token hash is required.", nameof(tokenHash));

        Id = Guid.NewGuid();
        WorkspaceId = workspaceId;
        CustomerId = customerId;
        QuoteId = quoteId;
        TokenHash = tokenHash;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid WorkspaceId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid QuoteId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }

    public void Revoke() => RevokedAtUtc ??= DateTime.UtcNow;
}
