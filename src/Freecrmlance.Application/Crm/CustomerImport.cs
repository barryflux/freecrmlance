using System.Globalization;
using System.Text;
using Freecrmlance.Domain.Crm;

namespace Freecrmlance.Application.Crm;

public enum CustomerImportField
{
    Ignore, Name, Type, Email, Phone, Siren, Siret, VatNumber,
    AddressLine1, AddressLine2, PostalCode, City, CountryCode
}

public sealed record CustomerImportColumn(int Index, string Header, IReadOnlyList<string> Samples, CustomerImportField SuggestedField);
public sealed record CustomerImportFile(IReadOnlyList<CustomerImportColumn> Columns, IReadOnlyList<IReadOnlyList<string>> Rows);
public sealed record CustomerImportMapping(int ColumnIndex, CustomerImportField Field);
public sealed record CustomerImportRowPreview(int RowNumber, string Name, string? Email, string? Siret, bool IsDuplicate, IReadOnlyList<string> Errors)
{
    public bool IsReady => !IsDuplicate && Errors.Count == 0;
}
public sealed record CustomerImportPreview(IReadOnlyList<CustomerImportRowPreview> Rows)
{
    public int ReadyCount => Rows.Count(x => x.IsReady);
    public int DuplicateCount => Rows.Count(x => x.IsDuplicate);
    public int InvalidCount => Rows.Count(x => x.Errors.Count > 0);
}
public sealed record CustomerImportResult(int ImportedCount, int DuplicateCount, int RejectedCount);

public interface ICustomerImportFileReader
{
    Task<CustomerImportFile> ReadAsync(string fileName, Stream content, CancellationToken cancellationToken = default);
}

public static class CustomerImportMappingSuggester
{
    private static readonly IReadOnlyDictionary<CustomerImportField, string[]> Aliases =
        new Dictionary<CustomerImportField, string[]>
        {
            [CustomerImportField.Name] = ["nom", "nom client", "client", "societe", "entreprise", "company", "raison sociale"],
            [CustomerImportField.Type] = ["type", "type client", "customer type"],
            [CustomerImportField.Email] = ["email", "e mail", "mail", "mail client", "email client", "courriel"],
            [CustomerImportField.Phone] = ["telephone", "tel", "mobile", "portable", "phone"],
            [CustomerImportField.Siren] = ["siren", "numero siren", "n siren"],
            [CustomerImportField.Siret] = ["siret", "numero siret", "n siret"],
            [CustomerImportField.VatNumber] = ["tva", "numero tva", "n tva", "vat", "vat number"],
            [CustomerImportField.AddressLine1] = ["adresse", "adresse 1", "address", "address line 1"],
            [CustomerImportField.AddressLine2] = ["complement", "complement adresse", "adresse 2", "address line 2"],
            [CustomerImportField.PostalCode] = ["code postal", "cp", "zip", "postal code"],
            [CustomerImportField.City] = ["ville", "localite", "city"],
            [CustomerImportField.CountryCode] = ["pays", "code pays", "country", "country code"]
        };

    public static CustomerImportField Suggest(string header)
    {
        var normalized = Normalize(header);
        foreach (var (field, aliases) in Aliases)
            if (aliases.Any(alias => Normalize(alias) == normalized))
                return field;
        return CustomerImportField.Ignore;
    }

    private static string Normalize(string value)
    {
        var decomposed = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();
        foreach (var c in decomposed)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                builder.Append(char.IsLetterOrDigit(c) ? c : ' ');
        return string.Join(' ', builder.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}

public sealed class CustomerImportService(ICustomerService customerService)
{
    public async Task<CustomerImportPreview> PreviewAsync(CustomerImportFile file, IReadOnlyList<CustomerImportMapping> mappings, CancellationToken cancellationToken = default)
    {
        ValidateMappings(mappings);
        var existing = await customerService.ListAsync(cancellationToken);
        var previews = new List<CustomerImportRowPreview>();
        var seenSirets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var seenEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var index = 0; index < file.Rows.Count; index++)
        {
            var values = Map(file.Rows[index], mappings);
            var errors = Validate(values);
            var siret = Value(values, CustomerImportField.Siret);
            var email = Value(values, CustomerImportField.Email);
            var duplicate = !string.IsNullOrWhiteSpace(siret) && (existing.Any(x => string.Equals(x.Siret, siret, StringComparison.OrdinalIgnoreCase)) || !seenSirets.Add(siret))
                || !string.IsNullOrWhiteSpace(email) && (existing.Any(x => string.Equals(x.Email, email, StringComparison.OrdinalIgnoreCase)) || !seenEmails.Add(email));
            previews.Add(new CustomerImportRowPreview(index + 2, Value(values, CustomerImportField.Name) ?? string.Empty, email, siret, duplicate, errors));
        }

        return new CustomerImportPreview(previews);
    }

    public async Task<CustomerImportResult> ImportAsync(CustomerImportFile file, IReadOnlyList<CustomerImportMapping> mappings, CancellationToken cancellationToken = default)
    {
        var preview = await PreviewAsync(file, mappings, cancellationToken);
        var imported = 0;

        for (var index = 0; index < file.Rows.Count; index++)
        {
            if (!preview.Rows[index].IsReady) continue;
            var values = Map(file.Rows[index], mappings);
            await customerService.CreateAsync(ToCommand(values), cancellationToken);
            imported++;
        }

        return new CustomerImportResult(imported, preview.DuplicateCount, preview.InvalidCount);
    }

    private static void ValidateMappings(IReadOnlyList<CustomerImportMapping> mappings)
    {
        var mapped = mappings.Where(x => x.Field != CustomerImportField.Ignore).ToArray();
        if (mapped.GroupBy(x => x.Field).Any(x => x.Count() > 1))
            throw new ArgumentException("Un champ Freecrmlance ne peut être associé qu'à une seule colonne.");
        foreach (var required in new[] { CustomerImportField.Name, CustomerImportField.Type, CustomerImportField.AddressLine1, CustomerImportField.PostalCode, CustomerImportField.City, CustomerImportField.CountryCode })
            if (mapped.All(x => x.Field != required))
                throw new ArgumentException($"Le champ {required} doit être associé.");
    }

    private static Dictionary<CustomerImportField, string?> Map(IReadOnlyList<string> row, IReadOnlyList<CustomerImportMapping> mappings)
        => mappings.Where(x => x.Field != CustomerImportField.Ignore).ToDictionary(x => x.Field, x => x.ColumnIndex < row.Count ? Normalize(row[x.ColumnIndex]) : null);

    private static List<string> Validate(IReadOnlyDictionary<CustomerImportField, string?> values)
    {
        var errors = new List<string>();
        foreach (var (field, label) in new[] {
            (CustomerImportField.Name, "Nom manquant"), (CustomerImportField.Type, "Type manquant"),
            (CustomerImportField.AddressLine1, "Adresse manquante"), (CustomerImportField.PostalCode, "Code postal manquant"),
            (CustomerImportField.City, "Ville manquante"), (CustomerImportField.CountryCode, "Pays manquant") })
            if (string.IsNullOrWhiteSpace(Value(values, field))) errors.Add(label);

        var email = Value(values, CustomerImportField.Email);
        if (email is not null && (!email.Contains('@') || email.StartsWith('@') || email.EndsWith('@'))) errors.Add("Email invalide");
        var siren = Value(values, CustomerImportField.Siren);
        if (siren is not null && (siren.Length != 9 || !siren.All(char.IsDigit))) errors.Add("SIREN invalide");
        var siret = Value(values, CustomerImportField.Siret);
        if (siret is not null && (siret.Length != 14 || !siret.All(char.IsDigit))) errors.Add("SIRET invalide");
        if (!TryType(Value(values, CustomerImportField.Type), out var type)) errors.Add("Type invalide (Entreprise ou Particulier)");
        else if (type == CustomerType.Business && string.IsNullOrWhiteSpace(siren)) errors.Add("SIREN requis pour une entreprise");
        var country = Value(values, CustomerImportField.CountryCode);
        if (country is not null && country.Length != 2) errors.Add("Le pays doit être un code à 2 lettres (ex. FR)");
        return errors;
    }

    private static CreateCustomerCommand ToCommand(IReadOnlyDictionary<CustomerImportField, string?> values)
    {
        TryType(Value(values, CustomerImportField.Type), out var type);
        return new CreateCustomerCommand(
            Value(values, CustomerImportField.Name)!, Value(values, CustomerImportField.Email), Value(values, CustomerImportField.Phone), [],
            type, Value(values, CustomerImportField.Siren), Value(values, CustomerImportField.Siret), Value(values, CustomerImportField.VatNumber),
            Value(values, CustomerImportField.AddressLine1)!, Value(values, CustomerImportField.AddressLine2), Value(values, CustomerImportField.PostalCode)!,
            Value(values, CustomerImportField.City)!, Value(values, CustomerImportField.CountryCode)!.ToUpperInvariant(),
            null, null, null, null, null);
    }

    private static bool TryType(string? value, out CustomerType type)
    {
        var normalized = value?.Trim().ToLowerInvariant();
        if (normalized is "entreprise" or "business" or "societe" or "société") { type = CustomerType.Business; return true; }
        if (normalized is "particulier" or "individual" or "personne") { type = CustomerType.Individual; return true; }
        type = default; return false;
    }

    private static string? Value(IReadOnlyDictionary<CustomerImportField, string?> values, CustomerImportField field)
        => values.TryGetValue(field, out var value) ? value : null;
    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
