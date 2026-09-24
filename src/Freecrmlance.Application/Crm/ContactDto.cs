namespace Freecrmlance.Application.Crm;

public sealed record ContactDto(Guid Id, Guid CustomerId, string Name, string? Email, string? Phone, string? Role);
