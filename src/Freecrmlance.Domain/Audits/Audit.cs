namespace Freecrmlance.Domain.Audits;

public sealed class Audit
{
 private readonly List<AuditSection> _sections=[];
 private Audit() { }
 public Audit(Guid workspaceId,Guid customerId,Guid templateId,string reference,string title,string? description)
 {
  if(workspaceId==Guid.Empty)throw new ArgumentException("Workspace id is required.",nameof(workspaceId));
  if(customerId==Guid.Empty)throw new ArgumentException("Customer id is required.",nameof(customerId));
  if(templateId==Guid.Empty)throw new ArgumentException("Template id is required.",nameof(templateId));
  if(string.IsNullOrWhiteSpace(reference))throw new ArgumentException("Reference is required.",nameof(reference));
  if(string.IsNullOrWhiteSpace(title))throw new ArgumentException("Title is required.",nameof(title));
  Id=Guid.NewGuid();WorkspaceId=workspaceId;CustomerId=customerId;TemplateId=templateId;Reference=reference.Trim();Title=title.Trim();Description=Normalize(description);Status=AuditStatus.Draft;CreatedAtUtc=DateTime.UtcNow;
 }
 public Guid Id{get;private set;} public Guid WorkspaceId{get;private set;} public Guid CustomerId{get;private set;} public Guid TemplateId{get;private set;}
 public string Reference{get;private set;}=string.Empty; public string Title{get;private set;}=string.Empty; public string? Description{get;private set;}
 public AuditStatus Status{get;private set;} public DateTime? StartedAtUtc{get;private set;} public DateTime? CompletedAtUtc{get;private set;} public DateTime CreatedAtUtc{get;private set;}
 public IReadOnlyCollection<AuditSection> Sections=>_sections;
 public AuditSection AddSection(string title,int position,string? description=null){var x=new AuditSection(Id,title,position,description);_sections.Add(x);return x;}
 public void Start(){if(Status==AuditStatus.Draft){Status=AuditStatus.InProgress;StartedAtUtc=DateTime.UtcNow;}}
 private static string? Normalize(string? value)=>string.IsNullOrWhiteSpace(value)?null:value.Trim();
}
