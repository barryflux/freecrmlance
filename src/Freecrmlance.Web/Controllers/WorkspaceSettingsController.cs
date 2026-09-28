using Freecrmlance.Application.Platform;
using Freecrmlance.Web.Models.WorkspaceSettings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Freecrmlance.Web.Controllers;

[Authorize]
public sealed class WorkspaceSettingsController(IWorkspaceSettingsService workspaceSettingsService) : Controller
{
    [HttpGet("Settings/LegalIdentity")]
    public async Task<IActionResult> LegalIdentity(CancellationToken cancellationToken)
    {
        var settings = await workspaceSettingsService.GetAsync(cancellationToken);
        if (settings is null) return NotFound();
        return View(new WorkspaceLegalIdentityViewModel
        {
            WorkspaceName = settings.WorkspaceName, LegalName = settings.LegalName ?? string.Empty, LegalForm = settings.LegalForm,
            Siren = settings.Siren, Siret = settings.Siret, VatNumber = settings.VatNumber,
            AddressLine1 = settings.AddressLine1 ?? string.Empty, AddressLine2 = settings.AddressLine2,
            PostalCode = settings.PostalCode ?? string.Empty, City = settings.City ?? string.Empty, CountryCode = settings.CountryCode ?? "FR",
            ContactEmail = settings.ContactEmail, ContactPhone = settings.ContactPhone
        });
    }

    [HttpPost("Settings/LegalIdentity")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LegalIdentity(WorkspaceLegalIdentityViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);
        var updated = await workspaceSettingsService.UpdateAsync(new(model.LegalName, model.LegalForm, model.Siren, model.Siret,
            model.VatNumber, model.AddressLine1, model.AddressLine2, model.PostalCode, model.City, model.CountryCode,
            model.ContactEmail, model.ContactPhone), cancellationToken);
        if (!updated) return NotFound();
        TempData["Success"] = "Legal identity updated.";
        return RedirectToAction(nameof(LegalIdentity));
    }
}
