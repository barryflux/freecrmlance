using Freecrmlance.Infrastructure;
using Freecrmlance.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace Freecrmlance.IntegrationTests.Identity;

public sealed class IdentityTests
{
    [Test]
    public async Task User_can_be_created_and_password_verified_on_PostgreSQL()
    {
        await using var postgres = new PostgreSqlBuilder().WithImage("postgres:17-alpine").Build();
        await postgres.StartAsync();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:PostgreSQL"] = postgres.GetConnectionString() })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(configuration);

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<FreecrmlanceDbContext>();
        await db.Database.MigrateAsync();

        var migrations = await db.Database.GetAppliedMigrationsAsync();
        Assert.That(migrations, Does.Contain("20260923162500_AddIdentity"));

        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var user = new IdentityUser { UserName = "test@example.com", Email = "test@example.com" };

        var created = await users.CreateAsync(user, "ValidPassword123!");
        Assert.That(created.Succeeded, Is.True);
        Assert.That(await users.CheckPasswordAsync(user, "ValidPassword123!"), Is.True);
        Assert.That(await users.CheckPasswordAsync(user, "WrongPassword123!"), Is.False);
    }
}
