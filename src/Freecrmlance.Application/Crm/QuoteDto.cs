using Freecrmlance.Domain.Crm;

namespace Freecrmlance.Application.Crm;

public sealed record QuoteLineDto(Guid Id, string Description, decimal Quantity, decimal UnitPrice, decimal Total);

public sealed record QuoteDto(
    Guid Id,
    Guid CustomerId,
    string Number,
    QuoteStatus Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    decimal Total,
    IReadOnlyList<QuoteLineDto> Lines);
