namespace Freecrmlance.Application.Platform;

public sealed class WorkspaceNotAvailableException()
    : InvalidOperationException("No workspace is available for the current user.");
