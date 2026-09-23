using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Freecrmlance.Infrastructure.Persistence;

public sealed class FreecrmlanceDbContext(DbContextOptions<FreecrmlanceDbContext> options)
    : IdentityDbContext(options);
