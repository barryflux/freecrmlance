using System.ComponentModel.DataAnnotations;using Microsoft.AspNetCore.Mvc.Rendering;
namespace Freecrmlance.Web.Models.Audits;
public sealed class CreateAuditViewModel{[Required]public Guid CustomerId{get;set;}[Required]public Guid TemplateId{get;set;}[StringLength(200)]public string? Title{get;set;}[StringLength(2000)]public string? Description{get;set;}public IReadOnlyList<SelectListItem> Customers{get;set;}=[];public IReadOnlyList<SelectListItem> Templates{get;set;}=[];}
