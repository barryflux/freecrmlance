namespace Freecrmlance.Domain.Audits;

public sealed class AuditReportVersion
{
    private AuditReportVersion() { }

    public AuditReportVersion(Guid auditId, int versionNumber, DateTime finalizedAtUtc, string finalizedByUserId, string snapshot, string hash)
    {
        if (auditId == Guid.Empty) throw new ArgumentException("Audit id is required.", nameof(auditId));
        if (versionNumber < 1) throw new ArgumentOutOfRangeException(nameof(versionNumber));
        if (string.IsNullOrWhiteSpace(finalizedByUserId)) throw new ArgumentException("User id is required.", nameof(finalizedByUserId));
        if (string.IsNullOrWhiteSpace(snapshot)) throw new ArgumentException("Snapshot is required.", nameof(snapshot));
        if (string.IsNullOrWhiteSpace(hash)) throw new ArgumentException("Hash is required.", nameof(hash));
        Id = Guid.NewGuid(); AuditId = auditId; VersionNumber = versionNumber; FinalizedAtUtc = finalizedAtUtc;
        FinalizedByUserId = finalizedByUserId.Trim(); Snapshot = snapshot; Hash = hash;
    }

    public Guid Id { get; private set; }
    public Guid AuditId { get; private set; }
    public int VersionNumber { get; private set; }
    public DateTime FinalizedAtUtc { get; private set; }
    public string FinalizedByUserId { get; private set; } = string.Empty;
    public string Snapshot { get; private set; } = string.Empty;
    public string Hash { get; private set; } = string.Empty;
    public Guid? GeneratedDocumentId { get; private set; }
    public void SetGeneratedDocument(Guid documentId) { if (documentId == Guid.Empty) throw new ArgumentException("Document id is required.", nameof(documentId)); GeneratedDocumentId = documentId; }
}
