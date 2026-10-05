using Freecrmlance.Application.Platform;using Freecrmlance.Domain.Audits;using Freecrmlance.Domain.Crm;using Freecrmlance.Infrastructure.Audits;using Freecrmlance.Infrastructure.Persistence;using Microsoft.EntityFrameworkCore;using NUnit.Framework;using Testcontainers.PostgreSql;
namespace Freecrmlance.IntegrationTests.Audits;
public sealed class AuditHistoryTests
{
 [Test]public async Task Timeline_combines_immutable_facts_and_append_only_events()
 {
  await using var pg=new PostgreSqlBuilder().WithImage("postgres:17-alpine").Build();await pg.StartAsync();var o=new DbContextOptionsBuilder<FreecrmlanceDbContext>().UseNpgsql(pg.GetConnectionString()).Options;var wid=Guid.NewGuid();await using var db=new FreecrmlanceDbContext(o);await db.Database.MigrateAsync();var customer=new Customer(wid,"Client");db.Customers.Add(customer);var audit=new Audit(wid,customer.Id,Guid.NewGuid(),"AUD-HIST","Audit",null);db.Audits.Add(audit);await db.SaveChangesAsync();var finalized=DateTime.UtcNow.AddMinutes(-3);var version=new AuditReportVersion(audit.Id,1,finalized,"finalizer","{}",new string('a',64));db.AuditReportVersions.Add(version);db.AuditHistoryEvents.Add(new AuditHistoryEvent(wid,audit.Id,AuditHistoryEventType.ReportGenerated,DateTime.UtcNow.AddMinutes(-2),null,version.Id));await db.SaveChangesAsync();var service=new AuditHistoryService(db,new StubWorkspace(wid));var history=await service.ListAsync(audit.Id);Assert.That(history,Is.Not.Null);var entries=history!;Assert.That(entries.Select(x=>x.Type),Does.Contain("Created"));Assert.That(entries.Select(x=>x.Type),Does.Contain("Finalized"));Assert.That(entries.Select(x=>x.Type),Does.Contain("ReportGenerated"));Assert.That(entries,Is.Ordered.Descending.By("OccurredAtUtc"));
 }
 [Test]public async Task Foreign_workspace_cannot_read_history()
 {
  await using var pg=new PostgreSqlBuilder().WithImage("postgres:17-alpine").Build();await pg.StartAsync();var o=new DbContextOptionsBuilder<FreecrmlanceDbContext>().UseNpgsql(pg.GetConnectionString()).Options;var wid=Guid.NewGuid();await using var db=new FreecrmlanceDbContext(o);await db.Database.MigrateAsync();var customer=new Customer(wid,"Client");db.Customers.Add(customer);var audit=new Audit(wid,customer.Id,Guid.NewGuid(),"AUD-X","Audit",null);db.Audits.Add(audit);await db.SaveChangesAsync();var service=new AuditHistoryService(db,new StubWorkspace(Guid.NewGuid()));Assert.That(await service.ListAsync(audit.Id),Is.Null);
 }
 private sealed class StubWorkspace(Guid id):IWorkspaceContext{public Task<Guid?> GetCurrentWorkspaceIdAsync(CancellationToken ct=default)=>Task.FromResult<Guid?>(id);public Task<Guid> RequireCurrentWorkspaceIdAsync(CancellationToken ct=default)=>Task.FromResult(id);}
}
