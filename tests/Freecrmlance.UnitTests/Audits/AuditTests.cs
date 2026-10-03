using Freecrmlance.Domain.Audits;
using NUnit.Framework;
namespace Freecrmlance.UnitTests.Audits;
public sealed class AuditTests
{
 [Test]public void Start_moves_draft_to_in_progress_once()
 {
  var audit=new Audit(Guid.NewGuid(),Guid.NewGuid(),Guid.NewGuid(),"AUD-2026-0001","Audit",null);
  audit.Start();var started=audit.StartedAtUtc;audit.Start();
  Assert.Multiple(()=>{Assert.That(audit.Status,Is.EqualTo(AuditStatus.InProgress));Assert.That(started,Is.Not.Null);Assert.That(audit.StartedAtUtc,Is.EqualTo(started));});
 }

    [Test]
    public void Finalize_and_reopen_follow_explicit_lifecycle()
    {
        var audit = new Audit(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "AUD-1", "Audit", null);
        var at = DateTime.UtcNow;
        audit.FinalizeAudit(at);
        Assert.That(audit.Status, Is.EqualTo(AuditStatus.Finalized));
        Assert.That(audit.CompletedAtUtc, Is.EqualTo(at));
        audit.Reopen();
        Assert.That(audit.Status, Is.EqualTo(AuditStatus.InProgress));
        Assert.That(audit.CompletedAtUtc, Is.Null);
    }
}
