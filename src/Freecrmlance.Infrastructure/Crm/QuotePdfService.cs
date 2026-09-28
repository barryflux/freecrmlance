using Freecrmlance.Application.Crm;

namespace Freecrmlance.Infrastructure.Crm;

public sealed class QuotePdfService(
    IQuoteService quoteService,
    ICustomerService customerService,
    IPdfGenerator pdfGenerator) : IQuotePdfService
{
    public async Task<GeneratedQuotePdf?> GenerateAsync(
        Guid customerId,
        Guid quoteId,
        CancellationToken cancellationToken = default)
    {
        var quote = await quoteService.GetAsync(customerId, quoteId, cancellationToken);
        if (quote is null) return null;

        var customer = await customerService.GetAsync(customerId, cancellationToken);
        if (customer is null) return null;

        var model = new QuotePdfModel(
            quote.Number,
            quote.Status.ToString(),
            customer.Name,
            quote.CreatedAtUtc,
            quote.UpdatedAtUtc,
            quote.Total,
            quote.Lines.Select(line => new QuotePdfLineModel(
                line.Description,
                line.Quantity,
                line.UnitPrice,
                line.Total)).ToList());

        return new GeneratedQuotePdf(pdfGenerator.GenerateQuote(model), BuildFileName(quote.Number));
    }

    private static string BuildFileName(string number)
    {
        var safeNumber = new string(number
            .Where(character => char.IsLetterOrDigit(character) || character is '-' or '_')
            .ToArray());

        return $"quote-{(string.IsNullOrWhiteSpace(safeNumber) ? "document" : safeNumber)}.pdf";
    }
}
