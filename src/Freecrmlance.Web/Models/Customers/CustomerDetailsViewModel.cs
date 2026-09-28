using Freecrmlance.Application.Crm;

namespace Freecrmlance.Web.Models.Customers;

public sealed record CustomerDetailsViewModel(
    CustomerDto Customer,
    IReadOnlyList<ContactDto> Contacts,
    IReadOnlyList<DocumentDto> Documents,
    IReadOnlyList<DocumentShareSummaryDto> DocumentShares,
    IReadOnlyList<QuoteDto> Quotes);
