namespace Freecrmlance.Domain.Audits;

public sealed class AuditItem
{
 private AuditItem() { }
 internal AuditItem(Guid auditSectionId,string label,AuditResponseType responseType,bool isRequired,int position,string? description,string? options){if(auditSectionId==Guid.Empty)throw new ArgumentException("Section id is required.",nameof(auditSectionId));if(string.IsNullOrWhiteSpace(label))throw new ArgumentException("Label is required.",nameof(label));Id=Guid.NewGuid();AuditSectionId=auditSectionId;Label=label.Trim();Description=string.IsNullOrWhiteSpace(description)?null:description.Trim();ResponseType=responseType;IsRequired=isRequired;Position=position;Options=string.IsNullOrWhiteSpace(options)?null:options.Trim();}
 public Guid Id{get;private set;} public Guid AuditSectionId{get;private set;} public string Label{get;private set;}=string.Empty; public string? Description{get;private set;} public AuditResponseType ResponseType{get;private set;} public bool IsRequired{get;private set;} public int Position{get;private set;} public string? Options{get;private set;}
}
