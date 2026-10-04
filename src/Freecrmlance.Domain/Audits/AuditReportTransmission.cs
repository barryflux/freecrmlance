namespace Freecrmlance.Domain.Audits;
public sealed class AuditReportTransmission
{
 private AuditReportTransmission(){}
 public AuditReportTransmission(Guid workspaceId,Guid auditId,Guid reportVersionId,Guid documentShareId,string recipient,string sentByUserId)
 {
  if(workspaceId==Guid.Empty)throw new ArgumentException("Workspace is required.",nameof(workspaceId));if(auditId==Guid.Empty)throw new ArgumentException("Audit is required.",nameof(auditId));if(reportVersionId==Guid.Empty)throw new ArgumentException("Report version is required.",nameof(reportVersionId));if(documentShareId==Guid.Empty)throw new ArgumentException("Document share is required.",nameof(documentShareId));if(string.IsNullOrWhiteSpace(recipient))throw new ArgumentException("Recipient is required.",nameof(recipient));if(string.IsNullOrWhiteSpace(sentByUserId))throw new ArgumentException("User is required.",nameof(sentByUserId));
  Id=Guid.NewGuid();WorkspaceId=workspaceId;AuditId=auditId;ReportVersionId=reportVersionId;DocumentShareId=documentShareId;Recipient=recipient.Trim();SentByUserId=sentByUserId.Trim();SentAtUtc=DateTime.UtcNow;
 }
 public Guid Id{get;private set;}public Guid WorkspaceId{get;private set;}public Guid AuditId{get;private set;}public Guid ReportVersionId{get;private set;}public Guid DocumentShareId{get;private set;}public string Recipient{get;private set;}=string.Empty;public string SentByUserId{get;private set;}=string.Empty;public DateTime SentAtUtc{get;private set;}
}
