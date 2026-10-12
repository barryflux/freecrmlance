using Freecrmlance.Application.Billing;
using Freecrmlance.Application.Crm;
using Freecrmlance.Domain.Crm;
using Freecrmlance.Web.Models.Customers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Freecrmlance.Web.Controllers;

[Authorize]
public sealed class CustomersController(ICustomerService customerService, IContactService contactService, IDocumentService documentService, IDocumentShareService documentShareService, IQuoteService quoteService, IInvoiceService invoiceService, ICustomerActionService customerActionService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CustomerStatus? status, CancellationToken cancellationToken)
    {
        if (status.HasValue && !Enum.IsDefined(status.Value)) return BadRequest();
        var customers = await customerService.ListAsync(cancellationToken);
        ViewBag.StatusFilter = status;
        ViewBag.ActionCounts = (await customerActionService.GetCountsAsync(customers.Select(c => c.Id).ToArray(), cancellationToken)).ToDictionary(c => c.CustomerId);
        return View(status.HasValue ? customers.Where(customer => customer.Status == status.Value).ToList() : customers);
    }

    [HttpPost("Customers/{id:guid}/Actions")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAction(Guid id, CustomerActionFormViewModel model, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(model.Title)) ModelState.AddModelError(nameof(model.Title), "Le titre est obligatoire.");
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return RedirectToAction(nameof(Details), new { id }, "actions");
        }
        var actionId = await customerActionService.CreateAsync(id, new SaveCustomerActionCommand(model.Title, model.Description, model.DueDate, model.Status), cancellationToken);
        if (actionId is null) return NotFound();
        TempData["SuccessMessage"] = "Action ajoutée.";
        return RedirectToAction(nameof(Details), new { id }, "actions");
    }

    [HttpPost("Customers/{id:guid}/Actions/{actionId:guid}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAction(Guid id, Guid actionId, CustomerActionFormViewModel model, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(model.Title)) ModelState.AddModelError(nameof(model.Title), "Le titre est obligatoire.");
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return RedirectToAction(nameof(Details), new { id }, "actions");
        }
        if (!await customerActionService.UpdateAsync(id, actionId, new SaveCustomerActionCommand(model.Title, model.Description, model.DueDate, model.Status), cancellationToken)) return NotFound();
        TempData["SuccessMessage"] = "Action modifiée.";
        return RedirectToAction(nameof(Details), new { id }, "actions");
    }

    [HttpPost("Customers/{id:guid}/Actions/{actionId:guid}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAction(Guid id, Guid actionId, CancellationToken cancellationToken)
    {
        if (!await customerActionService.DeleteAsync(id, actionId, cancellationToken)) return NotFound();
        TempData["SuccessMessage"] = "Action supprimée.";
        return RedirectToAction(nameof(Details), new { id }, "actions");
    }

    [HttpGet("Customers/FollowUps")]
    public async Task<IActionResult> FollowUps(CancellationToken cancellationToken)
        => View(await customerService.ListDueFollowUpsAsync(DateTime.UtcNow, cancellationToken));

    [HttpPost("Customers/{id:guid}/FollowUp")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ScheduleFollowUp(Guid id, DateTime? nextFollowUpAt, string? followUpNote, CancellationToken cancellationToken)
    {
        if (followUpNote?.Length > 2000) return BadRequest("Note trop longue.");
        if (!await customerService.ScheduleFollowUpAsync(id, nextFollowUpAt, followUpNote, cancellationToken)) return NotFound();
        TempData["SuccessMessage"] = "Relance enregistrée.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("Customers/{id:guid}/FollowUp/Complete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteFollowUp(Guid id, DateTime? nextFollowUpAt, string? followUpNote, CancellationToken cancellationToken)
    {
        if (followUpNote?.Length > 2000) return BadRequest("Note trop longue.");
        if (!await customerService.MarkContactedAsync(id, nextFollowUpAt, followUpNote, cancellationToken)) return NotFound();
        TempData["SuccessMessage"] = "Contact effectué et relance mise à jour.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet("Customers/{id:guid}")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var customer = await customerService.GetAsync(id, cancellationToken);
        if (customer is null) return NotFound();
        var contacts = await contactService.ListAsync(id, cancellationToken);
        if (contacts is null) return NotFound();
        var documents = await documentService.ListAsync(id, cancellationToken);
        if (documents is null) return NotFound();
        var documentShares = await documentShareService.ListAsync(id, cancellationToken);
        if (documentShares is null) return NotFound();
        var quotes = await quoteService.ListAsync(id, cancellationToken);
        if (quotes is null) return NotFound();
        var invoices = await invoiceService.ListAsync(id, cancellationToken);
        if (invoices is null) return NotFound();
        var actions = await customerActionService.ListAsync(id, cancellationToken);
        return actions is null ? NotFound() : View(new CustomerDetailsViewModel(customer, contacts, documents, documentShares, quotes, invoices, actions));
    }


    [HttpPost("Customers/{id:guid}/Documents")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UploadDocument(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return RedirectToAction(nameof(Details), new { id });

        await using var content = file.OpenReadStream();
        var documentId = await documentService.UploadAsync(
            id,
            file.FileName,
            file.ContentType,
            file.Length,
            content,
            cancellationToken);

        if (documentId is null) return NotFound();
        TempData["SuccessMessage"] = "Document ajouté avec succès.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet("Customers/{customerId:guid}/Documents/{documentId:guid}")]
    public async Task<IActionResult> DownloadDocument(Guid customerId, Guid documentId, CancellationToken cancellationToken)
    {
        var document = await documentService.DownloadAsync(customerId, documentId, cancellationToken);
        return document is null
            ? NotFound()
            : File(document.Content, document.ContentType, document.FileName);
    }

    [HttpPost("Customers/{customerId:guid}/Documents/{documentId:guid}/Shares")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDocumentShare(Guid customerId, Guid documentId, CancellationToken cancellationToken)
    {
        var share = await documentShareService.CreateAsync(customerId, documentId, cancellationToken);
        if (share is null) return NotFound();

        TempData["SuccessMessage"] = "Lien de partage du document créé.";
        TempData["DocumentShareUrl"] = Url.Action(
            "Download",
            "SharedDocuments",
            new { token = share.Token },
            Request.Scheme);

        return RedirectToAction(nameof(Details), new { id = customerId });
    }

    [HttpPost("Customers/{customerId:guid}/Documents/{documentId:guid}/Shares/{shareId:guid}/Revoke")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RevokeDocumentShare(Guid customerId, Guid documentId, Guid shareId, CancellationToken cancellationToken)
    {
        var revoked = await documentShareService.RevokeAsync(customerId, documentId, shareId, cancellationToken);
        if (!revoked) return NotFound();
        TempData["SuccessMessage"] = "Partage du document révoqué.";
        return RedirectToAction(nameof(Details), new { id = customerId });
    }

    [HttpPost("Customers/{customerId:guid}/Documents/{documentId:guid}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDocument(Guid customerId, Guid documentId, CancellationToken cancellationToken)
    {
        var deleted = await documentService.DeleteAsync(customerId, documentId, cancellationToken);
        if (!deleted) return NotFound();
        TempData["SuccessMessage"] = "Suppression effectuée avec succès.";
        return RedirectToAction(nameof(Details), new { id = customerId });
    }

    [HttpGet("Customers/{id:guid}/Contacts/Create")]
    public async Task<IActionResult> CreateContact(Guid id, CancellationToken cancellationToken)
        => await customerService.GetAsync(id, cancellationToken) is null ? NotFound() : View(new CreateContactViewModel());

    [HttpPost("Customers/{id:guid}/Contacts/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateContact(Guid id, CreateContactViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);
        var contactId = await contactService.CreateAsync(id, new CreateContactCommand(model.Name, model.Email, model.Phone, model.Role), cancellationToken);
        if (contactId is null) return NotFound();
        TempData["SuccessMessage"] = "Contact créé avec succès.";
        return Redirect($"{Url.Action(nameof(Details), new { id })}#contacts");
    }

    [HttpGet("Customers/{customerId:guid}/Contacts/{contactId:guid}/Edit")]
    public async Task<IActionResult> EditContact(Guid customerId, Guid contactId, CancellationToken cancellationToken)
    {
        var contact = await contactService.GetAsync(customerId, contactId, cancellationToken);
        if (contact is null) return NotFound();

        return View(new EditContactViewModel
        {
            Name = contact.Name,
            Email = contact.Email,
            Phone = contact.Phone,
            Role = contact.Role
        });
    }

    [HttpPost("Customers/{customerId:guid}/Contacts/{contactId:guid}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditContact(Guid customerId, Guid contactId, EditContactViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);

        var updated = await contactService.UpdateAsync(
            customerId,
            contactId,
            new UpdateContactCommand(model.Name, model.Email, model.Phone, model.Role),
            cancellationToken);

        if (!updated) return NotFound();
        TempData["SuccessMessage"] = "Contact modifié avec succès.";
        return RedirectToAction(nameof(Details), new { id = customerId });
    }

    [HttpGet("Customers/{customerId:guid}/Contacts/{contactId:guid}/Delete")]
    public async Task<IActionResult> DeleteContact(Guid customerId, Guid contactId, CancellationToken cancellationToken)
    {
        var contact = await contactService.GetAsync(customerId, contactId, cancellationToken);
        return contact is null ? NotFound() : View(contact);
    }

    [HttpPost("Customers/{customerId:guid}/Contacts/{contactId:guid}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteContactConfirmed(Guid customerId, Guid contactId, CancellationToken cancellationToken)
    {
        var deleted = await contactService.DeleteAsync(customerId, contactId, cancellationToken);
        if (!deleted) return NotFound();
        TempData["SuccessMessage"] = "Contact supprimé avec succès.";
        return RedirectToAction(nameof(Details), new { id = customerId });
    }

    [HttpGet("Customers/{id:guid}/Edit")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var customer = await customerService.GetAsync(id, cancellationToken);
        if (customer is null) return NotFound();
        return View(new EditCustomerViewModel
        {
            Name = customer.Name, Email = customer.Email, Phone = customer.Phone, Type = customer.Type, Status = customer.Status,
            Siren = customer.Siren, Siret = customer.Siret, VatNumber = customer.VatNumber,
            AddressLine1 = customer.AddressLine1 ?? string.Empty, AddressLine2 = customer.AddressLine2,
            PostalCode = customer.PostalCode ?? string.Empty, City = customer.City ?? string.Empty, CountryCode = customer.CountryCode ?? "FR",
            BillingAddressLine1 = customer.BillingAddressLine1, BillingAddressLine2 = customer.BillingAddressLine2,
            BillingPostalCode = customer.BillingPostalCode, BillingCity = customer.BillingCity, BillingCountryCode = customer.BillingCountryCode
        });
    }

    [HttpPost("Customers/{id:guid}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditCustomerViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            var updated = await customerService.UpdateAsync(id, new UpdateCustomerCommand(model.Name, model.Email, model.Phone, model.Type, model.Siren, model.Siret, model.VatNumber,
                    model.AddressLine1, model.AddressLine2, model.PostalCode, model.City, model.CountryCode,
                    model.BillingAddressLine1, model.BillingAddressLine2, model.BillingPostalCode, model.BillingCity, model.BillingCountryCode, model.Status), cancellationToken);
            if (!updated) return NotFound();
            TempData["SuccessMessage"] = "Fiche modifiée avec succès.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (ArgumentException)
        {
            ModelState.AddModelError(string.Empty, "Les informations de facturation sont incomplètes ou invalides. Vérifiez les champs renseignés.");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Create() => View(new CreateCustomerViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCustomerViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            var contacts = model.Contacts
                .Select(contact => new CreateContactCommand(contact.Name, contact.Email, contact.Phone, contact.Role))
                .ToArray();

            await customerService.CreateAsync(
                new CreateCustomerCommand(model.Name, model.Email, model.Phone, contacts, model.Type, model.Siren, model.Siret, model.VatNumber,
                    model.AddressLine1, model.AddressLine2, model.PostalCode, model.City, model.CountryCode,
                    model.BillingAddressLine1, model.BillingAddressLine2, model.BillingPostalCode, model.BillingCity, model.BillingCountryCode, model.Status),
                cancellationToken);

            TempData["SuccessMessage"] = "Fiche créée avec succès.";
            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException)
        {
            ModelState.AddModelError(string.Empty, "Les informations de facturation sont incomplètes ou invalides. Vérifiez les champs renseignés.");
            return View(model);
        }
    }
}
