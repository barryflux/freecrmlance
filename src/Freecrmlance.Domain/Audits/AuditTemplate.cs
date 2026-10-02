namespace Freecrmlance.Domain.Audits;

public sealed class AuditTemplate
{
    private readonly List<AuditTemplateSection> _sections = [];
    private AuditTemplate() { }
    public AuditTemplate(Guid workspaceId,string name,string? description=null)
    {
        if(workspaceId==Guid.Empty)throw new ArgumentException("Workspace id is required.",nameof(workspaceId));
        Id=Guid.NewGuid();WorkspaceId=workspaceId;CreatedAtUtc=UpdatedAtUtc=DateTime.UtcNow;Update(name,description);
    }
    public Guid Id{get;private set;} public Guid WorkspaceId{get;private set;}
    public string Name{get;private set;}=string.Empty; public string? Description{get;private set;}
    public bool IsArchived{get;private set;} public DateTime CreatedAtUtc{get;private set;} public DateTime UpdatedAtUtc{get;private set;}
    public IReadOnlyCollection<AuditTemplateSection> Sections=>_sections;
    public void Update(string name,string? description){if(string.IsNullOrWhiteSpace(name))throw new ArgumentException("Template name is required.",nameof(name));Name=name.Trim();Description=Normalize(description);UpdatedAtUtc=DateTime.UtcNow;}
    public AuditTemplateSection AddSection(string title,string? description=null){var s=new AuditTemplateSection(Id,title,_sections.Count,description);_sections.Add(s);UpdatedAtUtc=DateTime.UtcNow;return s;}
    public void ClearSections(){_sections.Clear();UpdatedAtUtc=DateTime.UtcNow;}
    public void Archive(){IsArchived=true;UpdatedAtUtc=DateTime.UtcNow;} public void Restore(){IsArchived=false;UpdatedAtUtc=DateTime.UtcNow;}
    private static string? Normalize(string? value)=>string.IsNullOrWhiteSpace(value)?null:value.Trim();
}