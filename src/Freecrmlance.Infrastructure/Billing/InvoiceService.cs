using Freecrmlance.Application.Billing;
using Freecrmlance.Application.Platform;
using Freecrmlance.Domain.Billing;
using Freecrmlance.Domain.Crm;
using Freecrmlance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Freecrmlance.Infrastructure.Billing;

public sealed class InvoiceService(FreecrmlanceDbContext dbContext, IWorkspaceContext workspaceContext) : IInvoiceService
{
    public async Task<IReadOnlyList<InvoiceDto>?> ListAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var workspaceId = await workspaceContext.RequireCurrentWorkspaceIdAsync(cancellationToken);
        if (!await CustomerExistsAsync(workspaceId, customerId, cancellationToken)) return null;

        var invoices = await dbContext.Invoices.AsNoTracking().Include(invoice => invoice.Lines)
            .Where(invoice => invoice.WorkspaceId == workspaceId && invoice.CustomerId == customerId)
            .OrderByDescending(invoice => invoice.CreatedAtUtc).ToListAsync(cancellationToken);
        return invoices.Select(ToDto).ToList();
    }

    public async Task<InvoiceDto?> GetAsync(Guid customerId, Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var workspaceId = await workspaceContext.RequireCurrentWorkspaceIdAsync(cancellationToken);
        var invoice = await dbContext.Invoices.AsNoTracking().Include(item => item.Lines)
            .SingleOrDefaultAsync(item => item.Id == invoiceId && item.CustomerId == customerId && item.WorkspaceId == workspaceId, cancellationToken);
        return invoice is null ? null : ToDto(invoice);
    }

    public async Task<Guid?> CreateAsync(CreateInvoiceCommand command, CancellationToken cancellationToken = default)
    {
        var workspaceId = await workspaceContext.RequireCurrentWorkspaceIdAsync(cancellationToken);
        if (!await CustomerExistsAsync(workspaceId, command.CustomerId, cancellationToken)) return null;

        var invoice = new Invoice(workspaceId, command.CustomerId, command.Number);
        foreach (var line in command.Lines ?? [])
            invoice.AddLine(line.Description, line.Quantity, line.UnitPrice);

        dbContext.Invoices.Add(invoice);
        await dbContext.SaveChangesAsync(cancellationToken);
        return invoice.Id;
    }

    public async Task<Guid?> CreateFromAcceptedQuoteAsync(CreateInvoiceFromQuoteCommand command, CancellationToken cancellationToken = default)
    {
        var workspaceId = await workspaceContext.RequireCurrentWorkspaceIdAsync(cancellationToken);
        var quote = await dbContext.Quotes.AsNoTracking().Include(item => item.Lines)
            .SingleOrDefaultAsync(item => item.Id == command.QuoteId && item.CustomerId == command.CustomerId && item.WorkspaceId == workspaceId, cancellationToken);

        if (quote is null || quote.Status != QuoteStatus.Accepted) return null;
        if (await dbContext.Invoices.AsNoTracking().AnyAsync(invoice => invoice.SourceQuoteId == quote.Id, cancellationToken)) return null;

        var invoice = new Invoice(workspaceId, command.CustomerId, command.Number, quote.Id);
        foreach (var line in quote.Lines)
            invoice.AddLine(line.Description, line.Quantity, line.UnitPrice);

        dbContext.Invoices.Add(invoice);
        await dbContext.SaveChangesAsync(cancellationToken);
        return invoice.Id;
    }

    private Task<bool> CustomerExistsAsync(Guid workspaceId, Guid customerId, CancellationToken cancellationToken)
        => dbContext.Customers.AsNoTracking().AnyAsync(customer => customer.Id == customerId && customer.WorkspaceId == workspaceId, cancellationToken);

    private static InvoiceDto ToDto(Invoice invoice)
        => new(invoice.Id, invoice.CustomerId, invoice.SourceQuoteId, invoice.Number, invoice.Status, invoice.CreatedAtUtc, invoice.UpdatedAtUtc, invoice.Total,
            invoice.Lines.Select(line => new InvoiceLineDto(line.Id, line.Description, line.Quantity, line.UnitPrice, line.Total)).ToList());
}
