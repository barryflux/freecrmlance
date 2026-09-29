using System.Text;
using System.Text.Json;
using Freecrmlance.Application.Crm;
using Freecrmlance.Web.Models.Customers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Freecrmlance.Web.Controllers;

[Authorize]
[Route("Customers/Import")]
public sealed class CustomerImportsController(ICustomerImportFileReader fileReader, CustomerImportService importService) : Controller
{
    private const string FileKey = "customer-import-file";
    private const string FileNameKey = "customer-import-file-name";
    private const string MappingKey = "customer-import-mapping";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [HttpGet("")]
    public IActionResult Index() => View();

    [HttpGet("Template")]
    public IActionResult Template()
    {
        const string csv = "Nom;Type;Email;Téléphone;SIREN;SIRET;N° TVA;Adresse;Complément;Code postal;Ville;Pays\r\n" +
                           "Exemple SAS;Entreprise;contact@exemple.fr;0102030405;123456789;12345678900012;FR00123456789;1 rue Exemple;;75001;Paris;FR\r\n";
        return File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv)).ToArray(), "text/csv", "modele-clients-freecrmlance.csv");
    }

    [HttpPost("Upload")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            ModelState.AddModelError("", "Sélectionnez un fichier CSV ou XLSX.");
            return View("Index");
        }

        if (file.Length > 5 * 1024 * 1024)
        {
            ModelState.AddModelError("", "Le fichier dépasse la taille maximale de 5 Mo.");
            return View("Index");
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var parsed = await fileReader.ReadAsync(file.FileName, stream, cancellationToken);
            HttpContext.Session.SetString(FileKey, JsonSerializer.Serialize(parsed, JsonOptions));
            HttpContext.Session.SetString(FileNameKey, Path.GetFileName(file.FileName));
            HttpContext.Session.Remove(MappingKey);
            return RedirectToAction(nameof(Map));
        }
        catch (InvalidDataException exception)
        {
            ModelState.AddModelError("", exception.Message);
            return View("Index");
        }
    }

    [HttpGet("Map")]
    public IActionResult Map()
    {
        var file = GetFile();
        if (file is null) return RedirectToAction(nameof(Index));
        return View(new CustomerImportMapViewModel { FileName = HttpContext.Session.GetString(FileNameKey) ?? "Fichier", Columns = file.Columns });
    }

    [HttpPost("Preview")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Preview(CancellationToken cancellationToken)
    {
        var file = GetFile();
        if (file is null) return RedirectToAction(nameof(Index));

        var mappings = file.Columns.Select(column =>
        {
            var raw = Request.Form[$"mapping_{column.Index}"].ToString();
            return new CustomerImportMapping(column.Index, Enum.TryParse<CustomerImportField>(raw, out var field) ? field : CustomerImportField.Ignore);
        }).ToArray();

        try
        {
            var preview = await importService.PreviewAsync(file, mappings, cancellationToken);
            HttpContext.Session.SetString(MappingKey, JsonSerializer.Serialize(mappings, JsonOptions));
            return View(new CustomerImportPreviewViewModel { Preview = preview, Mappings = mappings });
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError("", exception.Message);
            return View("Map", new CustomerImportMapViewModel { FileName = HttpContext.Session.GetString(FileNameKey) ?? "Fichier", Columns = file.Columns });
        }
    }

    [HttpPost("Confirm")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(CancellationToken cancellationToken)
    {
        var file = GetFile();
        var mappingsJson = HttpContext.Session.GetString(MappingKey);
        if (file is null || mappingsJson is null) return RedirectToAction(nameof(Index));
        var mappings = JsonSerializer.Deserialize<CustomerImportMapping[]>(mappingsJson, JsonOptions) ?? [];
        var result = await importService.ImportAsync(file, mappings, cancellationToken);
        HttpContext.Session.Remove(FileKey);
        HttpContext.Session.Remove(FileNameKey);
        HttpContext.Session.Remove(MappingKey);
        return View("Result", new CustomerImportResultViewModel { Result = result });
    }

    private CustomerImportFile? GetFile()
    {
        var json = HttpContext.Session.GetString(FileKey);
        return json is null ? null : JsonSerializer.Deserialize<CustomerImportFile>(json, JsonOptions);
    }
}
