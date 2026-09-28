using Freecrmlance.Application.Crm;
using Freecrmlance.Application.Platform;
using Freecrmlance.Domain.Crm;
using Freecrmlance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Freecrmlance.Infrastructure.Crm;

public sealed class QuoteService(
    FreecrmlanceDbContext dbContext,
    IWorkspaceContext workspaceContext) : IQuoteService
{
    public async Task<IReadOnlyList<QuoteDto>?> ListAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var workspaceId = await workspaceContext.RequireCurrentWorkspaceIdAsync(cancellationToken);
        if (!await CustomerExistsAsync(workspaceId, customerId, cancellationToken)) return null;

        var quotes = await dbContext.Quotes.AsNoTracking()
            .Include(quote => quote.Lines)
            .Where(quote => quote.WorkspaceId == workspaceId && quote.CustomerId == customerId)
            .OrderByDescending(quote => quote.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return quotes.Select(ToDto).ToList();
    }

    public async Task<QuoteDto?> GetAsync(Guid customerId, Guid quoteId, CancellationToken cancellationToken = default)
    {
        var workspaceId = await workspaceContext.RequireCurrentWorkspaceIdAsync(cancellationToken);
        var quote = await dbContext.Quotes.AsNoTracking()
            .Include(item => item.Lines)
            .SingleOrDefaultAsync(item => item.Id == quoteId
                && item.CustomerId == customerId
                && item.WorkspaceId == workspaceId, cancellationToken);
        return quote is null ? null : ToDto(quote);
    }

    public async Task<Guid?> CreateAsync(CreateQuoteCommand command, CancellationToken cancellationToken = default)
    {
        var workspaceId = await workspaceContext.RequireCurrentWorkspaceIdAsync(cancellationToken);
        if (!await CustomerExistsAsync(workspaceId, command.CustomerId, cancellationToken)) return null;

        var quote = new Quote(workspaceId, command.CustomerId, command.Number);
        foreach (var line in command.Lines ?? [])
            quote.AddLine(line.Description, line.Quantity, line.UnitPrice);

        dbContext.Quotes.Add(quote);
        await dbContext.SaveChangesAsync(cancellationToken);
        return quote.Id;
    }

    public async Task<bool> UpdateDraftAsync(Guid customerId, Guid quoteId, UpdateQuoteCommand command, CancellationToken cancellationToken = default)
    {
        var workspaceId = await workspaceContext.RequireCurrentWorkspaceIdAsync(cancellationToken);
        var quote = await dbContext.Quotes
            .Include(item => item.Lines)
            .SingleOrDefaultAsync(item => item.Id == quoteId
                && item.CustomerId == customerId
                && item.WorkspaceId == workspaceId, cancellationToken);

        if (quote is null || quote.Status != QuoteStatus.Draft) return false;

        var existingLines = quote.Lines.ToArray();

        quote.UpdateDraft(
            command.Number,
            (command.Lines ?? []).Select(line => (line.Description, line.Quantity, line.UnitPrice)));

        foreach (var existingLine in existingLines)
            dbContext.Entry(existingLine).State = EntityState.Deleted;

        foreach (var newLine in quote.Lines)
            dbContext.Entry(newLine).State = EntityState.Added;

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private Task<bool> CustomerExistsAsync(Guid workspaceId, Guid customerId, CancellationToken cancellationToken)
        => dbContext.Customers.AsNoTracking().AnyAsync(
            customer => customer.Id == customerId && customer.WorkspaceId == workspaceId,
            cancellationToken);

    private static QuoteDto ToDto(Quote quote)
        => new(
            quote.Id,
            quote.CustomerId,
            quote.Number,
            quote.Status,
            quote.CreatedAtUtc,
            quote.UpdatedAtUtc,
            quote.Total,
            quote.Lines.Select(line => new QuoteLineDto(
                line.Id, line.Description, line.Quantity, line.UnitPrice, line.Total)).ToList());
}
