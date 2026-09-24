namespace Freecrmlance.Domain.Crm;

public sealed class Contact
{
    private Contact() { }

    public Contact(Guid workspaceId, Guid customerId, string name, string? email = null, string? phone = null, string? role = null)
    {
        if (workspaceId == Guid.Empty) throw new ArgumentException("Workspace id is required.", nameof(workspaceId));
        if (customerId == Guid.Empty) throw new ArgumentException("Customer id is required.", nameof(customerId));

        Id = Guid.NewGuid();
        WorkspaceId = workspaceId;
        CustomerId = customerId;
        Update(name, email, phone, role);
    }

    public Guid Id { get; private set; }
    public Guid WorkspaceId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Role { get; private set; }

    public void Update(string name, string? email, string? phone, string? role)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Contact name is required.", nameof(name));

        Name = name.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        Role = string.IsNullOrWhiteSpace(role) ? null : role.Trim();
    }
}
