using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace Freecrmlance.Infrastructure.Persistence.Migrations;
public partial class AddAuditTemplates : Migration
{
 protected override void Up(MigrationBuilder m)
 {
  m.EnsureSchema(name:"audit");
  m.CreateTable(name:"AuditTemplates",schema:"audit",columns:t=>new{Id=t.Column<Guid>(type:"uuid",nullable:false),WorkspaceId=t.Column<Guid>(type:"uuid",nullable:false),Name=t.Column<string>(type:"character varying(200)",maxLength:200,nullable:false),Description=t.Column<string>(type:"character varying(2000)",maxLength:2000,nullable:true),IsArchived=t.Column<bool>(type:"boolean",nullable:false),CreatedAtUtc=t.Column<DateTime>(type:"timestamp with time zone",nullable:false),UpdatedAtUtc=t.Column<DateTime>(type:"timestamp with time zone",nullable:false)},constraints:t=>t.PrimaryKey("PK_AuditTemplates",x=>x.Id));
  m.CreateTable(name:"AuditTemplateSections",schema:"audit",columns:t=>new{Id=t.Column<Guid>(type:"uuid",nullable:false),AuditTemplateId=t.Column<Guid>(type:"uuid",nullable:false),Title=t.Column<string>(type:"character varying(200)",maxLength:200,nullable:false),Description=t.Column<string>(type:"character varying(2000)",maxLength:2000,nullable:true),Position=t.Column<int>(type:"integer",nullable:false)},constraints:t=>{t.PrimaryKey("PK_AuditTemplateSections",x=>x.Id);t.ForeignKey("FK_AuditTemplateSections_AuditTemplates_AuditTemplateId",x=>x.AuditTemplateId,principalSchema:"audit",principalTable:"AuditTemplates",principalColumn:"Id",onDelete:ReferentialAction.Cascade);});
  m.CreateTable(name:"AuditTemplateItems",schema:"audit",columns:t=>new{Id=t.Column<Guid>(type:"uuid",nullable:false),SectionId=t.Column<Guid>(type:"uuid",nullable:false),Label=t.Column<string>(type:"character varying(500)",maxLength:500,nullable:false),Description=t.Column<string>(type:"character varying(2000)",maxLength:2000,nullable:true),ResponseType=t.Column<string>(type:"character varying(32)",maxLength:32,nullable:false),IsRequired=t.Column<bool>(type:"boolean",nullable:false),Position=t.Column<int>(type:"integer",nullable:false),Options=t.Column<string>(type:"character varying(4000)",maxLength:4000,nullable:true)},constraints:t=>{t.PrimaryKey("PK_AuditTemplateItems",x=>x.Id);t.ForeignKey("FK_AuditTemplateItems_AuditTemplateSections_SectionId",x=>x.SectionId,principalSchema:"audit",principalTable:"AuditTemplateSections",principalColumn:"Id",onDelete:ReferentialAction.Cascade);});
  m.CreateIndex(name:"IX_AuditTemplates_WorkspaceId_IsArchived",schema:"audit",table:"AuditTemplates",columns:new[]{"WorkspaceId","IsArchived"});
  m.CreateIndex(name:"IX_AuditTemplateSections_AuditTemplateId_Position",schema:"audit",table:"AuditTemplateSections",columns:new[]{"AuditTemplateId","Position"},unique:true);
  m.CreateIndex(name:"IX_AuditTemplateItems_SectionId_Position",schema:"audit",table:"AuditTemplateItems",columns:new[]{"SectionId","Position"},unique:true);
 }
 protected override void Down(MigrationBuilder m){m.DropTable(name:"AuditTemplateItems",schema:"audit");m.DropTable(name:"AuditTemplateSections",schema:"audit");m.DropTable(name:"AuditTemplates",schema:"audit");}
}