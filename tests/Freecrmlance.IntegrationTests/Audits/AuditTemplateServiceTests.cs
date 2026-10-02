using Freecrmlance.Application.Audits;
using Freecrmlance.Application.Platform;
using Freecrmlance.Domain.Audits;
using Freecrmlance.Infrastructure.Audits;
using Freecrmlance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace Freecrmlance.IntegrationTests.Audits;

public sealed class AuditTemplateServiceTests
{
    [Test]
    public async Task Update_without_changes_keeps_existing_template_graph()
    {
        await using var postgres = new PostgreSqlBuilder().WithImage("postgres:17-alpine").Build();
        await postgres.StartAsync();
        var options = new DbContextOptionsBuilder<FreecrmlanceDbContext>().UseNpgsql(postgres.GetConnectionString()).Options;

        var workspaceId = Guid.NewGuid();
        Guid templateId;
        var command = Template("Audit sécurité", "Infrastructure", "Pare-feu actif ?");

        await using (var db = new FreecrmlanceDbContext(options))
        {
            await db.Database.MigrateAsync();
            var service = new AuditTemplateService(db, new StubWorkspaceContext(workspaceId));
            templateId = await service.CreateAsync(command);
            Assert.That(await service.UpdateAsync(templateId, command), Is.True);
        }

        await using var verificationDb = new FreecrmlanceDbContext(options);
        var saved = await verificationDb.AuditTemplates.AsNoTracking()
            .Include(t => t.Sections).ThenInclude(s => s.Items)
            .SingleAsync(t => t.Id == templateId);

        Assert.Multiple(() =>
        {
            Assert.That(saved.Name, Is.EqualTo("Audit sécurité"));
            Assert.That(saved.Sections, Has.Count.EqualTo(1));
            Assert.That(saved.Sections.Single().Items, Has.Count.EqualTo(1));
            Assert.That(saved.Sections.Single().Items.Single().Label, Is.EqualTo("Pare-feu actif ?"));
        });
    }

    [Test]
    public async Task Update_with_changes_replaces_template_graph_and_persists_changes()
    {
        await using var postgres = new PostgreSqlBuilder().WithImage("postgres:17-alpine").Build();
        await postgres.StartAsync();
        var options = new DbContextOptionsBuilder<FreecrmlanceDbContext>().UseNpgsql(postgres.GetConnectionString()).Options;

        var workspaceId = Guid.NewGuid();
        Guid templateId;

        await using (var db = new FreecrmlanceDbContext(options))
        {
            await db.Database.MigrateAsync();
            var service = new AuditTemplateService(db, new StubWorkspaceContext(workspaceId));
            templateId = await service.CreateAsync(Template("Audit sécurité", "Infrastructure", "Pare-feu actif ?"));

            var changed = Template("Audit sécurité v2", "Réseau", "MFA activée ?");
            Assert.That(await service.UpdateAsync(templateId, changed), Is.True);
        }

        await using var verificationDb = new FreecrmlanceDbContext(options);
        var saved = await verificationDb.AuditTemplates.AsNoTracking()
            .Include(t => t.Sections).ThenInclude(s => s.Items)
            .SingleAsync(t => t.Id == templateId);

        var section = saved.Sections.Single();
        var item = section.Items.Single();
        Assert.Multiple(() =>
        {
            Assert.That(saved.Name, Is.EqualTo("Audit sécurité v2"));
            Assert.That(saved.Sections, Has.Count.EqualTo(1));
            Assert.That(section.Title, Is.EqualTo("Réseau"));
            Assert.That(section.Items, Has.Count.EqualTo(1));
            Assert.That(item.Label, Is.EqualTo("MFA activée ?"));
            Assert.That(item.ResponseType, Is.EqualTo(AuditResponseType.YesNo));
            Assert.That(item.IsRequired, Is.True);
        });
    }

    private static SaveAuditTemplateCommand Template(string name, string section, string item) =>
        new(name, "Modèle de test",
        [
            new AuditTemplateSectionInput(section, null,
            [
                new AuditTemplateItemInput(item, AuditResponseType.YesNo, true, null, null)
            ])
        ]);

    private sealed class StubWorkspaceContext(Guid workspaceId) : IWorkspaceContext
    {
        public Task<Guid?> GetCurrentWorkspaceIdAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<Guid?>(workspaceId);

        public Task<Guid> RequireCurrentWorkspaceIdAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(workspaceId);
    }
}
