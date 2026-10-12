using Freecrmlance.Domain.Crm;
namespace Freecrmlance.Application.Crm;
public sealed record CustomerActionDto(Guid Id, Guid CustomerId, string Title, string? Description, CustomerActionStatus Status, DateOnly? DueDate, DateTime CreatedAtUtc, DateTime? CompletedAtUtc);
public sealed record CustomerActionCountsDto(Guid CustomerId, int Todo, int InProgress);
public sealed record SaveCustomerActionCommand(string Title, string? Description, DateOnly? DueDate, CustomerActionStatus Status);
