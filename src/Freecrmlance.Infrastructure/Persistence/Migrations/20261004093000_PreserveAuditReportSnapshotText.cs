using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace Freecrmlance.Infrastructure.Persistence.Migrations;
public partial class PreserveAuditReportSnapshotText : Migration
{
 protected override void Up(MigrationBuilder migrationBuilder)
 {
  migrationBuilder.Sql("""
ALTER TABLE audit."AuditReportVersions"
ALTER COLUMN "Snapshot" TYPE text
USING "Snapshot"::text;
""");
 }
 protected override void Down(MigrationBuilder migrationBuilder)
 {
  migrationBuilder.Sql("""
ALTER TABLE audit."AuditReportVersions"
ALTER COLUMN "Snapshot" TYPE jsonb
USING "Snapshot"::jsonb;
""");
 }
}
