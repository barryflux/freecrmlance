using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Freecrmlance.Infrastructure.Persistence;

/// <summary>
/// Creates the DbContext for EF tooling and migration bundles without starting the web host.
/// </summary>
public sealed class FreecrmlanceDbContextFactory : IDesignTimeDbContextFactory<FreecrmlanceDbContext>
{
    public FreecrmlanceDbContext CreateDbContext(string[] args)
    {
        // EF bundle generation needs to construct the context without connecting to a database.
        // At execution time the bundle receives the real connection via the environment
        // or its --connection argument.
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSQL")
            ?? "Host=localhost;Database=alouva_design_time;Username=postgres";

        var options = new DbContextOptionsBuilder<FreecrmlanceDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new FreecrmlanceDbContext(options);
    }
}
