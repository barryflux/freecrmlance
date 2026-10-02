using System.ComponentModel.DataAnnotations;
using Freecrmlance.Domain.Audits;
namespace Freecrmlance.Web.Models.Audits;
public sealed class AuditTemplateFormViewModel
{
 [Required,StringLength(200)] public string Name{get;set;}=string.Empty;
 [StringLength(2000)] public string? Description{get;set;}
 public List<AuditTemplateSectionViewModel> Sections{get;set;}=[new()];
}
public sealed class AuditTemplateSectionViewModel
{
 [Required,StringLength(200)] public string Title{get;set;}=string.Empty;
 [StringLength(2000)] public string? Description{get;set;}
 public List<AuditTemplateItemViewModel> Items{get;set;}=[new()];
}
public sealed class AuditTemplateItemViewModel
{
 [Required,StringLength(500)] public string Label{get;set;}=string.Empty;
 [StringLength(2000)] public string? Description{get;set;}
 public AuditResponseType ResponseType{get;set;}=AuditResponseType.YesNo;
 public bool IsRequired{get;set;}
 [StringLength(4000)] public string? Options{get;set;}
}
