using Freecrmlance.Application.Crm;
using Freecrmlance.Web.Models.Customers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Freecrmlance.Web.Controllers;

[Authorize]
public sealed class CustomersController(ICustomerService customerService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
        => View(await customerService.ListAsync(cancellationToken));

    [HttpGet("Customers/{id:guid}")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var customer = await customerService.GetAsync(id, cancellationToken);
        return customer is null ? NotFound() : View(customer);
    }

    [HttpGet("Customers/{id:guid}/Edit")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var customer = await customerService.GetAsync(id, cancellationToken);
        if (customer is null)
            return NotFound();

        return View(new EditCustomerViewModel
        {
            Name = customer.Name,
            Email = customer.Email,
            Phone = customer.Phone
        });
    }

    [HttpPost("Customers/{id:guid}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditCustomerViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        var updated = await customerService.UpdateAsync(
            id,
            new UpdateCustomerCommand(model.Name, model.Email, model.Phone),
            cancellationToken);

        return updated ? RedirectToAction(nameof(Details), new { id }) : NotFound();
    }

    [HttpGet]
    public IActionResult Create() => View(new CreateCustomerViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCustomerViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        await customerService.CreateAsync(new CreateCustomerCommand(model.Name, model.Email, model.Phone), cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}
