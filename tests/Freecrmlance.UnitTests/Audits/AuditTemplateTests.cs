using Freecrmlance.Domain.Audits;
using NUnit.Framework;
namespace Freecrmlance.UnitTests.Audits;
public sealed class AuditTemplateTests
{
 [Test] public void Workspace_is_required()=>Assert.Throws<ArgumentException>(()=>new AuditTemplate(Guid.Empty,"Audit"));
 [Test] public void Name_is_required()=>Assert.Throws<ArgumentException>(()=>new AuditTemplate(Guid.NewGuid()," "));
 [Test] public void Sections_and_items_keep_their_order()
 {
  var t=new AuditTemplate(Guid.NewGuid(),"Sécurité");var s1=t.AddSection("Accès");var s2=t.AddSection("Sauvegardes");
  var i1=s1.AddItem("MFA ?",AuditResponseType.YesNo,true);var i2=s1.AddItem("Commentaire",AuditResponseType.Text);
  Assert.Multiple(()=>{Assert.That(s1.Position,Is.EqualTo(0));Assert.That(s2.Position,Is.EqualTo(1));Assert.That(i1.Position,Is.EqualTo(0));Assert.That(i2.Position,Is.EqualTo(1));});
 }
 [Test] public void Choice_response_requires_options()
 {
  var t=new AuditTemplate(Guid.NewGuid(),"Audit");var s=t.AddSection("Section");
  Assert.Throws<ArgumentException>(()=>s.AddItem("Choix",AuditResponseType.SingleChoice,options:null));
 }
 [Test] public void Non_choice_response_discards_options()
 {
  var t=new AuditTemplate(Guid.NewGuid(),"Audit");var i=t.AddSection("Section").AddItem("Oui ?",AuditResponseType.YesNo,options:"A\nB");
  Assert.That(i.Options,Is.Null);
 }
 [Test] public void Template_can_be_archived_and_restored()
 {
  var t=new AuditTemplate(Guid.NewGuid(),"Audit");t.Archive();Assert.That(t.IsArchived,Is.True);t.Restore();Assert.That(t.IsArchived,Is.False);
 }
}
