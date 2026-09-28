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

    [Test]
    public void Legal_identity_requires_a_complete_address()
    {
        var workspace = new Workspace("Freelance");
        Assert.Throws<ArgumentException>(() => workspace.SetLegalIdentity("Barry Flux", "", "75001", "Paris", "FR"));
    }

    [Test]
    public void Legal_identity_validates_siren_and_siret()
    {
        var workspace = new Workspace("Freelance");
        Assert.Throws<ArgumentException>(() => workspace.SetLegalIdentity("Barry Flux", "1 rue Test", "75001", "Paris", "FR", siren: "123"));
        Assert.Throws<ArgumentException>(() => workspace.SetLegalIdentity("Barry Flux", "1 rue Test", "75001", "Paris", "FR", siret: "123"));
    }

    [Test]
    public void Legal_identity_is_normalized_and_stored()
    {
        var workspace = new Workspace("Freelance");
        workspace.SetLegalIdentity(" Barry Flux ", " 1 rue Test ", "75001", "Paris", "fr",
            "EI", "123456789", "12345678900012", "FR12345678901", contactEmail: "hello@example.test");

        Assert.Multiple(() =>
        {
            Assert.That(workspace.LegalName, Is.EqualTo("Barry Flux"));
            Assert.That(workspace.Siren, Is.EqualTo("123456789"));
            Assert.That(workspace.Siret, Is.EqualTo("12345678900012"));
            Assert.That(workspace.CountryCode, Is.EqualTo("FR"));
            Assert.That(workspace.ContactEmail, Is.EqualTo("hello@example.test"));
        });
    }
}
