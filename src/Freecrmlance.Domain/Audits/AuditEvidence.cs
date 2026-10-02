namespace Freecrmlance.Domain.Audits;

public sealed class AuditEvidence
{
    private AuditEvidence() { }

    public AuditEvidence(Guid auditItemId, Guid documentId, string uploadedByUserId)
    {
        if (auditItemId == Guid.Empty) throw new ArgumentException("Audit item id is required.", nameof(auditItemId));
        if (documentId == Guid.Empty) throw new ArgumentException("Document id is required.", nameof(documentId));
        if (string.IsNullOrWhiteSpace(uploadedByUserId)) throw new ArgumentException("User id is required.", nameof(uploadedByUserId));
        Id = Guid.NewGuid();
        AuditItemId = auditItemId;
        DocumentId = documentId;
        UploadedByUserId = uploadedByUserId.Trim();
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid AuditItemId { get; private set; }
    public Guid DocumentId { get; private set; }
    public string UploadedByUserId { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }
}
