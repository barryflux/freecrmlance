namespace Freecrmlance.Application.Crm;

public interface IQuoteShareService
{
    Task<QuoteShareSummaryDto?> GetActiveAsync(Guid customerId, Guid quoteId, CancellationToken cancellationToken = default);
    Task<QuoteShareDto?> CreateAsync(Guid customerId, Guid quoteId, CancellationToken cancellationToken = default);
    Task<SharedQuoteDto?> GetPublicAsync(string token, CancellationToken cancellationToken = default);
    Task<bool> RevokeAsync(Guid customerId, Guid quoteId, Guid shareId, CancellationToken cancellationToken = default);
}

public sealed record QuoteShareDto(Guid Id, string Token, DateTime CreatedAtUtc);
public sealed record QuoteShareSummaryDto(Guid Id, DateTime CreatedAtUtc);
public sealed record SharedQuoteDto(
    string Number,
    string Status,
    string CustomerName,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    decimal Total,
    IReadOnlyList<SharedQuoteLineDto> Lines);
public sealed record SharedQuoteLineDto(string Description, decimal Quantity, decimal UnitPrice, decimal Total);
