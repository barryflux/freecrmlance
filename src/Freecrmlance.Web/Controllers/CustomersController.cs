using Freecrmlance.Application.Crm;
using Freecrmlance.Web.Models.Customers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Freecrmlance.Web.Controllers;

[Authorize]
public sealed class CustomersController(ICustomerService customerService, IContactService contactService, IDocumentService documentService, IDocumentShareService documentShareService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) => View(await customerService.ListAsync(cancellationToken));

    [HttpGet("Customers/{id:guid}")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var customer = await customerService.GetAsync(id, cancellationToken);
        if (customer is null) return NotFound();
        var contacts = await contactService.ListAsync(id, cancellationToken);
        if (contacts is null) return NotFound();
        var documents = await documentService.ListAsync(id, cancellationToken);
        return documents is null ? NotFound() : View(new CustomerDetailsViewModel(customer, contacts, documents));
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

        return documentId is null ? NotFound() : RedirectToAction(nameof(Details), new { id });
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
        return revoked ? RedirectToAction(nameof(Details), new { id = customerId }) : NotFound();
    }

    [HttpPost("Customers/{customerId:guid}/Documents/{documentId:guid}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDocument(Guid customerId, Guid documentId, CancellationToken cancellationToken)
    {
        var deleted = await documentService.DeleteAsync(customerId, documentId, cancellationToken);
        return deleted ? RedirectToAction(nameof(Details), new { id = customerId }) : NotFound();
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
        return contactId is null ? NotFound() : RedirectToAction(nameof(Details), new { id });
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

        return updated ? RedirectToAction(nameof(Details), new { id = customerId }) : NotFound();
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
        return deleted ? RedirectToAction(nameof(Details), new { id = customerId }) : NotFound();
    }

    [HttpGet("Customers/{id:guid}/Edit")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var customer = await customerService.GetAsync(id, cancellationToken);
        if (customer is null) return NotFound();
        return View(new EditCustomerViewModel { Name = customer.Name, Email = customer.Email, Phone = customer.Phone });
    }

    [HttpPost("Customers/{id:guid}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditCustomerViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);
        var updated = await customerService.UpdateAsync(id, new UpdateCustomerCommand(model.Name, model.Email, model.Phone), cancellationToken);
        return updated ? RedirectToAction(nameof(Details), new { id }) : NotFound();
    }

    [HttpGet]
    public IActionResult Create() => View(new CreateCustomerViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCustomerViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);
        var contacts = model.Contacts
            .Select(contact => new CreateContactCommand(contact.Name, contact.Email, contact.Phone, contact.Role))
            .ToArray();

        var customerId = await customerService.CreateAsync(
            new CreateCustomerCommand(model.Name, model.Email, model.Phone, contacts),
            cancellationToken);

        return RedirectToAction(nameof(Details), new { id = customerId });
    }
}
