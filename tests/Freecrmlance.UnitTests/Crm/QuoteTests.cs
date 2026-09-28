using Freecrmlance.Domain.Crm;
using NUnit.Framework;

namespace Freecrmlance.UnitTests.Crm;

public sealed class QuoteTests
{
    [Test]
    public void Quote_starts_as_draft_and_calculates_total()
    {
        var quote = new Quote(Guid.NewGuid(), Guid.NewGuid(), "Q-001");
        quote.AddLine("Design", 2, 150m);
        quote.AddLine("Hosting", 1, 50m);

        Assert.Multiple(() =>
        {
            Assert.That(quote.Status, Is.EqualTo(QuoteStatus.Draft));
            Assert.That(quote.Total, Is.EqualTo(350m));
            Assert.That(quote.Lines, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public void Quote_line_requires_a_description()
    {
        var quote = new Quote(Guid.NewGuid(), Guid.NewGuid(), "Q-001");
        Assert.Throws<ArgumentException>(() => quote.AddLine(" ", 1, 10));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Quote_line_requires_a_positive_quantity(decimal quantity)
    {
        var quote = new Quote(Guid.NewGuid(), Guid.NewGuid(), "Q-001");
        Assert.Throws<ArgumentOutOfRangeException>(() => quote.AddLine("Work", quantity, 10));
    }

    [Test]
    public void Quote_line_rejects_a_negative_unit_price()
    {
        var quote = new Quote(Guid.NewGuid(), Guid.NewGuid(), "Q-001");
        Assert.Throws<ArgumentOutOfRangeException>(() => quote.AddLine("Work", 1, -1));
    }

    [Test]
    public void Quote_status_can_progress_through_supported_states()
    {
        var quote = new Quote(Guid.NewGuid(), Guid.NewGuid(), "Q-001");
        quote.MarkSent();
        Assert.That(quote.Status, Is.EqualTo(QuoteStatus.Sent));
        quote.Accept();
        Assert.That(quote.Status, Is.EqualTo(QuoteStatus.Accepted));
    }
}
