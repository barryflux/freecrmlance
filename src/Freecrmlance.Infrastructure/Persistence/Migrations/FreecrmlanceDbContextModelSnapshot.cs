using Freecrmlance.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
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
        modelBuilder.Entity<IdentityRole>(b => { b.Property<string>("Id"); b.Property<string>("ConcurrencyStamp").IsConcurrencyToken(); b.Property<string>("Name").HasMaxLength(256); b.Property<string>("NormalizedName").HasMaxLength(256); b.HasKey("Id"); b.HasIndex("NormalizedName").IsUnique().HasDatabaseName("RoleNameIndex").HasFilter("\"NormalizedName\" IS NOT NULL"); b.ToTable("AspNetRoles"); });
        modelBuilder.Entity<IdentityUser>(b => { b.Property<string>("Id"); b.Property<int>("AccessFailedCount"); b.Property<string>("ConcurrencyStamp").IsConcurrencyToken(); b.Property<string>("Email").HasMaxLength(256); b.Property<bool>("EmailConfirmed"); b.Property<bool>("LockoutEnabled"); b.Property<DateTimeOffset?>("LockoutEnd"); b.Property<string>("NormalizedEmail").HasMaxLength(256); b.Property<string>("NormalizedUserName").HasMaxLength(256); b.Property<string>("PasswordHash"); b.Property<string>("PhoneNumber"); b.Property<bool>("PhoneNumberConfirmed"); b.Property<string>("SecurityStamp"); b.Property<bool>("TwoFactorEnabled"); b.Property<string>("UserName").HasMaxLength(256); b.HasKey("Id"); b.HasIndex("NormalizedEmail").HasDatabaseName("EmailIndex"); b.HasIndex("NormalizedUserName").IsUnique().HasDatabaseName("UserNameIndex").HasFilter("\"NormalizedUserName\" IS NOT NULL"); b.ToTable("AspNetUsers"); });
        modelBuilder.Entity<IdentityRoleClaim<string>>(b => { b.Property<int>("Id").ValueGeneratedOnAdd(); b.Property<string>("ClaimType"); b.Property<string>("ClaimValue"); b.Property<string>("RoleId").IsRequired(); b.HasKey("Id"); b.HasIndex("RoleId"); b.ToTable("AspNetRoleClaims"); });
        modelBuilder.Entity<IdentityUserClaim<string>>(b => { b.Property<int>("Id").ValueGeneratedOnAdd(); b.Property<string>("ClaimType"); b.Property<string>("ClaimValue"); b.Property<string>("UserId").IsRequired(); b.HasKey("Id"); b.HasIndex("UserId"); b.ToTable("AspNetUserClaims"); });
        modelBuilder.Entity<IdentityUserLogin<string>>(b => { b.Property<string>("LoginProvider").HasMaxLength(128); b.Property<string>("ProviderKey").HasMaxLength(128); b.Property<string>("ProviderDisplayName"); b.Property<string>("UserId").IsRequired(); b.HasKey("LoginProvider","ProviderKey"); b.HasIndex("UserId"); b.ToTable("AspNetUserLogins"); });
        modelBuilder.Entity<IdentityUserRole<string>>(b => { b.Property<string>("UserId"); b.Property<string>("RoleId"); b.HasKey("UserId","RoleId"); b.HasIndex("RoleId"); b.ToTable("AspNetUserRoles"); });
        modelBuilder.Entity<IdentityUserToken<string>>(b => { b.Property<string>("UserId"); b.Property<string>("LoginProvider").HasMaxLength(128); b.Property<string>("Name").HasMaxLength(128); b.Property<string>("Value"); b.HasKey("UserId","LoginProvider","Name"); b.ToTable("AspNetUserTokens"); });
        modelBuilder.Entity<IdentityRoleClaim<string>>(b => b.HasOne<IdentityRole>().WithMany().HasForeignKey("RoleId").OnDelete(DeleteBehavior.Cascade).IsRequired());
        modelBuilder.Entity<IdentityUserClaim<string>>(b => b.HasOne<IdentityUser>().WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.Cascade).IsRequired());
        modelBuilder.Entity<IdentityUserLogin<string>>(b => b.HasOne<IdentityUser>().WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.Cascade).IsRequired());
        modelBuilder.Entity<IdentityUserRole<string>>(b => { b.HasOne<IdentityRole>().WithMany().HasForeignKey("RoleId").OnDelete(DeleteBehavior.Cascade).IsRequired(); b.HasOne<IdentityUser>().WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.Cascade).IsRequired(); });
        modelBuilder.Entity<IdentityUserToken<string>>(b => b.HasOne<IdentityUser>().WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.Cascade).IsRequired());
#pragma warning restore 612, 618
    }
}
