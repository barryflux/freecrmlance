using Freecrmlance.Domain.Audits;
namespace Freecrmlance.Application.Audits;
public sealed record CreateAuditCommand(Guid CustomerId,Guid TemplateId,string? Title=null,string? Description=null);
public sealed record AuditItemDto(Guid Id,string Label,string? Description,AuditResponseType ResponseType,bool IsRequired,int Position,string? Options);
public sealed record AuditSectionDto(Guid Id,string Title,string? Description,int Position,IReadOnlyList<AuditItemDto> Items);
public sealed record AuditDto(Guid Id,Guid CustomerId,Guid TemplateId,string Reference,string Title,string? Description,AuditStatus Status,DateTime CreatedAtUtc,IReadOnlyList<AuditSectionDto> Sections);
public interface IAuditService{Task<IReadOnlyList<AuditDto>> ListAsync(CancellationToken ct=default);Task<AuditDto?> GetAsync(Guid id,CancellationToken ct=default);Task<Guid?> CreateAsync(CreateAuditCommand command,CancellationToken ct=default);}
