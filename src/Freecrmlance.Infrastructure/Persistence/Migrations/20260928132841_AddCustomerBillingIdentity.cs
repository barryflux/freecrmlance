using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Freecrmlance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerBillingIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddressLine1",
                schema: "crm",
                table: "Customers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressLine2",
                schema: "crm",
                table: "Customers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingAddressLine1",
                schema: "crm",
                table: "Customers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingAddressLine2",
                schema: "crm",
                table: "Customers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingCity",
                schema: "crm",
                table: "Customers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingCountryCode",
                schema: "crm",
                table: "Customers",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingPostalCode",
                schema: "crm",
                table: "Customers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                schema: "crm",
                table: "Customers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                schema: "crm",
                table: "Customers",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                schema: "crm",
                table: "Customers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Siren",
                schema: "crm",
                table: "Customers",
                type: "character varying(9)",
                maxLength: 9,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Siret",
                schema: "crm",
                table: "Customers",
                type: "character varying(14)",
                maxLength: 14,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                schema: "crm",
                table: "Customers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VatNumber",
                schema: "crm",
                table: "Customers",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressLine1",
                schema: "crm",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AddressLine2",
                schema: "crm",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "BillingAddressLine1",
                schema: "crm",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "BillingAddressLine2",
                schema: "crm",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "BillingCity",
                schema: "crm",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "BillingCountryCode",
                schema: "crm",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "BillingPostalCode",
                schema: "crm",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "City",
                schema: "crm",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                schema: "crm",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                schema: "crm",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Siren",
                schema: "crm",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Siret",
                schema: "crm",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Type",
                schema: "crm",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "VatNumber",
                schema: "crm",
                table: "Customers");
        }
    }
}
