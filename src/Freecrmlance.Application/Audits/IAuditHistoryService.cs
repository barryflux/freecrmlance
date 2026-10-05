namespace Freecrmlance.Application.Audits;
public sealed record AuditHistoryEntryDto(DateTime OccurredAtUtc,string Type,string Label,string? UserId,int? VersionNumber,string? Recipient);
public interface IAuditHistoryService{Task<IReadOnlyList<AuditHistoryEntryDto>?> ListAsync(Guid auditId,CancellationToken ct=default);}
