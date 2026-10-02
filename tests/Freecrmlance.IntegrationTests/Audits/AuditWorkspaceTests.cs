using Freecrmlance.Application.Audits;using Freecrmlance.Application.Platform;using Freecrmlance.Domain.Audits;using Freecrmlance.Domain.Crm;using Freecrmlance.Infrastructure.Audits;using Freecrmlance.Infrastructure.Persistence;using Microsoft.EntityFrameworkCore;using NUnit.Framework;using Testcontainers.PostgreSql;
namespace Freecrmlance.IntegrationTests.Audits;
public sealed class AuditWorkspaceTests
{
 [Test]public async Task Save_response_persists_resumes_progress_and_starts_audit()
 {
  await using var postgres=new PostgreSqlBuilder().WithImage("postgres:17-alpine").Build();await postgres.StartAsync();var options=new DbContextOptionsBuilder<FreecrmlanceDbContext>().UseNpgsql(postgres.GetConnectionString()).Options;var wid=Guid.NewGuid();Guid auditId,itemId;
  await using(var db=new FreecrmlanceDbContext(options)){await db.Database.MigrateAsync();var customer=new Customer(wid,"Client");db.Customers.Add(customer);var template=new AuditTemplate(wid,"Audit",null);var section=template.AddSection("Section",0);section.AddItem("Contrôle",AuditResponseType.YesNo,true,0);db.AuditTemplates.Add(template);await db.SaveChangesAsync();var service=new AuditService(db,new Stub(wid));auditId=(await service.CreateAsync(new(customer.Id,template.Id)))!.Value;itemId=(await service.GetAsync(auditId))!.Sections.Single().Items.Single().Id;var error=await service.SaveResponseAsync(new(auditId,itemId,"Yes","Constat","Action","user-1"));Assert.That(error,Is.Null);}
  await using var verify=new FreecrmlanceDbContext(options);var audit=await new AuditService(verify,new Stub(wid)).GetAsync(auditId);
  Assert.Multiple(()=>{Assert.That(audit!.Status,Is.EqualTo(AuditStatus.InProgress));Assert.That(audit.ProgressPercent,Is.EqualTo(100));Assert.That(audit.RequiredItemsComplete,Is.True);Assert.That(audit.Sections.Single().Items.Single().Response!.Value,Is.EqualTo("Yes"));Assert.That(audit.Sections.Single().Items.Single().Response!.Observation,Is.EqualTo("Constat"));});
 }
 [Test]public async Task Save_response_rejects_foreign_item_and_invalid_typed_value()
 {
  await using var postgres=new PostgreSqlBuilder().WithImage("postgres:17-alpine").Build();await postgres.StartAsync();var options=new DbContextOptionsBuilder<FreecrmlanceDbContext>().UseNpgsql(postgres.GetConnectionString()).Options;var a=Guid.NewGuid();var b=Guid.NewGuid();
  await using var db=new FreecrmlanceDbContext(options);await db.Database.MigrateAsync();var auditA=SeedAudit(db,a,"A",AuditResponseType.Rating);var auditB=SeedAudit(db,b,"B",AuditResponseType.YesNo);await db.SaveChangesAsync();var service=new AuditService(db,new Stub(a));var invalid=await service.SaveResponseAsync(new(auditA.Id,auditA.Sections.Single().Items.Single().Id,"9",null,null,"user"));var foreign=await service.SaveResponseAsync(new(auditA.Id,auditB.Sections.Single().Items.Single().Id,"Yes",null,null,"user"));
  Assert.Multiple(()=>{Assert.That(invalid,Is.EqualTo("La note doit être comprise entre 1 et 5."));Assert.That(foreign,Is.EqualTo("Critère d'audit introuvable."));});
 }
 private static Audit SeedAudit(FreecrmlanceDbContext db,Guid wid,string reference,AuditResponseType type){var audit=new Audit(wid,Guid.NewGuid(),Guid.NewGuid(),reference,"Audit",null);audit.AddSection("Section",0).AddItem("Critère",type,true,0);db.Audits.Add(audit);return audit;}
 private sealed class Stub(Guid id):IWorkspaceContext{public Task<Guid?> GetCurrentWorkspaceIdAsync(CancellationToken ct=default)=>Task.FromResult<Guid?>(id);public Task<Guid> RequireCurrentWorkspaceIdAsync(CancellationToken ct=default)=>Task.FromResult(id);}
}
