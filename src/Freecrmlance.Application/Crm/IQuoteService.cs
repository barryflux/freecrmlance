namespace Freecrmlance.Application.Crm;

public interface IQuoteService
{
    Task<IReadOnlyList<QuoteDto>?> ListAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<QuoteDto?> GetAsync(Guid customerId, Guid quoteId, CancellationToken cancellationToken = default);
    Task<Guid?> CreateAsync(CreateQuoteCommand command, CancellationToken cancellationToken = default);
}
