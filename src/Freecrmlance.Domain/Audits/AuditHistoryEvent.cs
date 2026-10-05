namespace Freecrmlance.Domain.Audits;
public enum AuditHistoryEventType{Reopened,ReportGenerated,ReportRegenerated}
public sealed class AuditHistoryEvent
{
 private AuditHistoryEvent(){}
 public AuditHistoryEvent(Guid workspaceId,Guid auditId,AuditHistoryEventType type,DateTime occurredAtUtc,string? userId=null,Guid? reportVersionId=null)
 {
  if(workspaceId==Guid.Empty)throw new ArgumentException("Workspace is required.",nameof(workspaceId));if(auditId==Guid.Empty)throw new ArgumentException("Audit is required.",nameof(auditId));Id=Guid.NewGuid();WorkspaceId=workspaceId;AuditId=auditId;Type=type;OccurredAtUtc=occurredAtUtc;UserId=string.IsNullOrWhiteSpace(userId)?null:userId.Trim();ReportVersionId=reportVersionId;
 }
 public Guid Id{get;private set;}public Guid WorkspaceId{get;private set;}public Guid AuditId{get;private set;}public AuditHistoryEventType Type{get;private set;}public DateTime OccurredAtUtc{get;private set;}public string? UserId{get;private set;}public Guid? ReportVersionId{get;private set;}
}
