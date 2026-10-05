using Freecrmlance.Application.Audits;using Freecrmlance.Application.Crm;using Freecrmlance.Application.Platform;using Freecrmlance.Domain.Audits;using Freecrmlance.Infrastructure.Persistence;using Microsoft.EntityFrameworkCore;
namespace Freecrmlance.Infrastructure.Audits;
public sealed class AuditTransmissionService(FreecrmlanceDbContext db,IWorkspaceContext workspace,IDocumentShareService shares):IAuditTransmissionService
{
 public async Task<IReadOnlyList<AuditReportTransmissionDto>?> ListAsync(Guid auditId,CancellationToken ct=default){var wid=await workspace.RequireCurrentWorkspaceIdAsync(ct);if(!await db.Audits.AsNoTracking().AnyAsync(x=>x.Id==auditId&&x.WorkspaceId==wid,ct))return null;return await(from t in db.AuditReportTransmissions.AsNoTracking() join v in db.AuditReportVersions.AsNoTracking() on t.ReportVersionId equals v.Id where t.AuditId==auditId&&t.WorkspaceId==wid orderby t.SentAtUtc descending select new AuditReportTransmissionDto(t.Id,t.ReportVersionId,v.VersionNumber,t.Recipient,t.SentByUserId,t.SentAtUtc)).ToListAsync(ct);}
 public async Task<CreateAuditTransmissionResult>CreateAsync(Guid auditId,Guid versionId,string recipient,string sentByUserId,CancellationToken ct=default)
 {
  if(string.IsNullOrWhiteSpace(recipient))return new(null,null,"Le destinataire est obligatoire.");if(string.IsNullOrWhiteSpace(sentByUserId))return new(null,null,"Utilisateur introuvable.");
  var wid=await workspace.RequireCurrentWorkspaceIdAsync(ct);var row=await(from v in db.AuditReportVersions.AsNoTracking() join a in db.Audits.AsNoTracking() on v.AuditId equals a.Id where v.Id==versionId&&a.Id==auditId&&a.WorkspaceId==wid select new{Version=v,Audit=a}).SingleOrDefaultAsync(ct);if(row is null)return new(null,null,"Version d'audit introuvable.");if(!row.Version.GeneratedDocumentId.HasValue)return new(null,null,"Générez le PDF de cette version avant de la transmettre.");
  await using var transaction=await db.Database.BeginTransactionAsync(ct);var share=await shares.CreateAsync(row.Audit.CustomerId,row.Version.GeneratedDocumentId.Value,ct);if(share is null){await transaction.RollbackAsync(ct);return new(null,null,"Impossible de créer le lien de partage.");}
  var transmission=new AuditReportTransmission(wid,auditId,versionId,share.Id,recipient,sentByUserId);db.AuditReportTransmissions.Add(transmission);await db.SaveChangesAsync(ct);await transaction.CommitAsync(ct);return new(transmission.Id,share.Token,null);
 }
}
