using Freecrmlance.Domain.Crm;
using NUnit.Framework;

namespace Freecrmlance.UnitTests.Crm;

public sealed class ContactTests
{
    [Test] public void Name_is_required() => Assert.Throws<ArgumentException>(() => new Contact(Guid.NewGuid(), Guid.NewGuid(), " "));
    [Test] public void Workspace_is_required() => Assert.Throws<ArgumentException>(() => new Contact(Guid.Empty, Guid.NewGuid(), "Bruce Wayne"));
    [Test] public void Customer_is_required() => Assert.Throws<ArgumentException>(() => new Contact(Guid.NewGuid(), Guid.Empty, "Bruce Wayne"));
}
