using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace Freecrmlance.Infrastructure.Persistence.Migrations;
public partial class AddAuditReportVersions:Migration
{
 protected override void Up(MigrationBuilder m)
 {
  m.CreateTable(name:"AuditReportVersions",schema:"audit",columns:t=>new{Id=t.Column<Guid>(type:"uuid",nullable:false),AuditId=t.Column<Guid>(type:"uuid",nullable:false),VersionNumber=t.Column<int>(type:"integer",nullable:false),FinalizedAtUtc=t.Column<DateTime>(type:"timestamp with time zone",nullable:false),FinalizedByUserId=t.Column<string>(type:"character varying(450)",maxLength:450,nullable:false),Snapshot=t.Column<string>(type:"jsonb",nullable:false),Hash=t.Column<string>(type:"character varying(64)",maxLength:64,nullable:false),GeneratedDocumentId=t.Column<Guid>(type:"uuid",nullable:true)},constraints:t=>{t.PrimaryKey("PK_AuditReportVersions",x=>x.Id);t.ForeignKey("FK_AuditReportVersions_Audits_AuditId",x=>x.AuditId,principalSchema:"audit",principalTable:"Audits",principalColumn:"Id",onDelete:ReferentialAction.Cascade);t.ForeignKey("FK_AuditReportVersions_Documents_GeneratedDocumentId",x=>x.GeneratedDocumentId,principalSchema:"crm",principalTable:"Documents",principalColumn:"Id",onDelete:ReferentialAction.SetNull);});
  m.CreateIndex(name:"IX_AuditReportVersions_AuditId_VersionNumber",schema:"audit",table:"AuditReportVersions",columns:new[]{"AuditId","VersionNumber"},unique:true);
  m.CreateIndex(name:"IX_AuditReportVersions_GeneratedDocumentId",schema:"audit",table:"AuditReportVersions",column:"GeneratedDocumentId");
 }
 protected override void Down(MigrationBuilder m)=>m.DropTable(name:"AuditReportVersions",schema:"audit");
}
