using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace Freecrmlance.Infrastructure.Persistence.Migrations;
public partial class AddAuditItemResponses:Migration
{
 protected override void Up(MigrationBuilder m)
 {
  m.CreateTable(name:"AuditItemResponses",schema:"audit",columns:t=>new{Id=t.Column<Guid>(type:"uuid",nullable:false),AuditItemId=t.Column<Guid>(type:"uuid",nullable:false),Value=t.Column<string>(type:"character varying(4000)",maxLength:4000,nullable:true),Observation=t.Column<string>(type:"character varying(4000)",maxLength:4000,nullable:true),Recommendation=t.Column<string>(type:"character varying(4000)",maxLength:4000,nullable:true),UpdatedAtUtc=t.Column<DateTime>(type:"timestamp with time zone",nullable:false),UpdatedByUserId=t.Column<string>(type:"character varying(450)",maxLength:450,nullable:false)},constraints:t=>{t.PrimaryKey("PK_AuditItemResponses",x=>x.Id);t.ForeignKey("FK_AuditItemResponses_AuditItems_AuditItemId",x=>x.AuditItemId,principalSchema:"audit",principalTable:"AuditItems",principalColumn:"Id",onDelete:ReferentialAction.Cascade);});
  m.CreateIndex(name:"IX_AuditItemResponses_AuditItemId",schema:"audit",table:"AuditItemResponses",column:"AuditItemId",unique:true);
 }
 protected override void Down(MigrationBuilder m)=>m.DropTable(name:"AuditItemResponses",schema:"audit");
}
