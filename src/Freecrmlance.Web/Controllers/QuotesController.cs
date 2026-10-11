using Freecrmlance.Application.Billing;
using Freecrmlance.Application.Crm;
using Freecrmlance.Domain.Crm;
using Freecrmlance.Web.Models.Quotes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Freecrmlance.Web.Controllers;

[Authorize]
public sealed class QuotesController(
    IQuoteService quoteService,
    ICustomerService customerService,
    IQuotePdfService quotePdfService,
    IQuoteShareService quoteShareService,
    IInvoiceService invoiceService) : Controller
{
    [HttpGet("Customers/{customerId:guid}/Quotes/Create")]
    public async Task<IActionResult> Create(Guid customerId, CancellationToken cancellationToken)
        => await customerService.GetAsync(customerId, cancellationToken) is null
            ? NotFound()
            : View(new QuoteFormViewModel());

    [HttpPost("Customers/{customerId:guid}/Quotes/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Guid customerId, QuoteFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);
        var id = await quoteService.CreateAsync(
            new CreateQuoteCommand(customerId, model.Number, MapLines(model)),
            cancellationToken);
        if (id is null) return NotFound();
        TempData["SuccessMessage"] = "Devis créé avec succès.";
        return RedirectToAction(nameof(Details), new { customerId, quoteId = id });
    }

    [HttpGet("Customers/{customerId:guid}/Quotes/{quoteId:guid}")]
    public async Task<IActionResult> Details(Guid customerId, Guid quoteId, CancellationToken cancellationToken)
    {
        var quote = await quoteService.GetAsync(customerId, quoteId, cancellationToken);
        if (quote is null) return NotFound();

        var share = await quoteShareService.GetActiveAsync(customerId, quoteId, cancellationToken);
        var shareUrl = TempData["QuoteShareUrl"] as string;
        var invoices = await invoiceService.ListAsync(customerId, cancellationToken);
        if (invoices is null) return NotFound();
        var sourceInvoice = invoices.SingleOrDefault(invoice => invoice.SourceQuoteId == quoteId);
        return View(new QuoteDetailsViewModel(quote, share, shareUrl, sourceInvoice));
    }

    [HttpPost("Customers/{customerId:guid}/Quotes/{quoteId:guid}/Share")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Share(Guid customerId, Guid quoteId, CancellationToken cancellationToken)
    {
        var share = await quoteShareService.CreateAsync(customerId, quoteId, cancellationToken);
        if (share is null) return NotFound();

        TempData["QuoteShareUrl"] = Url.Action(
            nameof(SharedQuotesController.Details),
            "SharedQuotes",
            new { token = share.Token },
            Request.Scheme);

        TempData["SuccessMessage"] = "Lien de partage du devis créé.";
        return RedirectToAction(nameof(Details), new { customerId, quoteId });
    }

    [HttpPost("Customers/{customerId:guid}/Quotes/{quoteId:guid}/Shares/{shareId:guid}/Revoke")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RevokeShare(Guid customerId, Guid quoteId, Guid shareId, CancellationToken cancellationToken)
    {
        if (!await quoteShareService.RevokeAsync(customerId, quoteId, shareId, cancellationToken)) return NotFound();
        TempData["SuccessMessage"] = "Partage du devis révoqué.";
        return RedirectToAction(nameof(Details), new { customerId, quoteId });
    }

    [HttpGet("Customers/{customerId:guid}/Quotes/{quoteId:guid}/Edit")]
    public async Task<IActionResult> Edit(Guid customerId, Guid quoteId, CancellationToken cancellationToken)
    {
        var quote = await quoteService.GetAsync(customerId, quoteId, cancellationToken);
        if (quote is null) return NotFound();
        if (quote.Status != QuoteStatus.Draft) return RedirectToAction(nameof(Details), new { customerId, quoteId });

        return View(new QuoteFormViewModel
        {
            Number = quote.Number,
            Lines = quote.Lines.Select(line => new QuoteLineViewModel
            {
                Description = line.Description,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice
            }).ToList()
        });
    }

    [HttpPost("Customers/{customerId:guid}/Quotes/{quoteId:guid}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid customerId, Guid quoteId, QuoteFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);
        var updated = await quoteService.UpdateDraftAsync(
            customerId, quoteId, new UpdateQuoteCommand(model.Number, MapLines(model)), cancellationToken);
        if (!updated) return NotFound();
        TempData["SuccessMessage"] = "Devis modifié avec succès.";
        return RedirectToAction(nameof(Details), new { customerId, quoteId });
    }

    [HttpPost("Customers/{customerId:guid}/Quotes/{quoteId:guid}/Send")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(Guid customerId, Guid quoteId, CancellationToken cancellationToken)
    {
        if (!await quoteService.MarkSentAsync(customerId, quoteId, cancellationToken)) return NotFound();
        TempData["SuccessMessage"] = "Devis marqué comme envoyé.";
        return RedirectToAction(nameof(Details), new { customerId, quoteId });
    }

    [HttpPost("Customers/{customerId:guid}/Quotes/{quoteId:guid}/Accept")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(Guid customerId, Guid quoteId, CancellationToken cancellationToken)
    {
        if (!await quoteService.AcceptAsync(customerId, quoteId, cancellationToken)) return NotFound();
        TempData["SuccessMessage"] = "Devis accepté.";
        return RedirectToAction(nameof(Details), new { customerId, quoteId });
    }

    [HttpPost("Customers/{customerId:guid}/Quotes/{quoteId:guid}/Reject")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(Guid customerId, Guid quoteId, CancellationToken cancellationToken)
    {
        if (!await quoteService.RejectAsync(customerId, quoteId, cancellationToken)) return NotFound();
        TempData["SuccessMessage"] = "Devis refusé.";
        return RedirectToAction(nameof(Details), new { customerId, quoteId });
    }

    [HttpGet("Customers/{customerId:guid}/Quotes/{quoteId:guid}/Pdf")]
    public async Task<IActionResult> Pdf(Guid customerId, Guid quoteId, CancellationToken cancellationToken)
    {
        var pdf = await quotePdfService.GenerateAsync(customerId, quoteId, cancellationToken);
        return pdf is null ? NotFound() : File(pdf.Content, "application/pdf", pdf.FileName);
    }

    private static CreateQuoteLineCommand[] MapLines(QuoteFormViewModel model)
        => model.Lines.Select(line => new CreateQuoteLineCommand(line.Description, line.Quantity, line.UnitPrice)).ToArray();
}
