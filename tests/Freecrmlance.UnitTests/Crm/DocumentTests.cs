using Freecrmlance.Domain.Crm;
using NUnit.Framework;

namespace Freecrmlance.UnitTests.Crm;

public sealed class DocumentTests
{
    [Test]
    public void Constructor_sets_metadata_and_server_storage_key()
    {
        var workspaceId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var document = new Document(workspaceId, customerId, " proposal.pdf ", "application/pdf", 1234, "abc123");

        Assert.Multiple(() =>
        {
            Assert.That(document.WorkspaceId, Is.EqualTo(workspaceId));
            Assert.That(document.CustomerId, Is.EqualTo(customerId));
            Assert.That(document.FileName, Is.EqualTo("proposal.pdf"));
            Assert.That(document.ContentType, Is.EqualTo("application/pdf"));
            Assert.That(document.Size, Is.EqualTo(1234));
            Assert.That(document.StorageKey, Is.EqualTo("abc123"));
        });
    }
}
