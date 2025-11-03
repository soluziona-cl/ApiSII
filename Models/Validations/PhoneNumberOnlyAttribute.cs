using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ApiSII.Models.Validations
{
    public class PhoneNumberOnlyAttribute : ValidationAttribute
    {
        private static readonly Regex PhoneRegex = new Regex(@"^[0-9]+$", RegexOptions.Compiled);

        public PhoneNumberOnlyAttribute() : base("El campo {0} solo puede contener números.")
        {
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success; // Permitir null, usar Required para campos obligatorios
            }

            var phoneNumber = value.ToString();
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return ValidationResult.Success; // Permitir vacío, usar Required para campos obligatorios
            }

            // Eliminar espacios, guiones y otros caracteres para validar
            var cleanedPhone = phoneNumber.Replace(" ", "")
                                         .Replace("-", "")
                                         .Replace("(", "")
                                         .Replace(")", "")
                                         .Replace("+", "")
                                         .Replace(".", "");

            if (!PhoneRegex.IsMatch(cleanedPhone))
            {
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            }

            return ValidationResult.Success;
        }
    }
}

