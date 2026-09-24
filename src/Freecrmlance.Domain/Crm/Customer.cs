namespace Freecrmlance.Domain.Crm;

public sealed class Customer
{
    private Customer() { }

    public Customer(Guid workspaceId, string name, string? email = null, string? phone = null)
    {
        if (workspaceId == Guid.Empty)
            throw new ArgumentException("Workspace id is required.", nameof(workspaceId));

        Id = Guid.NewGuid();
        WorkspaceId = workspaceId;
        Update(name, email, phone);
    }

    public Guid Id { get; private set; }
    public Guid WorkspaceId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string? Phone { get; private set; }

    public void Update(string name, string? email, string? phone)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Customer name is required.", nameof(name));

        Name = name.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
    }
}
