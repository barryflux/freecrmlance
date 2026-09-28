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
    [Test]
    public void Draft_cannot_be_accepted_or_rejected_directly()
    {
        var quote = new Quote(Guid.NewGuid(), Guid.NewGuid(), "Q-001");

        Assert.Multiple(() =>
        {
            Assert.Throws<InvalidOperationException>(() => quote.Accept());
            Assert.Throws<InvalidOperationException>(() => quote.Reject());
        });
    }

    [Test]
    public void Sent_quote_can_be_rejected()
    {
        var quote = new Quote(Guid.NewGuid(), Guid.NewGuid(), "Q-001");
        quote.MarkSent();

        quote.Reject();

        Assert.That(quote.Status, Is.EqualTo(QuoteStatus.Rejected));
    }

    [TestCase(QuoteStatus.Accepted)]
    [TestCase(QuoteStatus.Rejected)]
    public void Terminal_quote_cannot_transition_again(QuoteStatus terminalStatus)
    {
        var quote = new Quote(Guid.NewGuid(), Guid.NewGuid(), "Q-001");
        quote.MarkSent();
        if (terminalStatus == QuoteStatus.Accepted) quote.Accept();
        else quote.Reject();

        Assert.Multiple(() =>
        {
            Assert.Throws<InvalidOperationException>(() => quote.MarkSent());
            Assert.Throws<InvalidOperationException>(() => quote.Accept());
            Assert.Throws<InvalidOperationException>(() => quote.Reject());
        });
    }

    [Test]
    public void Draft_can_replace_number_and_lines()
    {
        var quote = new Quote(Guid.NewGuid(), Guid.NewGuid(), "Q-001");
        quote.AddLine("Old", 1, 10);

        quote.UpdateDraft("Q-002", [("Design", 2m, 150m), ("Hosting", 1m, 50m)]);

        Assert.Multiple(() =>
        {
            Assert.That(quote.Number, Is.EqualTo("Q-002"));
            Assert.That(quote.Lines.Select(line => line.Description), Is.EqualTo(new[] { "Design", "Hosting" }));
            Assert.That(quote.Total, Is.EqualTo(350m));
        });
    }

    [Test]
    public void Non_draft_quote_cannot_be_edited()
    {
        var quote = new Quote(Guid.NewGuid(), Guid.NewGuid(), "Q-001");
        quote.MarkSent();

        Assert.Throws<InvalidOperationException>(() => quote.UpdateDraft("Q-002", []));
    }

}
