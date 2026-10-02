using Freecrmlance.Application.Audits;
using Freecrmlance.Web.Models.Audits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Freecrmlance.Web.Controllers;
[Authorize]
public sealed class AuditTemplatesController(IAuditTemplateService service):Controller
{
 [HttpGet] public async Task<IActionResult> Index(bool archived,CancellationToken ct)=>View(await service.ListAsync(archived,ct));
 [HttpGet] public IActionResult Create()=>View(new AuditTemplateFormViewModel());
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Create(AuditTemplateFormViewModel model,CancellationToken ct)
 {
  ValidateChoices(model); if(!ModelState.IsValid)return View(model);
  var id=await service.CreateAsync(ToCommand(model),ct);return RedirectToAction(nameof(Edit),new{id});
 }
 [HttpGet] public async Task<IActionResult> Edit(Guid id,CancellationToken ct)
 {
  var x=await service.GetAsync(id,ct);if(x is null)return NotFound();
  return View(new AuditTemplateFormViewModel{Name=x.Name,Description=x.Description,Sections=x.Sections.Select(s=>new AuditTemplateSectionViewModel{Title=s.Title,Description=s.Description,Items=s.Items.Select(i=>new AuditTemplateItemViewModel{Label=i.Label,Description=i.Description,ResponseType=i.ResponseType,IsRequired=i.IsRequired,Options=i.Options}).ToList()}).ToList()});
 }
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Edit(Guid id,AuditTemplateFormViewModel model,CancellationToken ct)
 {
  ValidateChoices(model);if(!ModelState.IsValid)return View(model);
  return await service.UpdateAsync(id,ToCommand(model),ct)?RedirectToAction(nameof(Index)):NotFound();
 }
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Duplicate(Guid id,CancellationToken ct)
 {var copy=await service.DuplicateAsync(id,ct);return copy is null?NotFound():RedirectToAction(nameof(Edit),new{id=copy});}
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Archive(Guid id,bool archived,CancellationToken ct)
 {return await service.SetArchivedAsync(id,archived,ct)?RedirectToAction(nameof(Index),new{archived}):NotFound();}
 private void ValidateChoices(AuditTemplateFormViewModel m)
 {
  for(var s=0;s<m.Sections.Count;s++)for(var i=0;i<m.Sections[s].Items.Count;i++){var x=m.Sections[s].Items[i];if((x.ResponseType is Freecrmlance.Domain.Audits.AuditResponseType.SingleChoice or Freecrmlance.Domain.Audits.AuditResponseType.MultipleChoice) && string.IsNullOrWhiteSpace(x.Options))ModelState.AddModelError($"Sections[{s}].Items[{i}].Options","Renseignez au moins une option.");}
 }
 private static SaveAuditTemplateCommand ToCommand(AuditTemplateFormViewModel m)=>new(m.Name,m.Description,m.Sections.Select(s=>new AuditTemplateSectionInput(s.Title,s.Description,s.Items.Select(i=>new AuditTemplateItemInput(i.Label,i.ResponseType,i.IsRequired,i.Description,i.Options)).ToArray())).ToArray());
}
