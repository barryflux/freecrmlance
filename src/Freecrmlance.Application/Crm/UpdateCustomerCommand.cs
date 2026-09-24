namespace Freecrmlance.Application.Crm;

public sealed record UpdateCustomerCommand(string Name, string? Email, string? Phone);
