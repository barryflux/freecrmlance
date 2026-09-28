namespace Freecrmlance.Domain.Crm;

public sealed class Customer
{
    private Customer() { }

    public Customer(Guid workspaceId, string name, string? email = null, string? phone = null)
    {
        if (workspaceId == Guid.Empty) throw new ArgumentException("Workspace id is required.", nameof(workspaceId));
        Id = Guid.NewGuid();
        WorkspaceId = workspaceId;
        Update(name, email, phone);
    }

    public Guid Id { get; private set; }
    public Guid WorkspaceId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public CustomerType? Type { get; private set; }
    public string? Siren { get; private set; }
    public string? Siret { get; private set; }
    public string? VatNumber { get; private set; }
    public string? AddressLine1 { get; private set; }
    public string? AddressLine2 { get; private set; }
    public string? PostalCode { get; private set; }
    public string? City { get; private set; }
    public string? CountryCode { get; private set; }
    public string? BillingAddressLine1 { get; private set; }
    public string? BillingAddressLine2 { get; private set; }
    public string? BillingPostalCode { get; private set; }
    public string? BillingCity { get; private set; }
    public string? BillingCountryCode { get; private set; }

    public void Update(string name, string? email, string? phone)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Customer name is required.", nameof(name));
        Name = name.Trim(); Email = Normalize(email); Phone = Normalize(phone);
    }

    public void SetBillingIdentity(CustomerType type, string addressLine1, string postalCode, string city, string countryCode,
        string? siren = null, string? siret = null, string? vatNumber = null, string? addressLine2 = null,
        string? billingAddressLine1 = null, string? billingAddressLine2 = null, string? billingPostalCode = null,
        string? billingCity = null, string? billingCountryCode = null)
    {
        if (string.IsNullOrWhiteSpace(addressLine1)) throw new ArgumentException("Address line 1 is required.", nameof(addressLine1));
        if (string.IsNullOrWhiteSpace(postalCode)) throw new ArgumentException("Postal code is required.", nameof(postalCode));
        if (string.IsNullOrWhiteSpace(city)) throw new ArgumentException("City is required.", nameof(city));
        if (string.IsNullOrWhiteSpace(countryCode) || countryCode.Trim().Length != 2) throw new ArgumentException("Country code must contain 2 letters.", nameof(countryCode));

        siren = Normalize(siren); siret = Normalize(siret);
        if (siren is not null && (siren.Length != 9 || !siren.All(char.IsDigit))) throw new ArgumentException("SIREN must contain 9 digits.", nameof(siren));
        if (siret is not null && (siret.Length != 14 || !siret.All(char.IsDigit))) throw new ArgumentException("SIRET must contain 14 digits.", nameof(siret));
        if (type == CustomerType.Business && siren is null) throw new ArgumentException("SIREN is required for business customers.", nameof(siren));

        var hasBilling = new[] { billingAddressLine1, billingPostalCode, billingCity, billingCountryCode }.Any(v => !string.IsNullOrWhiteSpace(v));
        if (hasBilling && (string.IsNullOrWhiteSpace(billingAddressLine1) || string.IsNullOrWhiteSpace(billingPostalCode) || string.IsNullOrWhiteSpace(billingCity) || string.IsNullOrWhiteSpace(billingCountryCode) || billingCountryCode.Trim().Length != 2))
            throw new ArgumentException("A distinct billing address must be complete.");

        Type = type; Siren = siren; Siret = siret; VatNumber = Normalize(vatNumber);
        AddressLine1 = addressLine1.Trim(); AddressLine2 = Normalize(addressLine2); PostalCode = postalCode.Trim(); City = city.Trim(); CountryCode = countryCode.Trim().ToUpperInvariant();
        BillingAddressLine1 = Normalize(billingAddressLine1); BillingAddressLine2 = Normalize(billingAddressLine2); BillingPostalCode = Normalize(billingPostalCode); BillingCity = Normalize(billingCity); BillingCountryCode = Normalize(billingCountryCode)?.ToUpperInvariant();
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
