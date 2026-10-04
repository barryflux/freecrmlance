namespace Freecrmlance.Application.Audits;
public sealed record AuditReportTransmissionDto(Guid Id,Guid ReportVersionId,int VersionNumber,string Recipient,string SentByUserId,DateTime SentAtUtc);
public sealed record CreateAuditTransmissionResult(Guid? TransmissionId,string? Token,string? Error){public bool Succeeded=>TransmissionId.HasValue&&Token is not null&&Error is null;}
public interface IAuditTransmissionService
{
 Task<IReadOnlyList<AuditReportTransmissionDto>?> ListAsync(Guid auditId,CancellationToken ct=default);
 Task<CreateAuditTransmissionResult> CreateAsync(Guid auditId,Guid versionId,string recipient,string sentByUserId,CancellationToken ct=default);
}
