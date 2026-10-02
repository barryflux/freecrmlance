using Freecrmlance.Application.Audits;
using Freecrmlance.Application.Platform;
using Freecrmlance.Domain.Audits;
using Freecrmlance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Freecrmlance.Infrastructure.Audits;

public sealed class AuditTemplateService(FreecrmlanceDbContext db, IWorkspaceContext workspaceContext) : IAuditTemplateService
{
 public async Task<IReadOnlyList<AuditTemplateDto>> ListAsync(bool includeArchived=false,CancellationToken ct=default)
 {
  var wid=await workspaceContext.RequireCurrentWorkspaceIdAsync(ct);
  var q=db.AuditTemplates.AsNoTracking().Where(x=>x.WorkspaceId==wid);
  if(!includeArchived) q=q.Where(x=>!x.IsArchived);
  var rows=await q.OrderBy(x=>x.Name).ToListAsync(ct);
  return rows.Select(MapSummary).ToArray();
 }
 public async Task<AuditTemplateDto?> GetAsync(Guid id,CancellationToken ct=default)
 {
  var x=await GetEntityAsync(id,true,ct); return x is null?null:Map(x);
 }
 public async Task<Guid> CreateAsync(SaveAuditTemplateCommand command,CancellationToken ct=default)
 {
  var wid=await workspaceContext.RequireCurrentWorkspaceIdAsync(ct); var x=Build(wid,command);
  db.AuditTemplates.Add(x); await db.SaveChangesAsync(ct); return x.Id;
 }
 public async Task<bool> UpdateAsync(Guid id,SaveAuditTemplateCommand command,CancellationToken ct=default)
 {
  var x=await GetEntityAsync(id,true,ct); if(x is null)return false;
  if(IsUnchanged(x,command))return true;

  // From here on the submitted template actually changed.
  db.ChangeTracker.Clear();
  x=await GetEntityAsync(id,false,ct); if(x is null)return false;
  await using var transaction=await db.Database.BeginTransactionAsync(ct);

  x.Update(command.Name,command.Description);
  await db.SaveChangesAsync(ct);

  var sectionIds=await db.AuditTemplateSections
   .Where(s=>s.AuditTemplateId==x.Id)
   .Select(s=>s.Id)
   .ToArrayAsync(ct);

  if(sectionIds.Length>0)
  {
   await db.AuditTemplateItems.Where(i=>sectionIds.Contains(i.SectionId)).ExecuteDeleteAsync(ct);
   await db.AuditTemplateSections.Where(s=>s.AuditTemplateId==x.Id).ExecuteDeleteAsync(ct);
  }

  // ExecuteDeleteAsync bypasses EF's change tracker. Clear it, then reload
  // only the root so the replacement children are the only tracked dependants.
  db.ChangeTracker.Clear();
  x=await GetEntityAsync(id,false,ct);
  if(x is null)
  {
   await transaction.RollbackAsync(ct);
   return false;
  }

  foreach(var s in command.Sections)
  {
   var section=x.AddSection(s.Title,s.Description);
   foreach(var i in s.Items)
    section.AddItem(i.Label,i.ResponseType,i.IsRequired,i.Description,i.Options);
  }

  await db.SaveChangesAsync(ct);
  await transaction.CommitAsync(ct);
  return true;
 }
 public async Task<Guid?> DuplicateAsync(Guid id,CancellationToken ct=default)
 {
  var source=await GetEntityAsync(id,true,ct); if(source is null)return null;
  var copy=new AuditTemplate(source.WorkspaceId,source.Name+" — copie",source.Description);
  foreach(var s in source.Sections.OrderBy(x=>x.Position)){var ns=copy.AddSection(s.Title,s.Description);foreach(var i in s.Items.OrderBy(x=>x.Position))ns.AddItem(i.Label,i.ResponseType,i.IsRequired,i.Description,i.Options);}
  db.AuditTemplates.Add(copy); await db.SaveChangesAsync(ct); return copy.Id;
 }
 public async Task<bool> SetArchivedAsync(Guid id,bool archived,CancellationToken ct=default)
 {
  var x=await GetEntityAsync(id,false,ct); if(x is null)return false;
  if(archived)x.Archive();else x.Restore(); await db.SaveChangesAsync(ct); return true;
 }
 private async Task<AuditTemplate?> GetEntityAsync(Guid id,bool graph,CancellationToken ct)
 {
  var wid=await workspaceContext.RequireCurrentWorkspaceIdAsync(ct);
  IQueryable<AuditTemplate> q=db.AuditTemplates;
  if(graph)q=q.Include(x=>x.Sections).ThenInclude(x=>x.Items);
  return await q.SingleOrDefaultAsync(x=>x.Id==id&&x.WorkspaceId==wid,ct);
 }
 private static bool IsUnchanged(AuditTemplate template,SaveAuditTemplateCommand command)
 {
  if(!string.Equals(template.Name,command.Name.Trim(),StringComparison.Ordinal))return false;
  if(!string.Equals(Normalize(template.Description),Normalize(command.Description),StringComparison.Ordinal))return false;

  var sections=template.Sections.OrderBy(s=>s.Position).ToArray();
  if(sections.Length!=command.Sections.Count)return false;

  for(var s=0;s<sections.Length;s++)
  {
   var existingSection=sections[s];
   var submittedSection=command.Sections[s];
   if(!string.Equals(existingSection.Title,submittedSection.Title.Trim(),StringComparison.Ordinal))return false;
   if(!string.Equals(Normalize(existingSection.Description),Normalize(submittedSection.Description),StringComparison.Ordinal))return false;

   var items=existingSection.Items.OrderBy(i=>i.Position).ToArray();
   if(items.Length!=submittedSection.Items.Count)return false;
   for(var i=0;i<items.Length;i++)
   {
    var existingItem=items[i];
    var submittedItem=submittedSection.Items[i];
    if(!string.Equals(existingItem.Label,submittedItem.Label.Trim(),StringComparison.Ordinal)
       || existingItem.ResponseType!=submittedItem.ResponseType
       || existingItem.IsRequired!=submittedItem.IsRequired
       || !string.Equals(Normalize(existingItem.Description),Normalize(submittedItem.Description),StringComparison.Ordinal)
       || !string.Equals(Normalize(existingItem.Options),Normalize(submittedItem.Options),StringComparison.Ordinal))
     return false;
   }
  }
  return true;
 }
 private static string? Normalize(string? value)=>string.IsNullOrWhiteSpace(value)?null:value.Trim();

 private static AuditTemplate Build(Guid wid,SaveAuditTemplateCommand c)
 {
  var x=new AuditTemplate(wid,c.Name,c.Description);
  foreach(var s in c.Sections){var section=x.AddSection(s.Title,s.Description);foreach(var i in s.Items)section.AddItem(i.Label,i.ResponseType,i.IsRequired,i.Description,i.Options);}
  return x;
 }
 private static AuditTemplateDto MapSummary(AuditTemplate x)=>new(x.Id,x.Name,x.Description,x.IsArchived,x.UpdatedAtUtc,[]);
 private static AuditTemplateDto Map(AuditTemplate x)=>new(x.Id,x.Name,x.Description,x.IsArchived,x.UpdatedAtUtc,
  x.Sections.OrderBy(s=>s.Position).Select(s=>new AuditTemplateSectionDto(s.Id,s.Title,s.Description,s.Position,
   s.Items.OrderBy(i=>i.Position).Select(i=>new AuditTemplateItemDto(i.Id,i.Label,i.Description,i.ResponseType,i.IsRequired,i.Position,i.Options)).ToArray())).ToArray());
}