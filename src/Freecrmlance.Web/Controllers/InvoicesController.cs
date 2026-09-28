using Freecrmlance.Application.Billing;
using Freecrmlance.Application.Crm;
using Freecrmlance.Domain.Crm;
using Freecrmlance.Web.Models.Invoices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Freecrmlance.Web.Controllers;

[Authorize]
public sealed class InvoicesController(IInvoiceService invoiceService, ICustomerService customerService, IQuoteService quoteService) : Controller
{
    [HttpGet("Customers/{customerId:guid}/Invoices/Create")]
    public async Task<IActionResult> Create(Guid customerId, CancellationToken cancellationToken)
        => await customerService.GetAsync(customerId, cancellationToken) is null
            ? NotFound()
            : View(new InvoiceFormViewModel());

    [HttpPost("Customers/{customerId:guid}/Invoices/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Guid customerId, InvoiceFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);
        var id = await invoiceService.CreateAsync(
            new CreateInvoiceCommand(customerId, model.DraftReference,
                model.Lines.Select(line => new CreateInvoiceLineCommand(line.Description, line.Quantity, line.UnitPrice, line.VatRate)).ToArray()),
            cancellationToken);
        return id is null ? NotFound() : RedirectToAction(nameof(Details), new { customerId, invoiceId = id });
    }

    [HttpGet("Customers/{customerId:guid}/Invoices/{invoiceId:guid}")]
    public async Task<IActionResult> Details(Guid customerId, Guid invoiceId, CancellationToken cancellationToken)
    {
        var invoice = await invoiceService.GetAsync(customerId, invoiceId, cancellationToken);
        return invoice is null ? NotFound() : View(invoice);
    }

    [HttpPost("Customers/{customerId:guid}/Invoices/{invoiceId:guid}/Issue")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Issue(Guid customerId, Guid invoiceId, CancellationToken cancellationToken)
    {
        var result = await invoiceService.IssueAsync(customerId, invoiceId, cancellationToken);
        return result switch
        {
            IssueInvoiceResult.Success => RedirectToAction(nameof(Details), new { customerId, invoiceId }),
            IssueInvoiceResult.NotFound => NotFound(),
            IssueInvoiceResult.InvalidStatus => Conflict(),
            IssueInvoiceResult.Incomplete => RedirectToAction(nameof(Details), new { customerId, invoiceId, issuanceError = true }),
            _ => throw new InvalidOperationException("Unknown invoice issuance result.")
        };
    }

    [HttpGet("Customers/{customerId:guid}/Quotes/{quoteId:guid}/Invoice/Create")]
    public async Task<IActionResult> CreateFromQuote(Guid customerId, Guid quoteId, CancellationToken cancellationToken)
    {
        var quote = await quoteService.GetAsync(customerId, quoteId, cancellationToken);
        if (quote is null) return NotFound();
        if (quote.Status != QuoteStatus.Accepted) return RedirectToAction("Details", "Quotes", new { customerId, quoteId });

        var invoices = await invoiceService.ListAsync(customerId, cancellationToken);
        if (invoices is null) return NotFound();
        var existing = invoices.SingleOrDefault(invoice => invoice.SourceQuoteId == quoteId);
        if (existing is not null) return RedirectToAction(nameof(Details), new { customerId, invoiceId = existing.Id });

        ViewBag.Quote = quote;
        return View(new CreateInvoiceFromQuoteViewModel());
    }

    [HttpPost("Customers/{customerId:guid}/Quotes/{quoteId:guid}/Invoice/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFromQuote(Guid customerId, Guid quoteId, CreateInvoiceFromQuoteViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var quote = await quoteService.GetAsync(customerId, quoteId, cancellationToken);
            if (quote is null) return NotFound();
            ViewBag.Quote = quote;
            return View(model);
        }

        var id = await invoiceService.CreateFromAcceptedQuoteAsync(new(customerId, quoteId, model.DraftReference), cancellationToken);
        if (id is not null) return RedirectToAction(nameof(Details), new { customerId, invoiceId = id });

        var invoices = await invoiceService.ListAsync(customerId, cancellationToken);
        var existing = invoices?.SingleOrDefault(invoice => invoice.SourceQuoteId == quoteId);
        return existing is not null
            ? RedirectToAction(nameof(Details), new { customerId, invoiceId = existing.Id })
            : NotFound();
    }
}
