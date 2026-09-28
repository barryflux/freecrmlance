using System.Security.Cryptography;
using System.Text;
using Freecrmlance.Application.Crm;
using Freecrmlance.Application.Platform;
using Freecrmlance.Domain.Crm;
using Freecrmlance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Freecrmlance.Infrastructure.Crm;

public sealed class QuoteShareService(
    FreecrmlanceDbContext dbContext,
    IWorkspaceContext workspaceContext) : IQuoteShareService
{
    public async Task<QuoteShareSummaryDto?> GetActiveAsync(Guid customerId, Guid quoteId, CancellationToken cancellationToken = default)
    {
        var workspaceId = await workspaceContext.RequireCurrentWorkspaceIdAsync(cancellationToken);
        return await dbContext.QuoteShares.AsNoTracking()
            .Where(share => share.CustomerId == customerId
                && share.QuoteId == quoteId
                && share.WorkspaceId == workspaceId
                && share.RevokedAtUtc == null)
            .Select(share => new QuoteShareSummaryDto(share.Id, share.CreatedAtUtc))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<QuoteShareDto?> CreateAsync(Guid customerId, Guid quoteId, CancellationToken cancellationToken = default)
    {
        var workspaceId = await workspaceContext.RequireCurrentWorkspaceIdAsync(cancellationToken);
        var quoteExists = await dbContext.Quotes.AsNoTracking().AnyAsync(
            quote => quote.Id == quoteId
                && quote.CustomerId == customerId
                && quote.WorkspaceId == workspaceId,
            cancellationToken);
        if (!quoteExists) return null;

        var hasActiveShare = await dbContext.QuoteShares.AsNoTracking().AnyAsync(
            share => share.QuoteId == quoteId
                && share.CustomerId == customerId
                && share.WorkspaceId == workspaceId
                && share.RevokedAtUtc == null,
            cancellationToken);
        if (hasActiveShare) return null;

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        var share = new QuoteShare(workspaceId, customerId, quoteId, HashToken(token));
        dbContext.QuoteShares.Add(share);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new QuoteShareDto(share.Id, token, share.CreatedAtUtc);
    }

    public async Task<SharedQuoteDto?> GetPublicAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;

        var tokenHash = HashToken(token);
        var data = await (
            from share in dbContext.QuoteShares.AsNoTracking()
            join quote in dbContext.Quotes.AsNoTracking() on share.QuoteId equals quote.Id
            join customer in dbContext.Customers.AsNoTracking() on quote.CustomerId equals customer.Id
            where share.TokenHash == tokenHash && share.RevokedAtUtc == null
            select new { quote, customer.Name })
            .SingleOrDefaultAsync(cancellationToken);

        if (data is null) return null;

        var lines = await dbContext.QuoteLines.AsNoTracking()
            .Where(line => line.QuoteId == data.quote.Id)
            .Select(line => new SharedQuoteLineDto(line.Description, line.Quantity, line.UnitPrice, line.Quantity * line.UnitPrice))
            .ToListAsync(cancellationToken);

        return new SharedQuoteDto(
            data.quote.Number,
            data.quote.Status.ToString(),
            data.Name,
            data.quote.CreatedAtUtc,
            data.quote.UpdatedAtUtc,
            lines.Sum(line => line.Total),
            lines);
    }

    public Task<PublicQuoteResponseResult> AcceptPublicAsync(string token, CancellationToken cancellationToken = default)
        => RespondPublicAsync(token, quote => quote.Accept(), cancellationToken);

    public Task<PublicQuoteResponseResult> RejectPublicAsync(string token, CancellationToken cancellationToken = default)
        => RespondPublicAsync(token, quote => quote.Reject(), cancellationToken);

    private async Task<PublicQuoteResponseResult> RespondPublicAsync(
        string token,
        Action<Quote> transition,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token)) return PublicQuoteResponseResult.NotFound;

        var tokenHash = HashToken(token);
        var quote = await (
            from share in dbContext.QuoteShares
            join candidate in dbContext.Quotes on share.QuoteId equals candidate.Id
            where share.TokenHash == tokenHash && share.RevokedAtUtc == null
            select candidate)
            .SingleOrDefaultAsync(cancellationToken);

        if (quote is null) return PublicQuoteResponseResult.NotFound;
        if (quote.Status != QuoteStatus.Sent) return PublicQuoteResponseResult.InvalidStatus;

        transition(quote);
        await dbContext.SaveChangesAsync(cancellationToken);
        return PublicQuoteResponseResult.Success;
    }

    public async Task<bool> RevokeAsync(Guid customerId, Guid quoteId, Guid shareId, CancellationToken cancellationToken = default)
    {
        var workspaceId = await workspaceContext.RequireCurrentWorkspaceIdAsync(cancellationToken);
        var share = await dbContext.QuoteShares.SingleOrDefaultAsync(
            share => share.Id == shareId
                && share.QuoteId == quoteId
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
