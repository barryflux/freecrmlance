using System.Text.Json;
using Freecrmlance.Application.Audits;
using Freecrmlance.Application.Crm;
using Freecrmlance.Application.Platform;
using Freecrmlance.Domain.Crm;
using Freecrmlance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Freecrmlance.Infrastructure.Audits;

public sealed class AuditReportService(FreecrmlanceDbContext db,IWorkspaceContext workspaceContext,IFileStorage storage,IAuditReportPdfGenerator pdf):IAuditReportService
{
 public async Task<GenerateAuditReportResult> GenerateAsync(Guid auditId,Guid versionId,string generatedByUserId,CancellationToken ct=default)
 {
  var wid=await workspaceContext.RequireCurrentWorkspaceIdAsync(ct);
  var row=await (from v in db.AuditReportVersions join a in db.Audits on v.AuditId equals a.Id where v.Id==versionId&&a.Id==auditId&&a.WorkspaceId==wid select new{Version=v,Audit=a}).SingleOrDefaultAsync(ct);
  if(row is null)return new(null,"Version d'audit introuvable.");
  SnapshotRoot? snapshot;try{snapshot=JsonSerializer.Deserialize<SnapshotRoot>(row.Version.Snapshot,new JsonSerializerOptions{PropertyNameCaseInsensitive=true});}catch(JsonException){return new(null,"Le snapshot de cette version est invalide.");}
  if(snapshot?.Audit is null)return new(null,"Le snapshot de cette version est invalide.");
  var model=new AuditReportPdfModel(snapshot.Audit.Reference,snapshot.Audit.Title,snapshot.Audit.Description,row.Version.VersionNumber,row.Version.FinalizedAtUtc,row.Version.Hash,
   snapshot.Sections.Select(s=>new AuditReportSectionPdfModel(s.Title,s.Description,s.Items.Select(i=>new AuditReportItemPdfModel(i.Label,i.Description,DisplayValue(i.ResponseType,i.Response?.Value),i.Response?.Observation,i.Response?.Recommendation,i.Evidence.Select(e=>e.FileName).ToArray())).ToArray())).ToArray());
  byte[] bytes;try{bytes=pdf.Generate(model);}catch(Exception ex) when(ex is not OperationCanceledException){return new(null,"La génération du PDF a échoué.");}
  string key;await using(var stream=new MemoryStream(bytes,false))key=await storage.SaveAsync(stream,ct);
  var oldDocumentId=row.Version.GeneratedDocumentId;
  var old=oldDocumentId.HasValue?await db.Documents.SingleOrDefaultAsync(x=>x.Id==oldDocumentId&&x.WorkspaceId==wid,ct):null;
  var document=new Document(wid,row.Audit.CustomerId,FileName(row.Audit.Reference,row.Version.VersionNumber),"application/pdf",bytes.LongLength,key);
  db.Documents.Add(document);row.Version.SetGeneratedDocument(document.Id);db.AuditHistoryEvents.Add(new Freecrmlance.Domain.Audits.AuditHistoryEvent(wid,auditId,old is null?Freecrmlance.Domain.Audits.AuditHistoryEventType.ReportGenerated:Freecrmlance.Domain.Audits.AuditHistoryEventType.ReportRegenerated,DateTime.UtcNow,generatedByUserId,versionId));if(old is not null)db.Documents.Remove(old);
  try{await db.SaveChangesAsync(ct);}catch{await storage.DeleteAsync(key,ct);throw;}
  if(old is not null)await storage.DeleteAsync(old.StorageKey,ct);
  return new(document.Id,null);
 }
 public async Task<AuditReportDownload?> DownloadAsync(Guid auditId,Guid versionId,CancellationToken ct=default)
 {
  var wid=await workspaceContext.RequireCurrentWorkspaceIdAsync(ct);
  var row=await (from v in db.AuditReportVersions.AsNoTracking() join a in db.Audits.AsNoTracking() on v.AuditId equals a.Id join d in db.Documents.AsNoTracking() on v.GeneratedDocumentId equals d.Id where v.Id==versionId&&a.Id==auditId&&a.WorkspaceId==wid&&d.WorkspaceId==wid&&d.CustomerId==a.CustomerId select new{d.FileName,d.ContentType,d.StorageKey}).SingleOrDefaultAsync(ct);
  return row is null?null:new(row.FileName,row.ContentType,await storage.OpenReadAsync(row.StorageKey,ct));
 }
 private static string FileName(string reference,int version){var safe=new string(reference.Where(c=>char.IsLetterOrDigit(c)||c is '-' or '_').ToArray());return $"audit-{(string.IsNullOrWhiteSpace(safe)?"rapport":safe)}-v{version}.pdf";}
 private static string? DisplayValue(string type,string? value){if(value is null)return null;if(type=="YesNo")return value=="Yes"?"Oui":value=="No"?"Non":value;if(type=="CompliantNonCompliant")return value=="Compliant"?"Conforme":value=="NonCompliant"?"Non conforme":value;if(type=="MultipleChoice"){try{return string.Join(", ",JsonSerializer.Deserialize<string[]>(value)??[]);}catch(JsonException){return value;}}return value;}
 private sealed record SnapshotRoot(SnapshotAudit Audit,IReadOnlyList<SnapshotSection> Sections);
 private sealed record SnapshotAudit(Guid Id,Guid WorkspaceId,Guid CustomerId,Guid TemplateId,string Reference,string Title,string? Description,DateTime CreatedAtUtc);
 private sealed record SnapshotSection(Guid Id,string Title,string? Description,int Position,IReadOnlyList<SnapshotItem> Items);
 private sealed record SnapshotItem(Guid Id,string Label,string? Description,string ResponseType,bool IsRequired,int Position,string? Options,SnapshotResponse? Response,IReadOnlyList<SnapshotEvidence> Evidence);
 private sealed record SnapshotResponse(string? Value,string? Observation,string? Recommendation,DateTime UpdatedAtUtc,string UpdatedByUserId);
 private sealed record SnapshotEvidence(Guid Id,Guid DocumentId,string FileName,string ContentType,long Size,DateTime CreatedAtUtc);
}
