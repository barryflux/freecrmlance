using Freecrmlance.Application.Platform;
using Freecrmlance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Freecrmlance.Infrastructure.Platform;

public sealed class WorkspaceContext(
    ICurrentUser currentUser,
    FreecrmlanceDbContext dbContext) : IWorkspaceContext
{
    public async Task<Guid?> GetCurrentWorkspaceIdAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(currentUser.UserId))
            return null;

        return await dbContext.WorkspaceMembers
            .AsNoTracking()
            .Where(member => member.UserId == currentUser.UserId)
            .Select(member => (Guid?)member.WorkspaceId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Guid> RequireCurrentWorkspaceIdAsync(CancellationToken cancellationToken = default)
        => await GetCurrentWorkspaceIdAsync(cancellationToken)
            ?? throw new WorkspaceNotAvailableException();
}
