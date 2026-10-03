using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Freecrmlance.Application.Audits;
using Freecrmlance.Application.Platform;
using Freecrmlance.Domain.Audits;
using Freecrmlance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Freecrmlance.Infrastructure.Audits;

public sealed class AuditFinalizationService(FreecrmlanceDbContext db, IWorkspaceContext workspaceContext) : IAuditFinalizationService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task<IReadOnlyList<AuditReportVersionDto>?> ListVersionsAsync(Guid auditId, CancellationToken ct = default)
    {
        var wid = await workspaceContext.RequireCurrentWorkspaceIdAsync(ct);
        if (!await db.Audits.AsNoTracking().AnyAsync(x => x.Id == auditId && x.WorkspaceId == wid, ct)) return null;
        return await db.AuditReportVersions.AsNoTracking().Where(x => x.AuditId == auditId).OrderByDescending(x => x.VersionNumber)
            .Select(x => new AuditReportVersionDto(x.Id, x.AuditId, x.VersionNumber, x.FinalizedAtUtc, x.FinalizedByUserId, x.Hash, x.GeneratedDocumentId)).ToListAsync(ct);
    }

    public async Task<AuditReportVersionDto?> GetVersionAsync(Guid auditId, Guid versionId, CancellationToken ct = default)
    {
        var wid = await workspaceContext.RequireCurrentWorkspaceIdAsync(ct);
        return await (from version in db.AuditReportVersions.AsNoTracking()
                      join audit in db.Audits.AsNoTracking() on version.AuditId equals audit.Id
                      where version.Id == versionId && audit.Id == auditId && audit.WorkspaceId == wid
                      select new AuditReportVersionDto(version.Id, version.AuditId, version.VersionNumber, version.FinalizedAtUtc, version.FinalizedByUserId, version.Hash, version.GeneratedDocumentId))
            .SingleOrDefaultAsync(ct);
    }

    public async Task<FinalizeAuditResult> FinalizeAsync(Guid auditId, string finalizedByUserId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(finalizedByUserId)) return new(null, "Utilisateur introuvable.");
        var wid = await workspaceContext.RequireCurrentWorkspaceIdAsync(ct);
        var audit = await db.Audits.Include(a => a.Sections).ThenInclude(s => s.Items).SingleOrDefaultAsync(a => a.Id == auditId && a.WorkspaceId == wid, ct);
        if (audit is null) return new(null, "Audit introuvable.");
        if (audit.Status is AuditStatus.Finalized or AuditStatus.Completed) return new(null, "Cet audit est déjà finalisé.");

        var items = audit.Sections.SelectMany(s => s.Items).ToArray();
        var itemIds = items.Select(x => x.Id).ToArray();
        var responses = await db.AuditItemResponses.AsNoTracking().Where(x => itemIds.Contains(x.AuditItemId)).ToDictionaryAsync(x => x.AuditItemId, ct);
        var missing = items.Where(x => x.IsRequired && (!responses.TryGetValue(x.Id, out var r) || string.IsNullOrWhiteSpace(r.Value))).ToArray();
        if (missing.Length > 0) return new(null, $"Impossible de finaliser : {missing.Length} critère(s) obligatoire(s) ne sont pas renseigné(s).");

        var evidence = await (from e in db.AuditEvidence.AsNoTracking()
                              join d in db.Documents.AsNoTracking() on e.DocumentId equals d.Id
                              where itemIds.Contains(e.AuditItemId) && d.WorkspaceId == wid && d.CustomerId == audit.CustomerId
                              select new { e.AuditItemId, e.Id, e.DocumentId, d.FileName, d.ContentType, d.Size, e.CreatedAtUtc }).ToListAsync(ct);

        var finalizedAt = DateTime.UtcNow;
        var snapshotObject = new
        {
            schemaVersion = 1,
            audit = new { audit.Id, audit.WorkspaceId, audit.CustomerId, audit.TemplateId, audit.Reference, audit.Title, audit.Description, audit.CreatedAtUtc },
            sections = audit.Sections.OrderBy(s => s.Position).Select(s => new
            {
                s.Id, s.Title, s.Description, s.Position,
                items = s.Items.OrderBy(i => i.Position).Select(i => new
                {
                    i.Id, i.Label, i.Description, responseType = i.ResponseType.ToString(), i.IsRequired, i.Position, i.Options,
                    response = responses.TryGetValue(i.Id, out var r) ? new { r.Value, r.Observation, r.Recommendation, r.UpdatedAtUtc, r.UpdatedByUserId } : null,
                    evidence = evidence.Where(e => e.AuditItemId == i.Id).OrderBy(e => e.Id).Select(e => new { e.Id, e.DocumentId, e.FileName, e.ContentType, e.Size, e.CreatedAtUtc })
                })
            })
        };
        var snapshot = JsonSerializer.Serialize(snapshotObject, JsonOptions);
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(snapshot))).ToLowerInvariant();
        var nextVersion = await db.AuditReportVersions.Where(x => x.AuditId == auditId).Select(x => (int?)x.VersionNumber).MaxAsync(ct) ?? 0;
        var version = new AuditReportVersion(auditId, nextVersion + 1, finalizedAt, finalizedByUserId, snapshot, hash);
        db.AuditReportVersions.Add(version);
        audit.FinalizeAudit(finalizedAt);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            db.Entry(version).State = EntityState.Detached;
            await db.Entry(audit).ReloadAsync(ct);
            return new(null, "L'audit a été finalisé simultanément. Rechargez la page.");
        }
        return new(version.Id, null);
    }

    public async Task<string?> ReopenAsync(Guid auditId, CancellationToken ct = default)
    {
        var wid = await workspaceContext.RequireCurrentWorkspaceIdAsync(ct);
        var audit = await db.Audits.SingleOrDefaultAsync(x => x.Id == auditId && x.WorkspaceId == wid, ct);
        if (audit is null) return "Audit introuvable.";
        if (audit.Status != AuditStatus.Finalized) return "Seul un audit finalisé peut être rouvert.";
        audit.Reopen(); await db.SaveChangesAsync(ct); return null;
    }

    public async Task<bool> VerifyHashAsync(Guid auditId, Guid versionId, CancellationToken ct = default)
    {
        var wid = await workspaceContext.RequireCurrentWorkspaceIdAsync(ct);
        var version = await (from v in db.AuditReportVersions.AsNoTracking()
                             join a in db.Audits.AsNoTracking() on v.AuditId equals a.Id
                             where v.Id == versionId && a.Id == auditId && a.WorkspaceId == wid
                             select v).SingleOrDefaultAsync(ct);
        if (version is null) return false;
        var actual = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(version.Snapshot))).ToLowerInvariant();
        return CryptographicOperations.FixedTimeEquals(Convert.FromHexString(actual), Convert.FromHexString(version.Hash));
    }
}
