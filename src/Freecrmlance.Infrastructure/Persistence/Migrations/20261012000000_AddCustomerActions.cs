using System;
using Freecrmlance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace Freecrmlance.Infrastructure.Persistence.Migrations;

[DbContext(typeof(FreecrmlanceDbContext))]
[Migration("20261012000000_AddCustomerActions")]
public sealed class AddCustomerActions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(name: "crm");
        migrationBuilder.CreateTable(
            name: "CustomerActions", schema: "crm",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CustomerActions", x => x.Id);
                table.ForeignKey(name: "FK_CustomerActions_Customers_CustomerId", column: x => x.CustomerId, principalSchema: "crm", principalTable: "Customers", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            });
        migrationBuilder.CreateIndex(name: "IX_CustomerActions_CustomerId", schema: "crm", table: "CustomerActions", column: "CustomerId");
        migrationBuilder.CreateIndex(name: "IX_CustomerActions_WorkspaceId_CustomerId_Status", schema: "crm", table: "CustomerActions", columns: new[] { "WorkspaceId", "CustomerId", "Status" });
    }
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "CustomerActions", schema: "crm");
}
