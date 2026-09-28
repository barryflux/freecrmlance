using System.ComponentModel.DataAnnotations;

namespace Freecrmlance.Web.Models.WorkspaceSettings;

public sealed class WorkspaceLegalIdentityViewModel
{
    public string WorkspaceName { get; set; } = string.Empty;
    [Required, StringLength(200)] public string LegalName { get; set; } = string.Empty;
    [StringLength(100)] public string? LegalForm { get; set; }
    [RegularExpression(@"^\d{9}$", ErrorMessage = "SIREN must contain 9 digits.")] public string? Siren { get; set; }
    [RegularExpression(@"^\d{14}$", ErrorMessage = "SIRET must contain 14 digits.")] public string? Siret { get; set; }
    [StringLength(30)] public string? VatNumber { get; set; }
    [Required, StringLength(200)] public string AddressLine1 { get; set; } = string.Empty;
    [StringLength(200)] public string? AddressLine2 { get; set; }
    [Required, StringLength(20)] public string PostalCode { get; set; } = string.Empty;
    [Required, StringLength(100)] public string City { get; set; } = string.Empty;
    [Required, StringLength(2, MinimumLength = 2)] public string CountryCode { get; set; } = "FR";
    [EmailAddress, StringLength(320)] public string? ContactEmail { get; set; }
    [StringLength(50)] public string? ContactPhone { get; set; }
}
