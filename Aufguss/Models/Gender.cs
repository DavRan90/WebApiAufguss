using System.ComponentModel.DataAnnotations;

namespace Aufguss.Models
{
    public enum Gender
    {
        [Display(Name = "Man")]
        Male,

        [Display(Name = "Kvinna")]
        Female,

        [Display(Name = "Annat")]
        Other
    }
}
