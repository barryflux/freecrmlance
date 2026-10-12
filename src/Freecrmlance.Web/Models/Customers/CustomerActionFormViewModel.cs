using System.ComponentModel.DataAnnotations;
using Freecrmlance.Domain.Crm;
namespace Freecrmlance.Web.Models.Customers;
public sealed class CustomerActionFormViewModel
{
    [Required(ErrorMessage = "Le titre est obligatoire.")]
    [StringLength(200, ErrorMessage = "Le titre ne peut pas dépasser 200 caractères.")]
    public string Title { get; set; } = string.Empty;
    [StringLength(2000, ErrorMessage = "La description ne peut pas dépasser 2 000 caractères.")]
    public string? Description { get; set; }
    [DataType(DataType.Date)]
    public DateOnly? DueDate { get; set; }
    [EnumDataType(typeof(CustomerActionStatus))]
    public CustomerActionStatus Status { get; set; } = CustomerActionStatus.Todo;
}
