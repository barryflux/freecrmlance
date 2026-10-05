using Freecrmlance.Application.Audits;using Freecrmlance.Application.Platform;using Freecrmlance.Domain.Audits;using Freecrmlance.Infrastructure.Persistence;using Microsoft.EntityFrameworkCore;
namespace Freecrmlance.Infrastructure.Audits;
public sealed class AuditHistoryService(FreecrmlanceDbContext db,IWorkspaceContext workspace):IAuditHistoryService
{
 public async Task<IReadOnlyList<AuditHistoryEntryDto>?> ListAsync(Guid auditId,CancellationToken ct=default)
 {
  var wid=await workspace.RequireCurrentWorkspaceIdAsync(ct);var audit=await db.Audits.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==auditId&&x.WorkspaceId==wid,ct);if(audit is null)return null;
  var versions=await db.AuditReportVersions.AsNoTracking().Where(x=>x.AuditId==auditId).Select(x=>new{x.Id,x.VersionNumber,x.FinalizedAtUtc,x.FinalizedByUserId}).ToListAsync(ct);var numbers=versions.ToDictionary(x=>x.Id,x=>x.VersionNumber);
  var transmissions=await db.AuditReportTransmissions.AsNoTracking().Where(x=>x.AuditId==auditId&&x.WorkspaceId==wid).ToListAsync(ct);var events=await db.AuditHistoryEvents.AsNoTracking().Where(x=>x.AuditId==auditId&&x.WorkspaceId==wid).ToListAsync(ct);
  var result=new List<AuditHistoryEntryDto>{new(audit.CreatedAtUtc,"Created","Audit créé",null,null,null)};
  if(audit.StartedAtUtc.HasValue)result.Add(new(audit.StartedAtUtc.Value,"Started","Audit démarré",null,null,null));
  result.AddRange(versions.Select(v=>new AuditHistoryEntryDto(v.FinalizedAtUtc,"Finalized",$"Version {v.VersionNumber} finalisée",v.FinalizedByUserId,v.VersionNumber,null)));
  result.AddRange(transmissions.Select(t=>new AuditHistoryEntryDto(t.SentAtUtc,"Transmitted",$"Version {(numbers.TryGetValue(t.ReportVersionId,out var n)?n:0)} transmise",t.SentByUserId,numbers.TryGetValue(t.ReportVersionId,out var vn)?vn:null,t.Recipient)));
  result.AddRange(events.Select(e=>new AuditHistoryEntryDto(e.OccurredAtUtc,e.Type.ToString(),Label(e.Type,e.ReportVersionId.HasValue&&numbers.TryGetValue(e.ReportVersionId.Value,out var n)?n:null),e.UserId,e.ReportVersionId.HasValue&&numbers.TryGetValue(e.ReportVersionId.Value,out var vn)?vn:null,null)));
  return result.OrderByDescending(x=>x.OccurredAtUtc).ToArray();
 }
 private static string Label(AuditHistoryEventType type,int? version)=>type switch{AuditHistoryEventType.Reopened=>"Audit rouvert pour correction",AuditHistoryEventType.ReportGenerated=>$"PDF de la version {version} généré",AuditHistoryEventType.ReportRegenerated=>$"PDF de la version {version} régénéré",_=>type.ToString()};
}
