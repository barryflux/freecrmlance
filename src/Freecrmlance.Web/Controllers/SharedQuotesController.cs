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
}
