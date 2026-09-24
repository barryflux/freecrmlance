using System.ComponentModel.DataAnnotations;

namespace Freecrmlance.Web.Models.Customers;

public sealed class EditContactViewModel
{
    [Required, StringLength(200)] public string Name { get; set; } = string.Empty;
    [EmailAddress, StringLength(320)] public string? Email { get; set; }
    [StringLength(50)] public string? Phone { get; set; }
    [StringLength(100)] public string? Role { get; set; }
}
