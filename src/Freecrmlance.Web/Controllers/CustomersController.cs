using Freecrmlance.Application.Crm;
using Freecrmlance.Web.Models.Customers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Freecrmlance.Web.Controllers;

[Authorize]
public sealed class CustomersController(ICustomerService customerService, IContactService contactService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) => View(await customerService.ListAsync(cancellationToken));

    [HttpGet("Customers/{id:guid}")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var customer = await customerService.GetAsync(id, cancellationToken);
        if (customer is null) return NotFound();
        var contacts = await contactService.ListAsync(id, cancellationToken);
        return contacts is null ? NotFound() : View(new CustomerDetailsViewModel(customer, contacts));
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
        var customerId = await customerService.CreateAsync(new CreateCustomerCommand(model.Name, model.Email, model.Phone), cancellationToken);

        foreach (var contact in model.Contacts)
        {
            await contactService.CreateAsync(
                customerId,
                new CreateContactCommand(contact.Name, contact.Email, contact.Phone, contact.Role),
                cancellationToken);
        }

        return RedirectToAction(nameof(Details), new { id = customerId });
    }
}
