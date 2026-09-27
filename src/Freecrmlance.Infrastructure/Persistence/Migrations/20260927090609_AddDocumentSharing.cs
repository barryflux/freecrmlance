using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Freecrmlance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentSharing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentShares",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentShares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentShares_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "crm",
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentShares_DocumentId",
                schema: "crm",
                table: "DocumentShares",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentShares_TokenHash",
                schema: "crm",
                table: "DocumentShares",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentShares_WorkspaceId_CustomerId_DocumentId",
                schema: "crm",
                table: "DocumentShares",
                columns: new[] { "WorkspaceId", "CustomerId", "DocumentId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentShares",
                schema: "crm");
        }
    }
}
