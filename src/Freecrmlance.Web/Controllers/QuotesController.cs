using Freecrmlance.Application.Crm;
using Freecrmlance.Domain.Crm;
using Freecrmlance.Web.Models.Quotes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Freecrmlance.Web.Controllers;

[Authorize]
public sealed class QuotesController(IQuoteService quoteService, ICustomerService customerService) : Controller
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
        return id is null ? NotFound() : RedirectToAction(nameof(Details), new { customerId, quoteId = id });
    }

    [HttpGet("Customers/{customerId:guid}/Quotes/{quoteId:guid}")]
    public async Task<IActionResult> Details(Guid customerId, Guid quoteId, CancellationToken cancellationToken)
    {
        var quote = await quoteService.GetAsync(customerId, quoteId, cancellationToken);
        return quote is null ? NotFound() : View(quote);
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
        return updated ? RedirectToAction(nameof(Details), new { customerId, quoteId }) : NotFound();
    }

    [HttpPost("Customers/{customerId:guid}/Quotes/{quoteId:guid}/Send")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(Guid customerId, Guid quoteId, CancellationToken cancellationToken)
        => await quoteService.MarkSentAsync(customerId, quoteId, cancellationToken)
            ? RedirectToAction(nameof(Details), new { customerId, quoteId })
            : NotFound();

    [HttpPost("Customers/{customerId:guid}/Quotes/{quoteId:guid}/Accept")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(Guid customerId, Guid quoteId, CancellationToken cancellationToken)
        => await quoteService.AcceptAsync(customerId, quoteId, cancellationToken)
            ? RedirectToAction(nameof(Details), new { customerId, quoteId })
            : NotFound();

    [HttpPost("Customers/{customerId:guid}/Quotes/{quoteId:guid}/Reject")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(Guid customerId, Guid quoteId, CancellationToken cancellationToken)
        => await quoteService.RejectAsync(customerId, quoteId, cancellationToken)
            ? RedirectToAction(nameof(Details), new { customerId, quoteId })
            : NotFound();

    private static CreateQuoteLineCommand[] MapLines(QuoteFormViewModel model)
        => model.Lines.Select(line => new CreateQuoteLineCommand(line.Description, line.Quantity, line.UnitPrice)).ToArray();
}
