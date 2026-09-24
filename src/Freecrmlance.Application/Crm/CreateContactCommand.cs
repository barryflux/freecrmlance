namespace Freecrmlance.Application.Crm;

public sealed record CreateContactCommand(string Name, string? Email, string? Phone, string? Role);
