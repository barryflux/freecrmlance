namespace Freecrmlance.Application.Crm;

public interface IPdfGenerator
{
    byte[] GenerateQuote(QuotePdfModel model);
}

public sealed record QuotePdfModel(
    string Number,
    string Status,
    string CustomerName,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    decimal Total,
    IReadOnlyList<QuotePdfLineModel> Lines);

public sealed record QuotePdfLineModel(
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal Total);
