using Freecrmlance.Domain.Billing;

namespace Freecrmlance.Application.Billing;

public interface IInvoiceService
{
    Task<IReadOnlyList<InvoiceDto>?> ListAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<InvoiceDto?> GetAsync(Guid customerId, Guid invoiceId, CancellationToken cancellationToken = default);
    Task<Guid?> CreateAsync(CreateInvoiceCommand command, CancellationToken cancellationToken = default);
    Task<Guid?> CreateFromAcceptedQuoteAsync(CreateInvoiceFromQuoteCommand command, CancellationToken cancellationToken = default);
    Task<UpdateInvoiceResult> UpdateAsync(UpdateInvoiceCommand command, CancellationToken cancellationToken = default);
    Task<IssueInvoiceResult> IssueAsync(Guid customerId, Guid invoiceId, CancellationToken cancellationToken = default);
}

public enum UpdateInvoiceResult
{
    Success,
    NotFound,
    InvalidStatus
}

public enum IssueInvoiceResult
{
    Success,
    NotFound,
    InvalidStatus,
    Incomplete
}

public sealed record CreateInvoiceCommand(Guid CustomerId, string DraftReference, IReadOnlyList<CreateInvoiceLineCommand>? Lines = null,
    DateTime? ServiceDate = null, DateTime? DueDate = null, string? PurchaseOrderReference = null, string? VatExemptionMention = null,
    string? PaymentTerms = null, string? EarlyPaymentDiscountTerms = null, string? LatePaymentPenaltyTerms = null, decimal? RecoveryCostIndemnity = null);
public sealed record CreateInvoiceFromQuoteCommand(Guid CustomerId, Guid QuoteId, string DraftReference);
public sealed record UpdateInvoiceCommand(Guid CustomerId, Guid InvoiceId, string DraftReference, IReadOnlyList<CreateInvoiceLineCommand> Lines,
    DateTime? ServiceDate = null, DateTime? DueDate = null, string? PurchaseOrderReference = null, string? VatExemptionMention = null,
    string? PaymentTerms = null, string? EarlyPaymentDiscountTerms = null, string? LatePaymentPenaltyTerms = null, decimal? RecoveryCostIndemnity = null);
public sealed record CreateInvoiceLineCommand(string Description, decimal Quantity, decimal UnitPrice, decimal VatRate = 0m);
public sealed record InvoiceDto(Guid Id, Guid CustomerId, Guid? SourceQuoteId, string DraftReference, string? Number, InvoiceStatus Status, DateTime CreatedAtUtc, DateTime UpdatedAtUtc,
    decimal Total, IReadOnlyList<InvoiceLineDto> Lines, decimal TotalExcludingTax = 0m, decimal TotalVat = 0m, decimal TotalIncludingTax = 0m,
    DateTime? IssueDate = null, DateTime? ServiceDate = null, DateTime? DueDate = null, string? PurchaseOrderReference = null,
    string? SellerLegalName = null, string? SellerSiren = null, string? SellerSiret = null, string? SellerVatNumber = null,
    string? CustomerLegalName = null, string? CustomerSiren = null, string? CustomerSiret = null, string? CustomerVatNumber = null,
    string? VatExemptionMention = null, string? PaymentTerms = null, string? EarlyPaymentDiscountTerms = null,
    string? LatePaymentPenaltyTerms = null, decimal? RecoveryCostIndemnity = null,
    string? SellerAddressLine1 = null, string? SellerAddressLine2 = null, string? SellerPostalCode = null, string? SellerCity = null, string? SellerCountryCode = null,
    string? CustomerAddressLine1 = null, string? CustomerAddressLine2 = null, string? CustomerPostalCode = null, string? CustomerCity = null, string? CustomerCountryCode = null);
public sealed record InvoiceLineDto(Guid Id, string Description, decimal Quantity, decimal UnitPrice, decimal Total,
    decimal VatRate = 0m, decimal TotalExcludingTax = 0m, decimal VatAmount = 0m, decimal TotalIncludingTax = 0m);
