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

    [Test]
    public void Invoice_calculates_ht_vat_and_ttc()
    {
        var invoice = new Invoice(Guid.NewGuid(), Guid.NewGuid(), "INV-001");
        invoice.AddLine("Design", 2, 100m, 20m);
        invoice.AddLine("Exempt service", 1, 50m, 0m);

        Assert.Multiple(() =>
        {
            Assert.That(invoice.TotalExcludingTax, Is.EqualTo(250m));
            Assert.That(invoice.TotalVat, Is.EqualTo(40m));
            Assert.That(invoice.TotalIncludingTax, Is.EqualTo(290m));
        });
    }

    [Test]
    public void Invoice_snapshot_is_owned_by_invoice()
    {
        var invoice = new Invoice(Guid.NewGuid(), Guid.NewGuid(), "INV-001");
        invoice.SnapshotSeller("Seller SAS", "SAS", "123456789", "12345678900012", "FR123", "1 rue Seller", null, "75001", "Paris", "FR", null, null);
        invoice.SnapshotCustomer("Customer SARL", "987654321", null, "FR987", "2 rue Customer", null, "69001", "Lyon", "FR");

        Assert.Multiple(() =>
        {
            Assert.That(invoice.SellerLegalName, Is.EqualTo("Seller SAS"));
            Assert.That(invoice.SellerSiren, Is.EqualTo("123456789"));
            Assert.That(invoice.CustomerLegalName, Is.EqualTo("Customer SARL"));
            Assert.That(invoice.CustomerSiren, Is.EqualTo("987654321"));
        });
    }

    [Test]
    public void Invoice_line_rejects_invalid_vat_rate()
    {
        var invoice = new Invoice(Guid.NewGuid(), Guid.NewGuid(), "INV-001");
        Assert.Throws<ArgumentOutOfRangeException>(() => invoice.AddLine("Work", 1, 100m, 101m));
    }

    [Test]
    public void Complete_draft_can_be_issued_and_becomes_immutable()
    {
        var invoice = CompleteInvoice();
        var issuedAt = new DateTime(2026, 9, 28, 14, 0, 0, DateTimeKind.Utc);

        invoice.Issue("2026-0001", issuedAt);

        Assert.Multiple(() =>
        {
            Assert.That(invoice.Status, Is.EqualTo(InvoiceStatus.Issued));
            Assert.That(invoice.Number, Is.EqualTo("2026-0001"));
            Assert.That(invoice.IssueDate, Is.EqualTo(issuedAt));
            Assert.Throws<InvalidOperationException>(() => invoice.AddLine("More work", 1, 10m, 20m));
            Assert.Throws<InvalidOperationException>(() => invoice.SetComplianceDetails(DateTime.UtcNow, DateTime.UtcNow.AddDays(30)));
        });
    }

    [Test]
    public void Incomplete_draft_cannot_be_issued()
    {
        var invoice = new Invoice(Guid.NewGuid(), Guid.NewGuid(), "DRAFT-001");
        invoice.AddLine("Work", 1, 100m, 20m);

        Assert.Throws<InvalidOperationException>(() => invoice.Issue("2026-0001", DateTime.UtcNow));
        Assert.That(invoice.Status, Is.EqualTo(InvoiceStatus.Draft));
    }

    [Test]
    public void Zero_vat_requires_an_exemption_mention_before_issuance()
    {
        var invoice = CompleteInvoice(vatRate: 0m, vatExemptionMention: null);
        Assert.Throws<InvalidOperationException>(() => invoice.Issue("2026-0001", DateTime.UtcNow));
    }

    private static Invoice CompleteInvoice(decimal vatRate = 20m, string? vatExemptionMention = null)
    {
        var invoice = new Invoice(Guid.NewGuid(), Guid.NewGuid(), "DRAFT-001");
        invoice.SnapshotSeller("Seller SAS", "SAS", "123456789", null, "FR123", "1 rue Seller", null, "75001", "Paris", "FR", null, null);
        invoice.SnapshotCustomer("Customer SARL", "987654321", null, "FR987", "2 rue Customer", null, "69001", "Lyon", "FR");
        invoice.AddLine("Work", 1, 100m, vatRate);
        invoice.SetComplianceDetails(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(30), vatExemptionMention: vatExemptionMention, paymentTerms: "30 days");
        return invoice;
    }
}
