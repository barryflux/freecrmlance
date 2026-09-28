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
        var customer = await dbContext.Customers.AsNoTracking().SingleOrDefaultAsync(item => item.Id == command.CustomerId && item.WorkspaceId == workspaceId, cancellationToken);
        var workspace = await dbContext.Workspaces.AsNoTracking().SingleOrDefaultAsync(item => item.Id == workspaceId, cancellationToken);
        if (customer is null || workspace is null) return null;

        var invoice = new Invoice(workspaceId, command.CustomerId, command.Number);
        SnapshotIdentities(invoice, workspace, customer);
        invoice.SetComplianceDetails(command.ServiceDate, command.DueDate, command.PurchaseOrderReference, command.VatExemptionMention,
            command.PaymentTerms, command.EarlyPaymentDiscountTerms, command.LatePaymentPenaltyTerms, command.RecoveryCostIndemnity);
        foreach (var line in command.Lines ?? [])
            invoice.AddLine(line.Description, line.Quantity, line.UnitPrice, line.VatRate);

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

        var customer = await dbContext.Customers.AsNoTracking().SingleOrDefaultAsync(item => item.Id == command.CustomerId && item.WorkspaceId == workspaceId, cancellationToken);
        var workspace = await dbContext.Workspaces.AsNoTracking().SingleOrDefaultAsync(item => item.Id == workspaceId, cancellationToken);
        if (customer is null || workspace is null) return null;

        var invoice = new Invoice(workspaceId, command.CustomerId, command.Number, quote.Id);
        SnapshotIdentities(invoice, workspace, customer);
        foreach (var line in quote.Lines)
            invoice.AddLine(line.Description, line.Quantity, line.UnitPrice);

        dbContext.Invoices.Add(invoice);
        await dbContext.SaveChangesAsync(cancellationToken);
        return invoice.Id;
    }

    private Task<bool> CustomerExistsAsync(Guid workspaceId, Guid customerId, CancellationToken cancellationToken)
        => dbContext.Customers.AsNoTracking().AnyAsync(customer => customer.Id == customerId && customer.WorkspaceId == workspaceId, cancellationToken);

    private static void SnapshotIdentities(Invoice invoice, Freecrmlance.Domain.Platform.Workspace workspace, Customer customer)
    {
        invoice.SnapshotSeller(workspace.LegalName, workspace.LegalForm, workspace.Siren, workspace.Siret, workspace.VatNumber,
            workspace.AddressLine1, workspace.AddressLine2, workspace.PostalCode, workspace.City, workspace.CountryCode,
            workspace.ContactEmail, workspace.ContactPhone);
        invoice.SnapshotCustomer(customer.Name, customer.Siren, customer.Siret, customer.VatNumber,
            customer.BillingAddressLine1 ?? customer.AddressLine1, customer.BillingAddressLine2 ?? customer.AddressLine2,
            customer.BillingPostalCode ?? customer.PostalCode, customer.BillingCity ?? customer.City,
            customer.BillingCountryCode ?? customer.CountryCode);
    }

    private static InvoiceDto ToDto(Invoice invoice)
        => new(invoice.Id, invoice.CustomerId, invoice.SourceQuoteId, invoice.Number, invoice.Status, invoice.CreatedAtUtc, invoice.UpdatedAtUtc, invoice.Total,
            invoice.Lines.Select(line => new InvoiceLineDto(line.Id, line.Description, line.Quantity, line.UnitPrice, line.Total,
                line.VatRate, line.TotalExcludingTax, line.VatAmount, line.TotalIncludingTax)).ToList(),
            invoice.TotalExcludingTax, invoice.TotalVat, invoice.TotalIncludingTax, invoice.IssueDate, invoice.ServiceDate, invoice.DueDate,
            invoice.PurchaseOrderReference, invoice.SellerLegalName, invoice.SellerSiren, invoice.SellerSiret, invoice.SellerVatNumber,
            invoice.CustomerLegalName, invoice.CustomerSiren, invoice.CustomerSiret, invoice.CustomerVatNumber);
}
