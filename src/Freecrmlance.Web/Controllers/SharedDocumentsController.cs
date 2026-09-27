using Freecrmlance.Application.Crm;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Freecrmlance.Web.Controllers;

[AllowAnonymous]
public sealed class SharedDocumentsController(IDocumentShareService documentShareService) : Controller
{
    [HttpGet("shared/documents/{token}")]
    public async Task<IActionResult> Download(string token, CancellationToken cancellationToken)
    {
        var document = await documentShareService.DownloadAsync(token, cancellationToken);
        return document is null
            ? NotFound()
            : File(document.Content, document.ContentType, document.FileName);
    }
}
