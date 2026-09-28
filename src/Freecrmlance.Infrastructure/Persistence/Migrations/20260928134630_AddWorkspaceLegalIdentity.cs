using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Freecrmlance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkspaceLegalIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddressLine1",
                schema: "platform",
                table: "Workspaces",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressLine2",
                schema: "platform",
                table: "Workspaces",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                schema: "platform",
                table: "Workspaces",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                schema: "platform",
                table: "Workspaces",
                type: "character varying(320)",
                maxLength: 320,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                schema: "platform",
                table: "Workspaces",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                schema: "platform",
                table: "Workspaces",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegalForm",
                schema: "platform",
                table: "Workspaces",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegalName",
                schema: "platform",
                table: "Workspaces",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                schema: "platform",
                table: "Workspaces",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Siren",
                schema: "platform",
                table: "Workspaces",
                type: "character varying(9)",
                maxLength: 9,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Siret",
                schema: "platform",
                table: "Workspaces",
                type: "character varying(14)",
                maxLength: 14,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VatNumber",
                schema: "platform",
                table: "Workspaces",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressLine1",
                schema: "platform",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "AddressLine2",
                schema: "platform",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "City",
                schema: "platform",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "ContactEmail",
                schema: "platform",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                schema: "platform",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                schema: "platform",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "LegalForm",
                schema: "platform",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "LegalName",
                schema: "platform",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                schema: "platform",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "Siren",
                schema: "platform",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "Siret",
                schema: "platform",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "VatNumber",
                schema: "platform",
                table: "Workspaces");
        }
    }
}
