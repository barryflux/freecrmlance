using Freecrmlance.Application.Audits;
using Freecrmlance.Application.Crm;
using Freecrmlance.Application.Platform;
using Freecrmlance.Domain.Audits;
using Freecrmlance.Domain.Crm;
using Freecrmlance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Freecrmlance.Infrastructure.Audits;

public sealed class AuditEvidenceService(FreecrmlanceDbContext db, IWorkspaceContext workspaceContext, IFileStorage fileStorage) : IAuditEvidenceService
{
    public async Task<IReadOnlyList<AuditEvidenceDto>?> ListAsync(Guid auditId, CancellationToken ct = default)
    {
        var wid = await workspaceContext.RequireCurrentWorkspaceIdAsync(ct);
        if (!await db.Audits.AsNoTracking().AnyAsync(a => a.Id == auditId && a.WorkspaceId == wid, ct)) return null;

        return await (from evidence in db.AuditEvidence.AsNoTracking()
                      join item in db.AuditItems.AsNoTracking() on evidence.AuditItemId equals item.Id
                      join section in db.AuditSections.AsNoTracking() on item.AuditSectionId equals section.Id
                      join document in db.Documents.AsNoTracking() on evidence.DocumentId equals document.Id
                      where section.AuditId == auditId && document.WorkspaceId == wid
                      orderby evidence.CreatedAtUtc descending
                      select new AuditEvidenceDto(evidence.Id, evidence.AuditItemId, document.Id, document.FileName, document.ContentType, document.Size, evidence.CreatedAtUtc))
            .ToListAsync(ct);
    }

    public async Task<string?> UploadAsync(Guid auditId, Guid auditItemId, string fileName, string contentType, long size, Stream content, string uploadedByUserId, CancellationToken ct = default)
    {
        var wid = await workspaceContext.RequireCurrentWorkspaceIdAsync(ct);
        var audit = await db.Audits.Include(a => a.Sections).ThenInclude(s => s.Items).SingleOrDefaultAsync(a => a.Id == auditId && a.WorkspaceId == wid, ct);
        if (audit is null) return "Audit introuvable.";
        if (audit.Status is AuditStatus.Completed or AuditStatus.Finalized) return "Cet audit n'est plus modifiable.";
        if (!audit.Sections.SelectMany(s => s.Items).Any(i => i.Id == auditItemId)) return "Critère d'audit introuvable.";
        if (size <= 0) return "Le fichier est vide.";

        var safeFileName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeFileName)) return "Nom de fichier invalide.";

        var storageKey = await fileStorage.SaveAsync(content, ct);
        try
        {
            var document = new Document(wid, audit.CustomerId, safeFileName, contentType, size, storageKey);
            db.Documents.Add(document);
            db.AuditEvidence.Add(new AuditEvidence(auditItemId, document.Id, uploadedByUserId));
            audit.Start();
            await db.SaveChangesAsync(ct);
            return null;
        }
        catch
        {
            await fileStorage.DeleteAsync(storageKey, ct);
            throw;
        }
    }

    public async Task<AuditEvidenceDownloadDto?> DownloadAsync(Guid auditId, Guid evidenceId, CancellationToken ct = default)
    {
        var wid = await workspaceContext.RequireCurrentWorkspaceIdAsync(ct);
        var row = await (from evidence in db.AuditEvidence.AsNoTracking()
                         join item in db.AuditItems.AsNoTracking() on evidence.AuditItemId equals item.Id
                         join section in db.AuditSections.AsNoTracking() on item.AuditSectionId equals section.Id
                         join audit in db.Audits.AsNoTracking() on section.AuditId equals audit.Id
                         join document in db.Documents.AsNoTracking() on evidence.DocumentId equals document.Id
                         where evidence.Id == evidenceId && audit.Id == auditId && audit.WorkspaceId == wid && document.WorkspaceId == wid && document.CustomerId == audit.CustomerId
                         select new { document.FileName, document.ContentType, document.StorageKey }).SingleOrDefaultAsync(ct);
        if (row is null) return null;
        return new AuditEvidenceDownloadDto(row.FileName, row.ContentType, await fileStorage.OpenReadAsync(row.StorageKey, ct));
    }

    public async Task<string?> DeleteAsync(Guid auditId, Guid evidenceId, CancellationToken ct = default)
    {
        var wid = await workspaceContext.RequireCurrentWorkspaceIdAsync(ct);
        var audit = await db.Audits.SingleOrDefaultAsync(a => a.Id == auditId && a.WorkspaceId == wid, ct);
        if (audit is null) return "Audit introuvable.";
        if (audit.Status is AuditStatus.Completed or AuditStatus.Finalized) return "Cet audit n'est plus modifiable.";

        var row = await (from evidence in db.AuditEvidence
                         join item in db.AuditItems on evidence.AuditItemId equals item.Id
                         join section in db.AuditSections on item.AuditSectionId equals section.Id
                         join document in db.Documents on evidence.DocumentId equals document.Id
                         where evidence.Id == evidenceId && section.AuditId == auditId && document.WorkspaceId == wid && document.CustomerId == audit.CustomerId
                         select new { Evidence = evidence, Document = document }).SingleOrDefaultAsync(ct);
        if (row is null) return "Preuve introuvable.";

        await fileStorage.DeleteAsync(row.Document.StorageKey, ct);
        db.AuditEvidence.Remove(row.Evidence);
        db.Documents.Remove(row.Document);
        await db.SaveChangesAsync(ct);
        return null;
    }
}
