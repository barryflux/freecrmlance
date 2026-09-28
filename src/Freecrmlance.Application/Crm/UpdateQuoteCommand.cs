namespace Freecrmlance.Application.Crm;

public sealed record UpdateQuoteCommand(
    string Number,
    IReadOnlyCollection<CreateQuoteLineCommand>? Lines = null);
