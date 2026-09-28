namespace Freecrmlance.Application.Crm;

public sealed record CreateQuoteLineCommand(string Description, decimal Quantity, decimal UnitPrice);

public sealed record CreateQuoteCommand(
    Guid CustomerId,
    string Number,
    IReadOnlyCollection<CreateQuoteLineCommand>? Lines = null);
