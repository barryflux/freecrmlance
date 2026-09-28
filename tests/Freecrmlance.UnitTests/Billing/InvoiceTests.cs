using Freecrmlance.Domain.Billing;
using NUnit.Framework;

namespace Freecrmlance.UnitTests.Billing;

public sealed class InvoiceTests
{
    [Test]
    public void Invoice_starts_as_draft_and_calculates_total()
    {
        var invoice = new Invoice(Guid.NewGuid(), Guid.NewGuid(), "INV-001");
        invoice.AddLine("Design", 2, 150m);
        invoice.AddLine("Hosting", 1, 50m);

        Assert.Multiple(() =>
        {
            Assert.That(invoice.Status, Is.EqualTo(InvoiceStatus.Draft));
            Assert.That(invoice.Total, Is.EqualTo(350m));
            Assert.That(invoice.Lines, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public void Invoice_can_reference_a_source_quote()
    {
        var quoteId = Guid.NewGuid();
        var invoice = new Invoice(Guid.NewGuid(), Guid.NewGuid(), "INV-001", quoteId);
        Assert.That(invoice.SourceQuoteId, Is.EqualTo(quoteId));
    }

    [Test]
    public void Invoice_line_enforces_commercial_invariants()
    {
        var invoice = new Invoice(Guid.NewGuid(), Guid.NewGuid(), "INV-001");
        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentException>(() => invoice.AddLine(" ", 1, 10));
            Assert.Throws<ArgumentOutOfRangeException>(() => invoice.AddLine("Work", 0, 10));
            Assert.Throws<ArgumentOutOfRangeException>(() => invoice.AddLine("Work", 1, -1));
        });
    }
}
