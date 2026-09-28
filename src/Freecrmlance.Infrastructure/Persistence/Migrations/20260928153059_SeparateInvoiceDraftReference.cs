using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Freecrmlance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeparateInvoiceDraftReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Invoices_WorkspaceId_Number",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                schema: "billing",
                table: "Invoices",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "DraftReference",
                schema: "billing",
                table: "Invoices",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            // Preserve the previous invoice number as the draft reference.
            migrationBuilder.Sql(
                """
                UPDATE billing."Invoices"
                SET "DraftReference" = "Number";
                """);

            // Draft invoices do not have a definitive accounting number.
            migrationBuilder.Sql(
                """
                UPDATE billing."Invoices"
                SET "Number" = NULL
                WHERE "Status" = 'Draft';
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_WorkspaceId_Number",
                schema: "billing",
                table: "Invoices",
                columns: new[] { "WorkspaceId", "Number" },
                unique: true,
                filter: "\"Number\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Invoices_WorkspaceId_Number",
                schema: "billing",
                table: "Invoices");

            // Restore the previous model where every invoice had a Number.
            migrationBuilder.Sql(
                """
                UPDATE billing."Invoices"
                SET "Number" = "DraftReference"
                WHERE "Number" IS NULL;
                """);

            migrationBuilder.DropColumn(
                name: "DraftReference",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                schema: "billing",
                table: "Invoices",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_WorkspaceId_Number",
                schema: "billing",
                table: "Invoices",
                columns: new[] { "WorkspaceId", "Number" },
                unique: true);
        }
    }
}