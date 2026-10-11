using Freecrmlance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Freecrmlance.Infrastructure.Persistence.Migrations;

[DbContext(typeof(FreecrmlanceDbContext))]
[Migration("20261011010000_AddCustomerFollowUps")]
public sealed class AddCustomerFollowUps : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(name: "LastContactAt", schema: "crm", table: "Customers", type: "timestamp with time zone", nullable: true);
        migrationBuilder.AddColumn<DateTime>(name: "NextFollowUpAt", schema: "crm", table: "Customers", type: "timestamp with time zone", nullable: true);
        migrationBuilder.AddColumn<string>(name: "FollowUpNote", schema: "crm", table: "Customers", type: "character varying(2000)", maxLength: 2000, nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "LastContactAt", schema: "crm", table: "Customers");
        migrationBuilder.DropColumn(name: "NextFollowUpAt", schema: "crm", table: "Customers");
        migrationBuilder.DropColumn(name: "FollowUpNote", schema: "crm", table: "Customers");
    }
}
