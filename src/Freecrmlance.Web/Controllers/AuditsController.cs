using System.Security.Claims;using System.Text.Json;using Freecrmlance.Application.Audits;using Freecrmlance.Application.Crm;using Freecrmlance.Web.Models.Audits;using Microsoft.AspNetCore.Authorization;using Microsoft.AspNetCore.Mvc;using Microsoft.AspNetCore.Mvc.Rendering;
namespace Freecrmlance.Web.Controllers;
[Authorize]public sealed class AuditsController(IAuditService audits,IAuditTemplateService templates,ICustomerService customers):Controller
{
 [HttpGet]public async Task<IActionResult> Index(CancellationToken ct)=>View(await audits.ListAsync(ct));
 [HttpGet]public async Task<IActionResult> Create(CancellationToken ct){var model=new CreateAuditViewModel();await Populate(model,ct);return View(model);}
 [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult> Create(CreateAuditViewModel model,CancellationToken ct){if(!ModelState.IsValid){await Populate(model,ct);return View(model);}var id=await audits.CreateAsync(new CreateAuditCommand(model.CustomerId,model.TemplateId,model.Title,model.Description),ct);if(id is null){ModelState.AddModelError(string.Empty,"Le client ou le modèle d'audit n'est pas disponible.");await Populate(model,ct);return View(model);}return RedirectToAction(nameof(Details),new{id});}
 [HttpGet]public async Task<IActionResult> Details(Guid id,CancellationToken ct){var audit=await audits.GetAsync(id,ct);return audit is null?NotFound():View(audit);}
 [HttpGet]public async Task<IActionResult> Workspace(Guid id,CancellationToken ct){var audit=await audits.GetAsync(id,ct);return audit is null?NotFound():View(new AuditWorkspaceViewModel{Audit=audit});}
 [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult> SaveResponse(Guid auditId,Guid auditItemId,string? value,string? observation,string? recommendation,string[]? selectedValues,CancellationToken ct)
 {
  if(selectedValues is {Length:>0})value=JsonSerializer.Serialize(selectedValues);
  var userId=User.FindFirstValue(ClaimTypes.NameIdentifier);if(string.IsNullOrWhiteSpace(userId))return Challenge();
  var error=await audits.SaveResponseAsync(new SaveAuditResponseCommand(auditId,auditItemId,value,observation,recommendation,userId),ct);
  if(error is not null)TempData["AuditError"]=error;else TempData["AuditSuccess"]="Critère enregistré.";
  return RedirectToAction(nameof(Workspace),new{id=auditId});
 }
 private async Task Populate(CreateAuditViewModel m,CancellationToken ct){m.Customers=(await customers.ListAsync(ct)).Select(x=>new SelectListItem(x.Name,x.Id.ToString())).ToArray();m.Templates=(await templates.ListAsync(false,ct)).Where(x=>!x.IsArchived).Select(x=>new SelectListItem(x.Name,x.Id.ToString())).ToArray();}
}
