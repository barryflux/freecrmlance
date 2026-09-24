namespace Freecrmlance.Application.Crm;

public sealed record UpdateContactCommand(string Name, string? Email, string? Phone, string? Role);
