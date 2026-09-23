using Freecrmlance.Domain.Crm;
using NUnit.Framework;

namespace Freecrmlance.UnitTests.Crm;

public sealed class CustomerTests
{
    [Test]
    public void Customer_requires_a_name()
    {
        Assert.Throws<ArgumentException>(() => new Customer(Guid.NewGuid(), " "));
    }

    [Test]
    public void Customer_requires_a_workspace()
    {
        Assert.Throws<ArgumentException>(() => new Customer(Guid.Empty, "Acme"));
    }
}
