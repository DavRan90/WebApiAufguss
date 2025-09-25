using System.ComponentModel.DataAnnotations;

namespace Aufguss
{
    public enum Gender
    {
        [Display(Name = "Man")]
        Man,
        [Display(Name = "Kvinna")]
        Woman
    }
}
