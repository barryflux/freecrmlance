using Freecrmlance.Application.Crm;
using Microsoft.Extensions.Configuration;

namespace Freecrmlance.Infrastructure.Crm;

public sealed class LocalFileStorage : IFileStorage
{
    private readonly string rootPath;

    public LocalFileStorage(IConfiguration configuration)
    {
        var configuredPath = configuration["FileStorage:RootPath"];
        rootPath = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(AppContext.BaseDirectory, "App_Data", "files")
            : Path.GetFullPath(configuredPath);
    }

    public async Task<string> SaveAsync(Stream content, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(rootPath);
        var storageKey = Guid.NewGuid().ToString("N");
        var path = GetPath(storageKey);

        await using var destination = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
        await content.CopyToAsync(destination, cancellationToken);
        return storageKey;
    }

    public Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        Stream stream = new FileStream(GetPath(storageKey), FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var path = GetPath(storageKey);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    private string GetPath(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey) || storageKey.Any(character => !char.IsLetterOrDigit(character)))
            throw new ArgumentException("Invalid storage key.", nameof(storageKey));

        return Path.Combine(rootPath, storageKey);
    }
}
