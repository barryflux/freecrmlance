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

    [Test]
    public void Billing_identity_requires_complete_address()
    {
        var customer = new Customer(Guid.NewGuid(), "Acme");
        Assert.Throws<ArgumentException>(() => customer.SetBillingIdentity(CustomerType.Business, "", "75001", "Paris", "FR", "123456789"));
    }

    [Test]
    public void Business_billing_identity_requires_valid_siren()
    {
        var customer = new Customer(Guid.NewGuid(), "Acme");
        Assert.Throws<ArgumentException>(() => customer.SetBillingIdentity(CustomerType.Business, "1 rue Test", "75001", "Paris", "FR"));
        Assert.Throws<ArgumentException>(() => customer.SetBillingIdentity(CustomerType.Business, "1 rue Test", "75001", "Paris", "FR", "123"));
    }

    [Test]
    public void Billing_identity_is_normalized_and_stored()
    {
        var customer = new Customer(Guid.NewGuid(), "Acme");
        customer.SetBillingIdentity(CustomerType.Business, " 1 rue Test ", "75001", "Paris", "fr", "123456789", "12345678900012", "FR12345678901");

        Assert.Multiple(() =>
        {
            Assert.That(customer.Type, Is.EqualTo(CustomerType.Business));
            Assert.That(customer.Siren, Is.EqualTo("123456789"));
            Assert.That(customer.Siret, Is.EqualTo("12345678900012"));
            Assert.That(customer.AddressLine1, Is.EqualTo("1 rue Test"));
            Assert.That(customer.CountryCode, Is.EqualTo("FR"));
        });
    }

    [Test]
    public void Distinct_billing_address_must_be_complete()
    {
        var customer = new Customer(Guid.NewGuid(), "Acme");
        Assert.Throws<ArgumentException>(() => customer.SetBillingIdentity(
            CustomerType.Individual, "1 rue Test", "75001", "Paris", "FR",
            billingAddressLine1: "2 rue Facturation"));
    }
}
