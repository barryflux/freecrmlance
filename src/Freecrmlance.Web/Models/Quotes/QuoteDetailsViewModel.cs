using Freecrmlance.Application.Crm;

namespace Freecrmlance.Web.Models.Quotes;

public sealed record QuoteDetailsViewModel(
    QuoteDto Quote,
    QuoteShareSummaryDto? ActiveShare,
    string? ShareUrl);
