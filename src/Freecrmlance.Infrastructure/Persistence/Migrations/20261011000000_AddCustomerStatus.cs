using Freecrmlance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Freecrmlance.Infrastructure.Persistence.Migrations;

[DbContext(typeof(FreecrmlanceDbContext))]
[Migration("20261011000000_AddCustomerStatus")]
public sealed class AddCustomerStatus : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Status",
            schema: "crm",
            table: "Customers",
            type: "character varying(20)",
            maxLength: 20,
            nullable: false,
            defaultValue: "Client");
        // Existing customers remain clients; new customers default to Prospect in the domain.
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Status", schema: "crm", table: "Customers");
    }
}
