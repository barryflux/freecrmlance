using Freecrmlance.Domain.Audits;

namespace Freecrmlance.Application.Audits;

public sealed record AuditTemplateItemInput(string Label, AuditResponseType ResponseType, bool IsRequired, string? Description, string? Options);
public sealed record AuditTemplateSectionInput(string Title, string? Description, IReadOnlyList<AuditTemplateItemInput> Items);
public sealed record SaveAuditTemplateCommand(string Name, string? Description, IReadOnlyList<AuditTemplateSectionInput> Sections);
public sealed record AuditTemplateItemDto(Guid Id, string Label, string? Description, AuditResponseType ResponseType, bool IsRequired, int Position, string? Options);
public sealed record AuditTemplateSectionDto(Guid Id, string Title, string? Description, int Position, IReadOnlyList<AuditTemplateItemDto> Items);
public sealed record AuditTemplateDto(Guid Id, string Name, string? Description, bool IsArchived, DateTime UpdatedAtUtc, IReadOnlyList<AuditTemplateSectionDto> Sections);

public interface IAuditTemplateService
{
    Task<IReadOnlyList<AuditTemplateDto>> ListAsync(bool includeArchived = false, CancellationToken cancellationToken = default);
    Task<AuditTemplateDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(SaveAuditTemplateCommand command, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Guid id, SaveAuditTemplateCommand command, CancellationToken cancellationToken = default);
    Task<Guid?> DuplicateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> SetArchivedAsync(Guid id, bool archived, CancellationToken cancellationToken = default);
}
