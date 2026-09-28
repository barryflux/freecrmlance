namespace Freecrmlance.Application.Crm;

public interface IQuotePdfService
{
    Task<GeneratedQuotePdf?> GenerateAsync(Guid customerId, Guid quoteId, CancellationToken cancellationToken = default);
}

public sealed record GeneratedQuotePdf(byte[] Content, string FileName);
