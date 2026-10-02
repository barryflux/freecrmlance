namespace Freecrmlance.Domain.Audits;

public sealed class AuditSection
{
 private readonly List<AuditItem> _items=[]; private AuditSection() { }
 internal AuditSection(Guid auditId,string title,int position,string? description){if(auditId==Guid.Empty)throw new ArgumentException("Audit id is required.",nameof(auditId));if(string.IsNullOrWhiteSpace(title))throw new ArgumentException("Title is required.",nameof(title));Id=Guid.NewGuid();AuditId=auditId;Title=title.Trim();Description=string.IsNullOrWhiteSpace(description)?null:description.Trim();Position=position;}
 public Guid Id{get;private set;} public Guid AuditId{get;private set;} public string Title{get;private set;}=string.Empty; public string? Description{get;private set;} public int Position{get;private set;} public IReadOnlyCollection<AuditItem> Items=>_items;
 public AuditItem AddItem(string label,AuditResponseType responseType,bool isRequired,int position,string? description=null,string? options=null){var x=new AuditItem(Id,label,responseType,isRequired,position,description,options);_items.Add(x);return x;}
}
