using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace Freecrmlance.Infrastructure.Persistence.Migrations;
public partial class AddAuditEvidence:Migration
{
 protected override void Up(MigrationBuilder m)
 {
  m.CreateTable(name:"AuditEvidence",schema:"audit",columns:t=>new{Id=t.Column<Guid>(type:"uuid",nullable:false),AuditItemId=t.Column<Guid>(type:"uuid",nullable:false),DocumentId=t.Column<Guid>(type:"uuid",nullable:false),UploadedByUserId=t.Column<string>(type:"character varying(450)",maxLength:450,nullable:false),CreatedAtUtc=t.Column<DateTime>(type:"timestamp with time zone",nullable:false)},constraints:t=>{t.PrimaryKey("PK_AuditEvidence",x=>x.Id);t.ForeignKey("FK_AuditEvidence_AuditItems_AuditItemId",x=>x.AuditItemId,principalSchema:"audit",principalTable:"AuditItems",principalColumn:"Id",onDelete:ReferentialAction.Cascade);t.ForeignKey("FK_AuditEvidence_Documents_DocumentId",x=>x.DocumentId,principalSchema:"crm",principalTable:"Documents",principalColumn:"Id",onDelete:ReferentialAction.Cascade);});
  m.CreateIndex(name:"IX_AuditEvidence_AuditItemId",schema:"audit",table:"AuditEvidence",column:"AuditItemId");
  m.CreateIndex(name:"IX_AuditEvidence_DocumentId",schema:"audit",table:"AuditEvidence",column:"DocumentId",unique:true);
 }
 protected override void Down(MigrationBuilder m)=>m.DropTable(name:"AuditEvidence",schema:"audit");
}
