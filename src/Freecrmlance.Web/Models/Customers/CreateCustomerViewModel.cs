using System.ComponentModel.DataAnnotations;

namespace Freecrmlance.Web.Models.Customers;

public sealed class CreateCustomerViewModel
{
    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [EmailAddress, StringLength(320)]
    public string? Email { get; set; }

    [StringLength(50)]
    public string? Phone { get; set; }

    public List<CreateContactViewModel> Contacts { get; set; } = [];
    [Required]
    public Freecrmlance.Domain.Crm.CustomerType? Type { get; set; }

    [RegularExpression(@"^\d{9}$", ErrorMessage = "SIREN must contain 9 digits.")]
    public string? Siren { get; set; }

    [RegularExpression(@"^\d{14}$", ErrorMessage = "SIRET must contain 14 digits.")]
    public string? Siret { get; set; }

    [StringLength(30)]
    public string? VatNumber { get; set; }

    [Required, StringLength(200)]
    public string AddressLine1 { get; set; } = string.Empty;
    [StringLength(200)] public string? AddressLine2 { get; set; }
    [Required, StringLength(20)] public string PostalCode { get; set; } = string.Empty;
    [Required, StringLength(100)] public string City { get; set; } = string.Empty;
    [Required, StringLength(2, MinimumLength = 2)] public string CountryCode { get; set; } = "FR";

    [StringLength(200)] public string? BillingAddressLine1 { get; set; }
    [StringLength(200)] public string? BillingAddressLine2 { get; set; }
    [StringLength(20)] public string? BillingPostalCode { get; set; }
    [StringLength(100)] public string? BillingCity { get; set; }
    [StringLength(2, MinimumLength = 2)] public string? BillingCountryCode { get; set; }
}
