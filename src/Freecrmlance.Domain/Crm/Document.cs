namespace Freecrmlance.Domain.Crm;

public sealed class Document
{
    private Document() { }

    public Document(Guid workspaceId, Guid customerId, string fileName, string contentType, long size, string storageKey)
    {
        if (workspaceId == Guid.Empty) throw new ArgumentException("Workspace is required.", nameof(workspaceId));
        if (customerId == Guid.Empty) throw new ArgumentException("Customer is required.", nameof(customerId));
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("File name is required.", nameof(fileName));
        if (size < 0) throw new ArgumentOutOfRangeException(nameof(size));
        if (string.IsNullOrWhiteSpace(storageKey)) throw new ArgumentException("Storage key is required.", nameof(storageKey));

        Id = Guid.NewGuid();
        WorkspaceId = workspaceId;
        CustomerId = customerId;
        FileName = fileName.Trim();
        ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType.Trim();
        Size = size;
        StorageKey = storageKey.Trim();
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid WorkspaceId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long Size { get; private set; }
    public string StorageKey { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }
}
