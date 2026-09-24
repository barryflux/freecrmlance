using Freecrmlance.Domain.Crm;
using NUnit.Framework;

namespace Freecrmlance.UnitTests.Crm;

public sealed class ContactTests
{
    [Test] public void Name_is_required() => Assert.Throws<ArgumentException>(() => new Contact(Guid.NewGuid(), Guid.NewGuid(), " "));
    [Test] public void Workspace_is_required() => Assert.Throws<ArgumentException>(() => new Contact(Guid.Empty, Guid.NewGuid(), "Bruce Wayne"));
    [Test]
    public void Update_changes_and_normalizes_contact_details()
    {
        var contact = new Contact(Guid.NewGuid(), Guid.NewGuid(), "Alice", "old@test.local", "123", "Buyer");

        contact.Update("  Alice Smith  ", " alice@example.test ", " ", " CTO ");

        Assert.Multiple(() =>
        {
            Assert.That(contact.Name, Is.EqualTo("Alice Smith"));
            Assert.That(contact.Email, Is.EqualTo("alice@example.test"));
            Assert.That(contact.Phone, Is.Null);
            Assert.That(contact.Role, Is.EqualTo("CTO"));
        });
    }

    [Test]
    public void Update_requires_name()
    {
        var contact = new Contact(Guid.NewGuid(), Guid.NewGuid(), "Alice");
        Assert.Throws<ArgumentException>(() => contact.Update(" ", null, null, null));
    }

    [Test] public void Customer_is_required() => Assert.Throws<ArgumentException>(() => new Contact(Guid.NewGuid(), Guid.Empty, "Bruce Wayne"));
}
