using Freecrmlance.Domain.Platform;
using NUnit.Framework;

namespace Freecrmlance.UnitTests.Platform;

public sealed class WorkspaceTests
{
    [Test]
    public void Workspace_requires_a_name()
    {
        Assert.Throws<ArgumentException>(() => new Workspace(" "));
    }

    [Test]
    public void Owner_membership_keeps_identity_as_external_id()
    {
        var workspace = new Workspace("Freelance");
        var member = new WorkspaceMember(workspace.Id, "identity-user-id", WorkspaceRole.Owner);

        Assert.Multiple(() =>
        {
            Assert.That(member.WorkspaceId, Is.EqualTo(workspace.Id));
            Assert.That(member.UserId, Is.EqualTo("identity-user-id"));
            Assert.That(member.Role, Is.EqualTo(WorkspaceRole.Owner));
        });
    }
}
