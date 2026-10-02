using Freecrmlance.Application.Audits;
using Freecrmlance.Application.Crm;
using Freecrmlance.Application.Platform;
using Freecrmlance.Domain.Audits;
using Freecrmlance.Domain.Crm;
using Freecrmlance.Infrastructure.Audits;
using Freecrmlance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace Freecrmlance.IntegrationTests.Audits;

public sealed class AuditServiceTests
{
 [Test]
 public async Task Create_snapshots_template_and_generates_sequential_references()
 {
  await using var postgres=new PostgreSqlBuilder().WithImage("postgres:17-alpine").Build();await postgres.StartAsync();
  var options=new DbContextOptionsBuilder<FreecrmlanceDbContext>().UseNpgsql(postgres.GetConnectionString()).Options;
  var wid=Guid.NewGuid();Guid templateId,customerId,auditId;
  await using(var db=new FreecrmlanceDbContext(options))
  {
   await db.Database.MigrateAsync();customerId=await SeedCustomer(db,wid);
   var templates=new AuditTemplateService(db,new StubWorkspaceContext(wid));
   templateId=await templates.CreateAsync(Template("Audit initial","Sécurité","Pare-feu actif ?"));
   var audits=new AuditService(db,new StubWorkspaceContext(wid));
   auditId=(await audits.CreateAsync(new CreateAuditCommand(customerId,templateId)))!.Value;
   var second=(await audits.CreateAsync(new CreateAuditCommand(customerId,templateId)))!.Value;
   Assert.That((await audits.GetAsync(second))!.Reference,Is.EqualTo($"AUD-{DateTime.UtcNow.Year}-0002"));
   await templates.UpdateAsync(templateId,Template("Audit modifié","Réseau","MFA activée ?"));
  }
  await using var verify=new FreecrmlanceDbContext(options);var service=new AuditService(verify,new StubWorkspaceContext(wid));var audit=await service.GetAsync(auditId);
  Assert.That(audit,Is.Not.Null);
  Assert.Multiple(()=>{Assert.That(audit!.Reference,Is.EqualTo($"AUD-{DateTime.UtcNow.Year}-0001"));Assert.That(audit.Status,Is.EqualTo(AuditStatus.Draft));Assert.That(audit.Title,Is.EqualTo("Audit initial"));Assert.That(audit.Sections.Single().Title,Is.EqualTo("Sécurité"));Assert.That(audit.Sections.Single().Items.Single().Label,Is.EqualTo("Pare-feu actif ?"));});
 }
 [Test]
 public async Task Create_rejects_customer_or_template_from_another_workspace()
 {
  await using var postgres=new PostgreSqlBuilder().WithImage("postgres:17-alpine").Build();await postgres.StartAsync();
  var options=new DbContextOptionsBuilder<FreecrmlanceDbContext>().UseNpgsql(postgres.GetConnectionString()).Options;var a=Guid.NewGuid();var b=Guid.NewGuid();
  await using var db=new FreecrmlanceDbContext(options);await db.Database.MigrateAsync();var customerA=await SeedCustomer(db,a);var customerB=await SeedCustomer(db,b);
  var templateA=await new AuditTemplateService(db,new StubWorkspaceContext(a)).CreateAsync(Template("A","S","I"));
  var templateB=await new AuditTemplateService(db,new StubWorkspaceContext(b)).CreateAsync(Template("B","S","I"));
  var service=new AuditService(db,new StubWorkspaceContext(a));
  var foreignCustomer=await service.CreateAsync(new CreateAuditCommand(customerB,templateA));
  var foreignTemplate=await service.CreateAsync(new CreateAuditCommand(customerA,templateB));
  Assert.Multiple(()=>{Assert.That(foreignCustomer,Is.Null);Assert.That(foreignTemplate,Is.Null);});
 }
 private static async Task<Guid> SeedCustomer(FreecrmlanceDbContext db,Guid wid){var c=new Customer(wid,"Client test");db.Customers.Add(c);await db.SaveChangesAsync();return c.Id;}
 private static SaveAuditTemplateCommand Template(string name,string section,string item)=>new(name,"Description",[new AuditTemplateSectionInput(section,"Section",[new AuditTemplateItemInput(item,AuditResponseType.YesNo,true,"Critère",null)])]);
 private sealed class StubWorkspaceContext(Guid id):IWorkspaceContext{public Task<Guid?> GetCurrentWorkspaceIdAsync(CancellationToken ct=default)=>Task.FromResult<Guid?>(id);public Task<Guid> RequireCurrentWorkspaceIdAsync(CancellationToken ct=default)=>Task.FromResult(id);}
}
