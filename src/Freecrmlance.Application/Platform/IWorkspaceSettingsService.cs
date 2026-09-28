namespace Freecrmlance.Application.Platform;

public interface IWorkspaceSettingsService
{
    Task<WorkspaceLegalIdentityDto?> GetAsync(CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(UpdateWorkspaceLegalIdentityCommand command, CancellationToken cancellationToken = default);
}

public sealed record WorkspaceLegalIdentityDto(Guid WorkspaceId, string WorkspaceName, string? LegalName, string? LegalForm,
    string? Siren, string? Siret, string? VatNumber, string? AddressLine1, string? AddressLine2, string? PostalCode,
    string? City, string? CountryCode, string? ContactEmail, string? ContactPhone);

public sealed record UpdateWorkspaceLegalIdentityCommand(string LegalName, string? LegalForm, string? Siren, string? Siret,
    string? VatNumber, string AddressLine1, string? AddressLine2, string PostalCode, string City, string CountryCode,
    string? ContactEmail, string? ContactPhone);
