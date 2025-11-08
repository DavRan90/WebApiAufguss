using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Aufguss.Models.Validation
{

    public class PasswordComplexityAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var password = value as string;
            if (string.IsNullOrWhiteSpace(password))
                return new ValidationResult("Lösenord är obligatoriskt.");

            var errors = new List<string>();

            if (password.Length < 8)
                errors.Add("Minst 8 tecken");
            if (!Regex.IsMatch(password, "[A-Z]"))
                errors.Add("En stor bokstav");
            if (!Regex.IsMatch(password, "[a-z]"))
                errors.Add("En liten bokstav");
            if (!Regex.IsMatch(password, "[0-9]"))
                errors.Add("En siffra");
            if (!Regex.IsMatch(password, @"[\W_]"))
                errors.Add("Ett specialtecken");

            if (errors.Any())
            {
                return new ValidationResult($"Lösenordet uppfyller inte kraven: {string.Join(", ", errors)}");
            }

            return ValidationResult.Success;
        }
    }

}
