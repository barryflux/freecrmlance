namespace Freecrmlance.Application.Audits;

public sealed record AuditReportVersionDto(Guid Id, Guid AuditId, int VersionNumber, DateTime FinalizedAtUtc, string FinalizedByUserId, string Hash, Guid? GeneratedDocumentId);
public sealed record FinalizeAuditResult(Guid? VersionId, string? Error)
{
    public bool Succeeded => VersionId.HasValue && Error is null;
}

public interface IAuditFinalizationService
{
    Task<IReadOnlyList<AuditReportVersionDto>?> ListVersionsAsync(Guid auditId, CancellationToken ct = default);
    Task<AuditReportVersionDto?> GetVersionAsync(Guid auditId, Guid versionId, CancellationToken ct = default);
    Task<FinalizeAuditResult> FinalizeAsync(Guid auditId, string finalizedByUserId, CancellationToken ct = default);
    Task<string?> ReopenAsync(Guid auditId, string reopenedByUserId, CancellationToken ct = default);
    Task<bool> VerifyHashAsync(Guid auditId, Guid versionId, CancellationToken ct = default);
}
