using Freecrmlance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace Freecrmlance.Infrastructure.Persistence.Migrations;

[DbContext(typeof(FreecrmlanceDbContext))]
partial class FreecrmlanceDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder.HasAnnotation("ProductVersion", "9.0.9");
#pragma warning restore 612, 618
    }
}
