using System.Text;
using ClosedXML.Excel;
using Freecrmlance.Application.Crm;

namespace Freecrmlance.Infrastructure.Crm;

public sealed class CustomerImportFileReader : ICustomerImportFileReader
{
    public async Task<CustomerImportFile> ReadAsync(string fileName, Stream content, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".csv" => await ReadCsvAsync(content, cancellationToken),
            ".xlsx" => ReadXlsx(content),
            _ => throw new InvalidDataException("Format non pris en charge. Utilisez un fichier CSV ou XLSX.")
        };
    }

    private static async Task<CustomerImportFile> ReadCsvAsync(Stream content, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(content, Encoding.UTF8, true, leaveOpen: true);
        var text = await reader.ReadToEndAsync(cancellationToken);
        var separator = DetectSeparator(text);
        var records = ParseCsv(text, separator);
        return Build(records);
    }

    private static CustomerImportFile ReadXlsx(Stream content)
    {
        using var workbook = new XLWorkbook(content);
        var sheet = workbook.Worksheets.FirstOrDefault() ?? throw new InvalidDataException("Le classeur ne contient aucune feuille.");
        var used = sheet.RangeUsed();
        if (used is null) throw new InvalidDataException("La première feuille est vide.");

        var records = used.Rows().Select(row => (IReadOnlyList<string>)row.Cells().Select(cell => cell.GetFormattedString()).ToArray()).ToArray();
        return Build(records);
    }

    private static CustomerImportFile Build(IReadOnlyList<IReadOnlyList<string>> records)
    {
        if (records.Count < 2) throw new InvalidDataException("Le fichier doit contenir une ligne d'en-têtes et au moins un client.");
        var headers = records[0].Select(x => x.Trim()).ToArray();
        if (headers.All(string.IsNullOrWhiteSpace)) throw new InvalidDataException("La ligne d'en-têtes est vide.");

        var rows = records.Skip(1).Where(row => row.Any(value => !string.IsNullOrWhiteSpace(value))).Take(2000).ToArray();
        if (rows.Length == 0) throw new InvalidDataException("Aucune ligne client n'a été trouvée.");

        var columns = headers.Select((header, index) => new CustomerImportColumn(
            index,
            string.IsNullOrWhiteSpace(header) ? $"Colonne {index + 1}" : header,
            rows.Select(row => index < row.Count ? row[index] : string.Empty).Where(x => !string.IsNullOrWhiteSpace(x)).Take(3).ToArray(),
            CustomerImportMappingSuggester.Suggest(header))).ToArray();

        return new CustomerImportFile(columns, rows);
    }

    private static char DetectSeparator(string text)
    {
        var firstLine = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).FirstOrDefault() ?? string.Empty;
        var candidates = new[] { ';', ',', '\t' };
        return candidates.OrderByDescending(separator => firstLine.Count(c => c == separator)).First();
    }

    private static IReadOnlyList<IReadOnlyList<string>> ParseCsv(string text, char separator)
    {
        var records = new List<IReadOnlyList<string>>();
        var row = new List<string>();
        var field = new StringBuilder();
        var quoted = false;

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (c == '"')
            {
                if (quoted && i + 1 < text.Length && text[i + 1] == '"') { field.Append('"'); i++; }
                else quoted = !quoted;
            }
            else if (c == separator && !quoted) { row.Add(field.ToString()); field.Clear(); }
            else if ((c == '\n' || c == '\r') && !quoted)
            {
                if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n') i++;
                row.Add(field.ToString()); field.Clear();
                records.Add(row.ToArray()); row = [];
            }
            else field.Append(c);
        }

        if (field.Length > 0 || row.Count > 0) { row.Add(field.ToString()); records.Add(row.ToArray()); }
        return records;
    }
}
