using Freecrmlance.Domain.Crm;

namespace Freecrmlance.Application.Crm;

public sealed record CreateCustomerCommand(
    string Name, string? Email, string? Phone, IReadOnlyList<CreateContactCommand>? Contacts = null,
    CustomerType? Type = null, string? Siren = null, string? Siret = null, string? VatNumber = null,
    string? AddressLine1 = null, string? AddressLine2 = null, string? PostalCode = null, string? City = null, string? CountryCode = null,
    string? BillingAddressLine1 = null, string? BillingAddressLine2 = null, string? BillingPostalCode = null, string? BillingCity = null, string? BillingCountryCode = null);
