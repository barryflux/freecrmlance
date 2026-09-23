using Freecrmlance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace Freecrmlance.IntegrationTests.Persistence;

public sealed class MigrationsTests
{
    [Test]
    public async Task Migrations_can_be_applied_to_an_empty_PostgreSQL_database()
    {
        var postgres = new PostgreSqlBuilder("postgres:17-alpine")
            .Build();

        await postgres.StartAsync();

        try
        {
            var options = new DbContextOptionsBuilder<FreecrmlanceDbContext>()
                .UseNpgsql(postgres.GetConnectionString())
                .Options;

            await using var dbContext = new FreecrmlanceDbContext(options);

            await dbContext.Database.MigrateAsync();

            Assert.That(await dbContext.Database.CanConnectAsync(), Is.True);

            var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync();
            Assert.That(appliedMigrations, Does.Contain("20260923162000_InitialPersistence"));
        }
        finally
        {
            await postgres.DisposeAsync();
        }
    }
}
