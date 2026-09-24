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

    public List<CreateContactViewModel> Contacts { get; set; } = [new()];
}
