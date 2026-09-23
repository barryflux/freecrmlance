using Freecrmlance.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Freecrmlance.Infrastructure.Persistence.Migrations;

[DbContext(typeof(FreecrmlanceDbContext))]
partial class FreecrmlanceDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "9.0.9")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        modelBuilder.Entity<IdentityRole>(b => {
            b.Property<string>("Id").HasColumnType("text");
            b.Property<string>("ConcurrencyStamp").IsConcurrencyToken().HasColumnType("text");
            b.Property<string>("Name").HasMaxLength(256).HasColumnType("character varying(256)");
            b.Property<string>("NormalizedName").HasMaxLength(256).HasColumnType("character varying(256)");
            b.HasKey("Id");
            b.HasIndex("NormalizedName").IsUnique().HasDatabaseName("RoleNameIndex").HasFilter("\"NormalizedName\" IS NOT NULL");
            b.ToTable("AspNetRoles", (string)null);
        });

        modelBuilder.Entity<IdentityUser>(b => {
            b.Property<string>("Id").HasColumnType("text");
            b.Property<int>("AccessFailedCount").HasColumnType("integer");
            b.Property<string>("ConcurrencyStamp").IsConcurrencyToken().HasColumnType("text");
            b.Property<string>("Email").HasMaxLength(256).HasColumnType("character varying(256)");
            b.Property<bool>("EmailConfirmed").HasColumnType("boolean");
            b.Property<bool>("LockoutEnabled").HasColumnType("boolean");
            b.Property<DateTimeOffset?>("LockoutEnd").HasColumnType("timestamp with time zone");
            b.Property<string>("NormalizedEmail").HasMaxLength(256).HasColumnType("character varying(256)");
            b.Property<string>("NormalizedUserName").HasMaxLength(256).HasColumnType("character varying(256)");
            b.Property<string>("PasswordHash").HasColumnType("text");
            b.Property<string>("PhoneNumber").HasColumnType("text");
            b.Property<bool>("PhoneNumberConfirmed").HasColumnType("boolean");
            b.Property<string>("SecurityStamp").HasColumnType("text");
            b.Property<bool>("TwoFactorEnabled").HasColumnType("boolean");
            b.Property<string>("UserName").HasMaxLength(256).HasColumnType("character varying(256)");
            b.HasKey("Id");
            b.HasIndex("NormalizedEmail").HasDatabaseName("EmailIndex");
            b.HasIndex("NormalizedUserName").IsUnique().HasDatabaseName("UserNameIndex").HasFilter("\"NormalizedUserName\" IS NOT NULL");
            b.ToTable("AspNetUsers", (string)null);
        });

        modelBuilder.Entity<IdentityRoleClaim<string>>(b => {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer");
            NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
            b.Property<string>("ClaimType").HasColumnType("text");
            b.Property<string>("ClaimValue").HasColumnType("text");
            b.Property<string>("RoleId").IsRequired().HasColumnType("text");
            b.HasKey("Id"); b.HasIndex("RoleId"); b.ToTable("AspNetRoleClaims", (string)null);
        });
        modelBuilder.Entity<IdentityUserClaim<string>>(b => {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer");
            NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
            b.Property<string>("ClaimType").HasColumnType("text"); b.Property<string>("ClaimValue").HasColumnType("text");
            b.Property<string>("UserId").IsRequired().HasColumnType("text");
            b.HasKey("Id"); b.HasIndex("UserId"); b.ToTable("AspNetUserClaims", (string)null);
        });
        modelBuilder.Entity<IdentityUserLogin<string>>(b => {
            b.Property<string>("LoginProvider").HasMaxLength(128).HasColumnType("character varying(128)");
            b.Property<string>("ProviderKey").HasMaxLength(128).HasColumnType("character varying(128)");
            b.Property<string>("ProviderDisplayName").HasColumnType("text"); b.Property<string>("UserId").IsRequired().HasColumnType("text");
            b.HasKey("LoginProvider","ProviderKey"); b.HasIndex("UserId"); b.ToTable("AspNetUserLogins", (string)null);
        });
        modelBuilder.Entity<IdentityUserRole<string>>(b => {
            b.Property<string>("UserId").HasColumnType("text"); b.Property<string>("RoleId").HasColumnType("text");
            b.HasKey("UserId","RoleId"); b.HasIndex("RoleId"); b.ToTable("AspNetUserRoles", (string)null);
        });
        modelBuilder.Entity<IdentityUserToken<string>>(b => {
            b.Property<string>("UserId").HasColumnType("text");
            b.Property<string>("LoginProvider").HasMaxLength(128).HasColumnType("character varying(128)");
            b.Property<string>("Name").HasMaxLength(128).HasColumnType("character varying(128)");
            b.Property<string>("Value").HasColumnType("text");
            b.HasKey("UserId","LoginProvider","Name"); b.ToTable("AspNetUserTokens", (string)null);
        });

        modelBuilder.Entity<IdentityRoleClaim<string>>(b => b.HasOne<IdentityRole>().WithMany().HasForeignKey("RoleId").OnDelete(DeleteBehavior.Cascade).IsRequired());
        modelBuilder.Entity<IdentityUserClaim<string>>(b => b.HasOne<IdentityUser>().WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.Cascade).IsRequired());
        modelBuilder.Entity<IdentityUserLogin<string>>(b => b.HasOne<IdentityUser>().WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.Cascade).IsRequired());
        modelBuilder.Entity<IdentityUserRole<string>>(b => { b.HasOne<IdentityRole>().WithMany().HasForeignKey("RoleId").OnDelete(DeleteBehavior.Cascade).IsRequired(); b.HasOne<IdentityUser>().WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.Cascade).IsRequired(); });
        modelBuilder.Entity<IdentityUserToken<string>>(b => b.HasOne<IdentityUser>().WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.Cascade).IsRequired());
#pragma warning restore 612, 618
    }
}
