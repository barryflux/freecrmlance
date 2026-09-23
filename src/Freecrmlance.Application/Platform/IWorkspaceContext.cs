namespace Freecrmlance.Application.Platform;

public interface IWorkspaceContext
{
    Task<Guid?> GetCurrentWorkspaceIdAsync(CancellationToken cancellationToken = default);
    Task<Guid> RequireCurrentWorkspaceIdAsync(CancellationToken cancellationToken = default);
}
