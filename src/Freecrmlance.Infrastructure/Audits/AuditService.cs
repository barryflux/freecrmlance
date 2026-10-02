using System.Globalization;
using System.Text.Json;
using Freecrmlance.Application.Audits;using Freecrmlance.Application.Platform;using Freecrmlance.Domain.Audits;using Freecrmlance.Infrastructure.Persistence;using Microsoft.EntityFrameworkCore;using Microsoft.EntityFrameworkCore.Storage;
namespace Freecrmlance.Infrastructure.Audits;
public sealed class AuditService(FreecrmlanceDbContext db,IWorkspaceContext workspaceContext):IAuditService
{
 public async Task<IReadOnlyList<AuditDto>> ListAsync(CancellationToken ct=default){var wid=await workspaceContext.RequireCurrentWorkspaceIdAsync(ct);var rows=await db.Audits.AsNoTracking().Where(x=>x.WorkspaceId==wid).OrderByDescending(x=>x.CreatedAtUtc).ToListAsync(ct);return rows.Select(MapSummary).ToArray();}
 public async Task<AuditDto?> GetAsync(Guid id,CancellationToken ct=default)
 {
  var wid=await workspaceContext.RequireCurrentWorkspaceIdAsync(ct);
  var x=await db.Audits.AsNoTracking().Include(a=>a.Sections).ThenInclude(s=>s.Items).SingleOrDefaultAsync(a=>a.Id==id&&a.WorkspaceId==wid,ct);if(x is null)return null;
  var itemIds=x.Sections.SelectMany(s=>s.Items).Select(i=>i.Id).ToArray();
  var responses=await db.AuditItemResponses.AsNoTracking().Where(r=>itemIds.Contains(r.AuditItemId)).ToDictionaryAsync(r=>r.AuditItemId,ct);
  return Map(x,responses);
 }
 public async Task<Guid?> CreateAsync(CreateAuditCommand command,CancellationToken ct=default)
 {
  var wid=await workspaceContext.RequireCurrentWorkspaceIdAsync(ct);
  var customerExists=await db.Customers.AsNoTracking().AnyAsync(c=>c.Id==command.CustomerId&&c.WorkspaceId==wid,ct);if(!customerExists)return null;
  var template=await db.AuditTemplates.AsNoTracking().Include(t=>t.Sections).ThenInclude(s=>s.Items).SingleOrDefaultAsync(t=>t.Id==command.TemplateId&&t.WorkspaceId==wid&&!t.IsArchived,ct);if(template is null)return null;
  await using var transaction=await db.Database.BeginTransactionAsync(ct);var now=DateTime.UtcNow;
  await using var sql=db.Database.GetDbConnection().CreateCommand();sql.Transaction=transaction.GetDbTransaction();
  sql.CommandText="""
INSERT INTO audit."AuditNumberSequences" ("WorkspaceId", "Year", "LastNumber")
VALUES (@workspaceId, @year, 1)
ON CONFLICT ("WorkspaceId", "Year")
DO UPDATE SET "LastNumber" = audit."AuditNumberSequences"."LastNumber" + 1
RETURNING "LastNumber"
""";
  var wp=sql.CreateParameter();wp.ParameterName="workspaceId";wp.Value=wid;sql.Parameters.Add(wp);var yp=sql.CreateParameter();yp.ParameterName="year";yp.Value=now.Year;sql.Parameters.Add(yp);var next=Convert.ToInt32(await sql.ExecuteScalarAsync(ct));
  var audit=new Audit(wid,command.CustomerId,template.Id,$"AUD-{now.Year}-{next:D4}",string.IsNullOrWhiteSpace(command.Title)?template.Name:command.Title!,command.Description??template.Description);
  foreach(var s in template.Sections.OrderBy(s=>s.Position)){var snapshot=audit.AddSection(s.Title,s.Position,s.Description);foreach(var i in s.Items.OrderBy(i=>i.Position))snapshot.AddItem(i.Label,i.ResponseType,i.IsRequired,i.Position,i.Description,i.Options);}
  db.Audits.Add(audit);await db.SaveChangesAsync(ct);await transaction.CommitAsync(ct);return audit.Id;
 }
 public async Task<string?> SaveResponseAsync(SaveAuditResponseCommand command,CancellationToken ct=default)
 {
  var wid=await workspaceContext.RequireCurrentWorkspaceIdAsync(ct);
  var audit=await db.Audits.Include(a=>a.Sections).ThenInclude(s=>s.Items).SingleOrDefaultAsync(a=>a.Id==command.AuditId&&a.WorkspaceId==wid,ct);
  if(audit is null)return "Audit introuvable.";
  if(audit.Status is AuditStatus.Completed or AuditStatus.Finalized)return "Cet audit n'est plus modifiable.";
  var item=audit.Sections.SelectMany(s=>s.Items).SingleOrDefault(i=>i.Id==command.AuditItemId);if(item is null)return "Critère d'audit introuvable.";
  var value=Normalize(command.Value);var validation=Validate(item,value);if(validation is not null)return validation;
  var response=await db.AuditItemResponses.SingleOrDefaultAsync(r=>r.AuditItemId==item.Id,ct);
  if(response is null)db.AuditItemResponses.Add(new AuditItemResponse(item.Id,value,command.Observation,command.Recommendation,command.UpdatedByUserId));else response.Update(value,command.Observation,command.Recommendation,command.UpdatedByUserId);
  audit.Start();await db.SaveChangesAsync(ct);return null;
 }
 private static string? Validate(AuditItem item,string? value)
 {
  if(value is null)return null;
  switch(item.ResponseType)
  {
   case AuditResponseType.YesNo: if(value is not ("Yes" or "No"))return "La réponse doit être Oui ou Non.";break;
   case AuditResponseType.CompliantNonCompliant: if(value is not ("Compliant" or "NonCompliant"))return "La réponse doit être Conforme ou Non conforme.";break;
   case AuditResponseType.Number: if(!decimal.TryParse(value,NumberStyles.Number,CultureInfo.InvariantCulture,out _))return "La réponse doit être un nombre.";break;
   case AuditResponseType.Rating: if(!int.TryParse(value,out var rating)||rating<1||rating>5)return "La note doit être comprise entre 1 et 5.";break;
   case AuditResponseType.SingleChoice:
    if(!Options(item.Options).Contains(value,StringComparer.Ordinal))return "Le choix sélectionné n'est pas valide.";break;
   case AuditResponseType.MultipleChoice:
    try{var selected=JsonSerializer.Deserialize<string[]>(value)??[];var allowed=Options(item.Options);if(selected.Length==0||selected.Any(x=>!allowed.Contains(x,StringComparer.Ordinal)))return "Un ou plusieurs choix sont invalides.";}catch(JsonException){return "Les choix sélectionnés ne sont pas valides.";}break;
  }
  return null;
 }
 private static string[] Options(string? options)=>string.IsNullOrWhiteSpace(options)?[]:options.Split(new[]{'\r','\n',',',';'},StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries);
 private static string? Normalize(string? value)=>string.IsNullOrWhiteSpace(value)?null:value.Trim();
 private static AuditDto MapSummary(Audit x)=>new(x.Id,x.CustomerId,x.TemplateId,x.Reference,x.Title,x.Description,x.Status,x.CreatedAtUtc,[],0,0,0,0);
 private static AuditDto Map(Audit x,IReadOnlyDictionary<Guid,AuditItemResponse> responses)
 {
  var items=x.Sections.SelectMany(s=>s.Items).ToArray();var answered=items.Count(i=>responses.TryGetValue(i.Id,out var r)&&r.Value is not null);var required=items.Count(i=>i.IsRequired);var requiredAnswered=items.Count(i=>i.IsRequired&&responses.TryGetValue(i.Id,out var r)&&r.Value is not null);
  var sections=x.Sections.OrderBy(s=>s.Position).Select(s=>new AuditSectionDto(s.Id,s.Title,s.Description,s.Position,s.Items.OrderBy(i=>i.Position).Select(i=>new AuditItemDto(i.Id,i.Label,i.Description,i.ResponseType,i.IsRequired,i.Position,i.Options,responses.TryGetValue(i.Id,out var r)?new AuditItemResponseDto(r.Value,r.Observation,r.Recommendation,r.UpdatedAtUtc,r.UpdatedByUserId):null)).ToArray())).ToArray();
  return new AuditDto(x.Id,x.CustomerId,x.TemplateId,x.Reference,x.Title,x.Description,x.Status,x.CreatedAtUtc,sections,answered,items.Length,requiredAnswered,required);
 }
}
