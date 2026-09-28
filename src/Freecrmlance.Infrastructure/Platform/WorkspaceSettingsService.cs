using Freecrmlance.Application.Platform;
using Freecrmlance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Freecrmlance.Infrastructure.Platform;

public sealed class WorkspaceSettingsService(FreecrmlanceDbContext dbContext, IWorkspaceContext workspaceContext) : IWorkspaceSettingsService
{
    public async Task<WorkspaceLegalIdentityDto?> GetAsync(CancellationToken cancellationToken = default)
    {
        var workspaceId = await workspaceContext.RequireCurrentWorkspaceIdAsync(cancellationToken);
        return await dbContext.Workspaces.AsNoTracking()
            .Where(workspace => workspace.Id == workspaceId)
            .Select(workspace => new WorkspaceLegalIdentityDto(workspace.Id, workspace.Name, workspace.LegalName, workspace.LegalForm,
                workspace.Siren, workspace.Siret, workspace.VatNumber, workspace.AddressLine1, workspace.AddressLine2,
                workspace.PostalCode, workspace.City, workspace.CountryCode, workspace.ContactEmail, workspace.ContactPhone))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> UpdateAsync(UpdateWorkspaceLegalIdentityCommand command, CancellationToken cancellationToken = default)
    {
        var workspaceId = await workspaceContext.RequireCurrentWorkspaceIdAsync(cancellationToken);
        var workspace = await dbContext.Workspaces.SingleOrDefaultAsync(workspace => workspace.Id == workspaceId, cancellationToken);
        if (workspace is null) return false;

        workspace.SetLegalIdentity(command.LegalName, command.AddressLine1, command.PostalCode, command.City, command.CountryCode,
            command.LegalForm, command.Siren, command.Siret, command.VatNumber, command.AddressLine2, command.ContactEmail, command.ContactPhone);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
