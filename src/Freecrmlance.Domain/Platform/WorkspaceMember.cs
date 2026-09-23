namespace Freecrmlance.Domain.Platform;

public sealed class WorkspaceMember
{
    private WorkspaceMember() { }

    public WorkspaceMember(Guid workspaceId, string userId, WorkspaceRole role)
    {
        if (workspaceId == Guid.Empty)
            throw new ArgumentException("Workspace id is required.", nameof(workspaceId));
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User id is required.", nameof(userId));

        WorkspaceId = workspaceId;
        UserId = userId;
        Role = role;
    }

    public Guid WorkspaceId { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public WorkspaceRole Role { get; private set; }
    public Workspace Workspace { get; private set; } = null!;
}
