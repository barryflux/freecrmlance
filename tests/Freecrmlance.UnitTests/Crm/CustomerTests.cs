using Freecrmlance.Domain.Crm;
using NUnit.Framework;

namespace Freecrmlance.UnitTests.Crm;

public sealed class CustomerTests
{
    [Test]
    public void Name_is_required()
    {
        Assert.Throws<ArgumentException>(() => new Customer(Guid.NewGuid(), " "));
    }

    [Test]
    public void Workspace_is_required()
    {
        Assert.Throws<ArgumentException>(() => new Customer(Guid.Empty, "Acme"));
    }

    [Test]
    public void Update_changes_customer_information()
    {
        var customer = new Customer(Guid.NewGuid(), "Acme");
        customer.Update("Acme France", "hello@acme.test", "0102030405");

        Assert.Multiple(() =>
        {
            Assert.That(customer.Name, Is.EqualTo("Acme France"));
            Assert.That(customer.Email, Is.EqualTo("hello@acme.test"));
            Assert.That(customer.Phone, Is.EqualTo("0102030405"));
        });
    }

    [Test]
    public void Update_requires_a_name()
    {
        var customer = new Customer(Guid.NewGuid(), "Acme");
        Assert.Throws<ArgumentException>(() => customer.Update(" ", null, null));
    }
}
