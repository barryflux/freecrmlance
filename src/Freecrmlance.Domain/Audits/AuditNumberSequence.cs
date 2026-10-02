namespace Freecrmlance.Domain.Audits;

public sealed class AuditNumberSequence
{
 private AuditNumberSequence() { }
 public AuditNumberSequence(Guid workspaceId,int year){if(workspaceId==Guid.Empty)throw new ArgumentException("Workspace is required.",nameof(workspaceId));WorkspaceId=workspaceId;Year=year;}
 public Guid WorkspaceId{get;private set;} public int Year{get;private set;} public int LastNumber{get;private set;}
}
