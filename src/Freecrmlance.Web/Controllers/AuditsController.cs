using Freecrmlance.Application.Audits;using Freecrmlance.Application.Crm;using Freecrmlance.Web.Models.Audits;using Microsoft.AspNetCore.Authorization;using Microsoft.AspNetCore.Mvc;using Microsoft.AspNetCore.Mvc.Rendering;
namespace Freecrmlance.Web.Controllers;
[Authorize]public sealed class AuditsController(IAuditService audits,IAuditTemplateService templates,ICustomerService customers):Controller
{
 [HttpGet]public async Task<IActionResult> Index(CancellationToken ct)=>View(await audits.ListAsync(ct));
 [HttpGet]public async Task<IActionResult> Create(CancellationToken ct){var model=new CreateAuditViewModel();await Populate(model,ct);return View(model);}
 [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult> Create(CreateAuditViewModel model,CancellationToken ct){if(!ModelState.IsValid){await Populate(model,ct);return View(model);}var id=await audits.CreateAsync(new CreateAuditCommand(model.CustomerId,model.TemplateId,model.Title,model.Description),ct);if(id is null){ModelState.AddModelError(string.Empty,"Le client ou le modèle d'audit n'est pas disponible.");await Populate(model,ct);return View(model);}return RedirectToAction(nameof(Details),new{id});}
 [HttpGet]public async Task<IActionResult> Details(Guid id,CancellationToken ct){var audit=await audits.GetAsync(id,ct);return audit is null?NotFound():View(audit);}
 private async Task Populate(CreateAuditViewModel m,CancellationToken ct){m.Customers=(await customers.ListAsync(ct)).Select(x=>new SelectListItem(x.Name,x.Id.ToString())).ToArray();m.Templates=(await templates.ListAsync(false,ct)).Where(x=>!x.IsArchived).Select(x=>new SelectListItem(x.Name,x.Id.ToString())).ToArray();}
}
