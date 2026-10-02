namespace Freecrmlance.Domain.Audits;

public sealed class AuditItemResponse
{
 private AuditItemResponse() { }

 public AuditItemResponse(Guid auditItemId,string? value,string? observation,string? recommendation,string updatedByUserId)
 {
  if(auditItemId==Guid.Empty)throw new ArgumentException("Audit item id is required.",nameof(auditItemId));
  if(string.IsNullOrWhiteSpace(updatedByUserId))throw new ArgumentException("User id is required.",nameof(updatedByUserId));
  Id=Guid.NewGuid();AuditItemId=auditItemId;Update(value,observation,recommendation,updatedByUserId);
 }

 public Guid Id{get;private set;}
 public Guid AuditItemId{get;private set;}
 public string? Value{get;private set;}
 public string? Observation{get;private set;}
 public string? Recommendation{get;private set;}
 public DateTime UpdatedAtUtc{get;private set;}
 public string UpdatedByUserId{get;private set;}=string.Empty;

 public void Update(string? value,string? observation,string? recommendation,string updatedByUserId)
 {
  if(string.IsNullOrWhiteSpace(updatedByUserId))throw new ArgumentException("User id is required.",nameof(updatedByUserId));
  Value=Normalize(value);Observation=Normalize(observation);Recommendation=Normalize(recommendation);UpdatedByUserId=updatedByUserId;UpdatedAtUtc=DateTime.UtcNow;
 }
 private static string? Normalize(string? value)=>string.IsNullOrWhiteSpace(value)?null:value.Trim();
}
