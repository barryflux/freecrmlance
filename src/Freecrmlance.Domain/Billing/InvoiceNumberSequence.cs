namespace Freecrmlance.Domain.Billing;

public sealed class InvoiceNumberSequence
{
    private InvoiceNumberSequence() { }

    public InvoiceNumberSequence(Guid workspaceId, int year)
    {
        if (workspaceId == Guid.Empty) throw new ArgumentException("Workspace is required.", nameof(workspaceId));
        if (year < 2000 || year > 9999) throw new ArgumentOutOfRangeException(nameof(year));
        WorkspaceId = workspaceId;
        Year = year;
    }

    public Guid WorkspaceId { get; private set; }
    public int Year { get; private set; }
    public int LastNumber { get; private set; }

    public int Next()
    {
        checked { LastNumber++; }
        return LastNumber;
    }
}
