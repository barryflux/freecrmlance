namespace Freecrmlance.Application.Crm;

public interface IDocumentService
{
    Task<IReadOnlyList<DocumentDto>?> ListAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<Guid?> UploadAsync(Guid customerId, string fileName, string contentType, long size, Stream content, CancellationToken cancellationToken = default);
    Task<DocumentDownloadDto?> DownloadAsync(Guid customerId, Guid documentId, CancellationToken cancellationToken = default);
}
