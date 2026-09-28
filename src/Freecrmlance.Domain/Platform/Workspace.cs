namespace Freecrmlance.Domain.Platform;

public sealed class Workspace
{
    private Workspace() { }

    public Workspace(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Workspace name is required.", nameof(name));
        Id = Guid.NewGuid();
        Name = name.Trim();
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? LegalName { get; private set; }
    public string? LegalForm { get; private set; }
    public string? Siren { get; private set; }
    public string? Siret { get; private set; }
    public string? VatNumber { get; private set; }
    public string? AddressLine1 { get; private set; }
    public string? AddressLine2 { get; private set; }
    public string? PostalCode { get; private set; }
    public string? City { get; private set; }
    public string? CountryCode { get; private set; }
    public string? ContactEmail { get; private set; }
    public string? ContactPhone { get; private set; }

    public void SetLegalIdentity(string legalName, string addressLine1, string postalCode, string city, string countryCode,
        string? legalForm = null, string? siren = null, string? siret = null, string? vatNumber = null,
        string? addressLine2 = null, string? contactEmail = null, string? contactPhone = null)
    {
        if (string.IsNullOrWhiteSpace(legalName)) throw new ArgumentException("Legal name is required.", nameof(legalName));
        if (string.IsNullOrWhiteSpace(addressLine1)) throw new ArgumentException("Address line 1 is required.", nameof(addressLine1));
        if (string.IsNullOrWhiteSpace(postalCode)) throw new ArgumentException("Postal code is required.", nameof(postalCode));
        if (string.IsNullOrWhiteSpace(city)) throw new ArgumentException("City is required.", nameof(city));
        if (string.IsNullOrWhiteSpace(countryCode) || countryCode.Trim().Length != 2) throw new ArgumentException("Country code must contain 2 letters.", nameof(countryCode));

        siren = Normalize(siren); siret = Normalize(siret);
        if (siren is not null && (siren.Length != 9 || !siren.All(char.IsDigit))) throw new ArgumentException("SIREN must contain 9 digits.", nameof(siren));
        if (siret is not null && (siret.Length != 14 || !siret.All(char.IsDigit))) throw new ArgumentException("SIRET must contain 14 digits.", nameof(siret));

        LegalName = legalName.Trim(); LegalForm = Normalize(legalForm); Siren = siren; Siret = siret; VatNumber = Normalize(vatNumber);
        AddressLine1 = addressLine1.Trim(); AddressLine2 = Normalize(addressLine2); PostalCode = postalCode.Trim(); City = city.Trim();
        CountryCode = countryCode.Trim().ToUpperInvariant(); ContactEmail = Normalize(contactEmail); ContactPhone = Normalize(contactPhone);
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
