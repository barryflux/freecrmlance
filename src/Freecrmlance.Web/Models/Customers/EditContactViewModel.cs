using System.ComponentModel.DataAnnotations;

namespace Freecrmlance.Web.Models.Customers;

public sealed class EditContactViewModel
{
    [Required, StringLength(200), Display(Name = "Nom")]
    public string Name { get; set; } = string.Empty;

    [EmailAddress, StringLength(320), Display(Name = "E-mail")]
    public string? Email { get; set; }

    [StringLength(50), Display(Name = "Téléphone")]
    public string? Phone { get; set; }

    [StringLength(100), Display(Name = "Fonction")]
    public string? Role { get; set; }
}
