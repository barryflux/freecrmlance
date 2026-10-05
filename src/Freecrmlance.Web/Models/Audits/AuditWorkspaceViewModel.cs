using Freecrmlance.Application.Audits;
namespace Freecrmlance.Web.Models.Audits;
public sealed class AuditWorkspaceViewModel
{
 public required AuditDto Audit{get;init;}
 public required IReadOnlyList<AuditEvidenceDto> Evidence{get;init;}
 public required IReadOnlyList<AuditReportVersionDto> Versions{get;init;}
 public required IReadOnlyList<AuditReportTransmissionDto> Transmissions{get;init;}
 public required IReadOnlyList<AuditHistoryEntryDto> History{get;init;}
}
