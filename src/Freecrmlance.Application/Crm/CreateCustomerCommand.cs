namespace Freecrmlance.Application.Crm;

public sealed record CreateCustomerCommand(string Name, string? Email, string? Phone);
