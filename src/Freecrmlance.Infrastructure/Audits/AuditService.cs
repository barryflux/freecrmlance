using Freecrmlance.Application.Audits;using Freecrmlance.Application.Platform;using Freecrmlance.Domain.Audits;using Freecrmlance.Infrastructure.Persistence;using Microsoft.EntityFrameworkCore;using Microsoft.EntityFrameworkCore.Storage;
namespace Freecrmlance.Infrastructure.Audits;
public sealed class AuditService(FreecrmlanceDbContext db,IWorkspaceContext workspaceContext):IAuditService
{
 public async Task<IReadOnlyList<AuditDto>> ListAsync(CancellationToken ct=default){var wid=await workspaceContext.RequireCurrentWorkspaceIdAsync(ct);var rows=await db.Audits.AsNoTracking().Where(x=>x.WorkspaceId==wid).OrderByDescending(x=>x.CreatedAtUtc).ToListAsync(ct);return rows.Select(MapSummary).ToArray();}
 public async Task<AuditDto?> GetAsync(Guid id,CancellationToken ct=default){var wid=await workspaceContext.RequireCurrentWorkspaceIdAsync(ct);var x=await db.Audits.AsNoTracking().Include(a=>a.Sections).ThenInclude(s=>s.Items).SingleOrDefaultAsync(a=>a.Id==id&&a.WorkspaceId==wid,ct);return x is null?null:Map(x);}
 public async Task<Guid?> CreateAsync(CreateAuditCommand command,CancellationToken ct=default)
 {
  var wid=await workspaceContext.RequireCurrentWorkspaceIdAsync(ct);
  var customerExists=await db.Customers.AsNoTracking().AnyAsync(c=>c.Id==command.CustomerId&&c.WorkspaceId==wid,ct);if(!customerExists)return null;
  var template=await db.AuditTemplates.AsNoTracking().Include(t=>t.Sections).ThenInclude(s=>s.Items).SingleOrDefaultAsync(t=>t.Id==command.TemplateId&&t.WorkspaceId==wid&&!t.IsArchived,ct);if(template is null)return null;
  await using var transaction=await db.Database.BeginTransactionAsync(ct);var now=DateTime.UtcNow;
  await using var sql=db.Database.GetDbConnection().CreateCommand();
  sql.Transaction=transaction.GetDbTransaction();
  sql.CommandText =
   """
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
 private static AuditDto MapSummary(Audit x)=>new(x.Id,x.CustomerId,x.TemplateId,x.Reference,x.Title,x.Description,x.Status,x.CreatedAtUtc,[]);
 private static AuditDto Map(Audit x)=>new(x.Id,x.CustomerId,x.TemplateId,x.Reference,x.Title,x.Description,x.Status,x.CreatedAtUtc,x.Sections.OrderBy(s=>s.Position).Select(s=>new AuditSectionDto(s.Id,s.Title,s.Description,s.Position,s.Items.OrderBy(i=>i.Position).Select(i=>new AuditItemDto(i.Id,i.Label,i.Description,i.ResponseType,i.IsRequired,i.Position,i.Options)).ToArray())).ToArray());
}
