namespace Freecrmlance.Domain.Crm;

public sealed class CustomerAction
{
    private CustomerAction() { }
    public CustomerAction(Guid workspaceId, Guid customerId, string title, string? description, DateOnly? dueDate)
    {
        if (workspaceId == Guid.Empty || customerId == Guid.Empty) throw new ArgumentException("Workspace and customer are required.");
        Id = Guid.NewGuid();
        WorkspaceId = workspaceId;
        CustomerId = customerId;
        CreatedAtUtc = DateTime.UtcNow;
        Update(title, description, dueDate);
    }
    public Guid Id { get; private set; }
    public Guid WorkspaceId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public CustomerActionStatus Status { get; private set; } = CustomerActionStatus.Todo;
    public DateOnly? DueDate { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }

    public void Update(string title, string? description, DateOnly? dueDate)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > 200) throw new ArgumentException("Title must be 1-200 characters.", nameof(title));
        if (description?.Trim().Length > 2000) throw new ArgumentException("Description cannot exceed 2000 characters.", nameof(description));
        Title = title.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        DueDate = dueDate;
    }
    public void SetStatus(CustomerActionStatus status)
    {
        if (!Enum.IsDefined(status)) throw new ArgumentOutOfRangeException(nameof(status));
        if (Status == status) return;
        Status = status;
        CompletedAtUtc = status == CustomerActionStatus.Done ? DateTime.UtcNow : null;
    }
}
