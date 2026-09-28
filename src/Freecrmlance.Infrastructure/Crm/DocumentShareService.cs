using System.Security.Cryptography;
using System.Text;
using Freecrmlance.Application.Crm;
using Freecrmlance.Application.Platform;
using Freecrmlance.Domain.Crm;
using Freecrmlance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Freecrmlance.Infrastructure.Crm;

public sealed class DocumentShareService(
    FreecrmlanceDbContext dbContext,
    IWorkspaceContext workspaceContext,
    IFileStorage fileStorage) : IDocumentShareService
{
    public async Task<IReadOnlyList<DocumentShareSummaryDto>?> ListAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var workspaceId = await workspaceContext.RequireCurrentWorkspaceIdAsync(cancellationToken);
        var customerExists = await dbContext.Customers.AsNoTracking().AnyAsync(
            customer => customer.Id == customerId && customer.WorkspaceId == workspaceId,
            cancellationToken);
        if (!customerExists) return null;

        return await dbContext.DocumentShares.AsNoTracking()
            .Where(share => share.CustomerId == customerId
                && share.WorkspaceId == workspaceId
                && share.RevokedAtUtc == null)
            .OrderByDescending(share => share.CreatedAtUtc)
            .Select(share => new DocumentShareSummaryDto(share.Id, share.DocumentId, share.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<DocumentShareDto?> CreateAsync(Guid customerId, Guid documentId, CancellationToken cancellationToken = default)
    {
        var workspaceId = await workspaceContext.RequireCurrentWorkspaceIdAsync(cancellationToken);
        var documentExists = await dbContext.Documents.AsNoTracking().AnyAsync(
            document => document.Id == documentId
                && document.CustomerId == customerId
                && document.WorkspaceId == workspaceId,
            cancellationToken);

        if (!documentExists) return null;

        var hasActiveShare = await dbContext.DocumentShares.AsNoTracking().AnyAsync(
            share => share.DocumentId == documentId
                && share.CustomerId == customerId
                && share.WorkspaceId == workspaceId
                && share.RevokedAtUtc == null,
            cancellationToken);
        if (hasActiveShare) return null;

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        var share = new DocumentShare(workspaceId, customerId, documentId, HashToken(token));
        dbContext.DocumentShares.Add(share);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new DocumentShareDto(share.Id, token, share.CreatedAtUtc);
    }

    public async Task<DocumentDownloadDto?> DownloadAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;

        var tokenHash = HashToken(token);
        var data = await (
            from share in dbContext.DocumentShares.AsNoTracking()
            join document in dbContext.Documents.AsNoTracking() on share.DocumentId equals document.Id
            where share.TokenHash == tokenHash && share.RevokedAtUtc == null
            select new { document.FileName, document.ContentType, document.StorageKey })
            .SingleOrDefaultAsync(cancellationToken);

        if (data is null) return null;

        var content = await fileStorage.OpenReadAsync(data.StorageKey, cancellationToken);
        return new DocumentDownloadDto(data.FileName, data.ContentType, content);
    }

    public async Task<bool> RevokeAsync(Guid customerId, Guid documentId, Guid shareId, CancellationToken cancellationToken = default)
    {
        var workspaceId = await workspaceContext.RequireCurrentWorkspaceIdAsync(cancellationToken);
        var share = await dbContext.DocumentShares.SingleOrDefaultAsync(
            share => share.Id == shareId
                && share.DocumentId == documentId
                && share.CustomerId == customerId
                && share.WorkspaceId == workspaceId,
            cancellationToken);

        if (share is null) return false;

        share.Revoke();
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static string HashToken(string token)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();
}
