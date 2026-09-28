using Freecrmlance.Application.Crm;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Freecrmlance.Web.Controllers;

[AllowAnonymous]
public sealed class SharedQuotesController(IQuoteShareService quoteShareService) : Controller
{
    [HttpGet("shared/quotes/{token}")]
    public async Task<IActionResult> Details(string token, CancellationToken cancellationToken)
    {
        var quote = await quoteShareService.GetPublicAsync(token, cancellationToken);
        return quote is null ? NotFound() : View(quote);
    }

    [HttpPost("shared/quotes/{token}/accept")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(string token, CancellationToken cancellationToken)
        => await RespondAsync(token, quoteShareService.AcceptPublicAsync, cancellationToken);

    [HttpPost("shared/quotes/{token}/reject")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(string token, CancellationToken cancellationToken)
        => await RespondAsync(token, quoteShareService.RejectPublicAsync, cancellationToken);

    private async Task<IActionResult> RespondAsync(
        string token,
        Func<string, CancellationToken, Task<PublicQuoteResponseResult>> response,
        CancellationToken cancellationToken)
    {
        var result = await response(token, cancellationToken);
        return result switch
        {
            PublicQuoteResponseResult.Success => RedirectToAction(nameof(Details), new { token }),
            PublicQuoteResponseResult.NotFound => NotFound(),
            PublicQuoteResponseResult.InvalidStatus => Conflict(),
            _ => throw new InvalidOperationException("Unknown public quote response result.")
        };
    }
}
