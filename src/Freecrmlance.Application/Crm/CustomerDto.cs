namespace Freecrmlance.Application.Crm;

public sealed record CustomerDto(Guid Id, string Name, string? Email, string? Phone);
