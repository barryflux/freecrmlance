using Microsoft.EntityFrameworkCore;

namespace Freecrmlance.Infrastructure.Persistence;

public sealed class FreecrmlanceDbContext(DbContextOptions<FreecrmlanceDbContext> options)
    : DbContext(options);
