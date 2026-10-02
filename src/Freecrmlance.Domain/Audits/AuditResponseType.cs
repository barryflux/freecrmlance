using System.ComponentModel.DataAnnotations;
namespace Freecrmlance.Domain.Audits;
public enum AuditResponseType
{
 [Display(Name="Oui / Non")] YesNo,
 [Display(Name="Conforme / Non conforme")] CompliantNonCompliant,
 [Display(Name="Texte")] Text,
 [Display(Name="Texte long")] LongText,
 [Display(Name="Nombre")] Number,
 [Display(Name="Note")] Rating,
 [Display(Name="Choix unique")] SingleChoice,
 [Display(Name="Choix multiple")] MultipleChoice
}