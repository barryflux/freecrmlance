using Freecrmlance.Domain.Billing;

namespace Freecrmlance.Application.Billing;

public interface IInvoiceService
{
    Task<IReadOnlyList<InvoiceDto>?> ListAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<InvoiceDto?> GetAsync(Guid customerId, Guid invoiceId, CancellationToken cancellationToken = default);
    Task<Guid?> CreateAsync(CreateInvoiceCommand command, CancellationToken cancellationToken = default);
    Task<Guid?> CreateFromAcceptedQuoteAsync(CreateInvoiceFromQuoteCommand command, CancellationToken cancellationToken = default);
}

public sealed record CreateInvoiceCommand(Guid CustomerId, string Number, IReadOnlyList<CreateInvoiceLineCommand>? Lines = null);
public sealed record CreateInvoiceFromQuoteCommand(Guid CustomerId, Guid QuoteId, string Number);
public sealed record CreateInvoiceLineCommand(string Description, decimal Quantity, decimal UnitPrice);
public sealed record InvoiceDto(Guid Id, Guid CustomerId, Guid? SourceQuoteId, string Number, InvoiceStatus Status, DateTime CreatedAtUtc, DateTime UpdatedAtUtc, decimal Total, IReadOnlyList<InvoiceLineDto> Lines);
public sealed record InvoiceLineDto(Guid Id, string Description, decimal Quantity, decimal UnitPrice, decimal Total);
