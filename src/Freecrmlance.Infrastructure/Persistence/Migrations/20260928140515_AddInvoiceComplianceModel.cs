using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Freecrmlance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceComplianceModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerAddressLine1",
                schema: "billing",
                table: "Invoices",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerAddressLine2",
                schema: "billing",
                table: "Invoices",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerCity",
                schema: "billing",
                table: "Invoices",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerCountryCode",
                schema: "billing",
                table: "Invoices",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerLegalName",
                schema: "billing",
                table: "Invoices",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerPostalCode",
                schema: "billing",
                table: "Invoices",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerSiren",
                schema: "billing",
                table: "Invoices",
                type: "character varying(9)",
                maxLength: 9,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerSiret",
                schema: "billing",
                table: "Invoices",
                type: "character varying(14)",
                maxLength: 14,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerVatNumber",
                schema: "billing",
                table: "Invoices",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                schema: "billing",
                table: "Invoices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EarlyPaymentDiscountTerms",
                schema: "billing",
                table: "Invoices",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "IssueDate",
                schema: "billing",
                table: "Invoices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LatePaymentPenaltyTerms",
                schema: "billing",
                table: "Invoices",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentTerms",
                schema: "billing",
                table: "Invoices",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PurchaseOrderReference",
                schema: "billing",
                table: "Invoices",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RecoveryCostIndemnity",
                schema: "billing",
                table: "Invoices",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerAddressLine1",
                schema: "billing",
                table: "Invoices",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerAddressLine2",
                schema: "billing",
                table: "Invoices",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerCity",
                schema: "billing",
                table: "Invoices",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerContactEmail",
                schema: "billing",
                table: "Invoices",
                type: "character varying(320)",
                maxLength: 320,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerContactPhone",
                schema: "billing",
                table: "Invoices",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerCountryCode",
                schema: "billing",
                table: "Invoices",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerLegalForm",
                schema: "billing",
                table: "Invoices",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerLegalName",
                schema: "billing",
                table: "Invoices",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerPostalCode",
                schema: "billing",
                table: "Invoices",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerSiren",
                schema: "billing",
                table: "Invoices",
                type: "character varying(9)",
                maxLength: 9,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerSiret",
                schema: "billing",
                table: "Invoices",
                type: "character varying(14)",
                maxLength: 14,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerVatNumber",
                schema: "billing",
                table: "Invoices",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ServiceDate",
                schema: "billing",
                table: "Invoices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VatExemptionMention",
                schema: "billing",
                table: "Invoices",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VatRate",
                schema: "billing",
                table: "InvoiceLines",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerAddressLine1",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CustomerAddressLine2",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CustomerCity",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CustomerCountryCode",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CustomerLegalName",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CustomerPostalCode",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CustomerSiren",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CustomerSiret",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CustomerVatNumber",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "DueDate",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "EarlyPaymentDiscountTerms",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "IssueDate",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "LatePaymentPenaltyTerms",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PaymentTerms",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PurchaseOrderReference",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "RecoveryCostIndemnity",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SellerAddressLine1",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SellerAddressLine2",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SellerCity",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SellerContactEmail",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SellerContactPhone",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SellerCountryCode",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SellerLegalForm",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SellerLegalName",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SellerPostalCode",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SellerSiren",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SellerSiret",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SellerVatNumber",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ServiceDate",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "VatExemptionMention",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "VatRate",
                schema: "billing",
                table: "InvoiceLines");
        }
    }
}
