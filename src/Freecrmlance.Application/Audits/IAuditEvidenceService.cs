namespace Freecrmlance.Application.Audits;

public sealed record AuditEvidenceDto(Guid Id, Guid AuditItemId, Guid DocumentId, string FileName, string ContentType, long Size, DateTime CreatedAtUtc);
public sealed record AuditEvidenceDownloadDto(string FileName, string ContentType, Stream Content);

public interface IAuditEvidenceService
{
    Task<IReadOnlyList<AuditEvidenceDto>?> ListAsync(Guid auditId, CancellationToken ct = default);
    Task<string?> UploadAsync(Guid auditId, Guid auditItemId, string fileName, string contentType, long size, Stream content, string uploadedByUserId, CancellationToken ct = default);
    Task<AuditEvidenceDownloadDto?> DownloadAsync(Guid auditId, Guid evidenceId, CancellationToken ct = default);
    Task<string?> DeleteAsync(Guid auditId, Guid evidenceId, CancellationToken ct = default);
}
