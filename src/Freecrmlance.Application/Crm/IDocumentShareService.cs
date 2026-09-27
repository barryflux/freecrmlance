namespace Freecrmlance.Application.Crm;

public interface IDocumentShareService
{
    Task<DocumentShareDto?> CreateAsync(Guid customerId, Guid documentId, CancellationToken cancellationToken = default);
    Task<DocumentDownloadDto?> DownloadAsync(string token, CancellationToken cancellationToken = default);
    Task<bool> RevokeAsync(Guid customerId, Guid documentId, Guid shareId, CancellationToken cancellationToken = default);
}
