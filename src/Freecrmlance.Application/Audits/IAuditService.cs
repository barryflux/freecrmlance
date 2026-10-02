using Freecrmlance.Domain.Audits;
namespace Freecrmlance.Application.Audits;
public sealed record CreateAuditCommand(Guid CustomerId,Guid TemplateId,string? Title=null,string? Description=null);
public sealed record SaveAuditResponseCommand(Guid AuditId,Guid AuditItemId,string? Value,string? Observation,string? Recommendation,string UpdatedByUserId);
public sealed record AuditItemResponseDto(string? Value,string? Observation,string? Recommendation,DateTime UpdatedAtUtc,string UpdatedByUserId);
public sealed record AuditItemDto(Guid Id,string Label,string? Description,AuditResponseType ResponseType,bool IsRequired,int Position,string? Options,AuditItemResponseDto? Response);
public sealed record AuditSectionDto(Guid Id,string Title,string? Description,int Position,IReadOnlyList<AuditItemDto> Items);
public sealed record AuditDto(Guid Id,Guid CustomerId,Guid TemplateId,string Reference,string Title,string? Description,AuditStatus Status,DateTime CreatedAtUtc,IReadOnlyList<AuditSectionDto> Sections,int AnsweredItems,int TotalItems,int RequiredAnsweredItems,int RequiredItems)
{
 public int ProgressPercent=>TotalItems==0?100:(int)Math.Round(AnsweredItems*100d/TotalItems);
 public bool RequiredItemsComplete=>RequiredAnsweredItems==RequiredItems;
}
public interface IAuditService
{
 Task<IReadOnlyList<AuditDto>> ListAsync(CancellationToken ct=default);
 Task<AuditDto?> GetAsync(Guid id,CancellationToken ct=default);
 Task<Guid?> CreateAsync(CreateAuditCommand command,CancellationToken ct=default);
 Task<string?> SaveResponseAsync(SaveAuditResponseCommand command,CancellationToken ct=default);
}
