namespace Freecrmlance.Application.Crm;

public interface IFileStorage
{
    Task<string> SaveAsync(Stream content, CancellationToken cancellationToken = default);
    Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default);
    Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
}
