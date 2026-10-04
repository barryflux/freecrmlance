namespace Freecrmlance.Application.Audits;

public sealed record AuditReportDownload(string FileName, string ContentType, Stream Content);
public sealed record GenerateAuditReportResult(Guid? DocumentId, string? Error)
{
    public bool Succeeded => DocumentId.HasValue && Error is null;
}
public interface IAuditReportService
{
    Task<GenerateAuditReportResult> GenerateAsync(Guid auditId, Guid versionId, CancellationToken ct = default);
    Task<AuditReportDownload?> DownloadAsync(Guid auditId, Guid versionId, CancellationToken ct = default);
}
public interface IAuditReportPdfGenerator
{
    byte[] Generate(AuditReportPdfModel model);
}
public sealed record AuditReportPdfModel(string Reference,string Title,string? Description,int VersionNumber,DateTime FinalizedAtUtc,string Hash,IReadOnlyList<AuditReportSectionPdfModel> Sections);
public sealed record AuditReportSectionPdfModel(string Title,string? Description,IReadOnlyList<AuditReportItemPdfModel> Items);
public sealed record AuditReportItemPdfModel(string Label,string? Description,string? Value,string? Observation,string? Recommendation,IReadOnlyList<string> Evidence);
