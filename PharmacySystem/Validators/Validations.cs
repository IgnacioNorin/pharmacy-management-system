using PharmacySystem.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PharmacySystem.Validators
{
    // UI-side rule table: maps a rule name to a predicate and the message shown to the user.
    // Document validation is country-agnostic (DocumentValidator in PharmacySystem.Domain):
    // a national check-digit scheme would tie the system to one jurisdiction.
    public static class Validations
    {
        private static string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        public static Dictionary<string, RuleValidation> rules = new Dictionary<string, RuleValidation>
        {
            {
                "NotEmpty", new RuleValidation(
                    value => !string.IsNullOrWhiteSpace(value),
                    "Este campo no puede estar vacío")
            },
            {
                "ComboNotEmpty", new RuleValidation(
                    value => !string.IsNullOrWhiteSpace(value),
                    "No hay registros para seleccionar, creé uno")
            },
            {
                // Phone-style field: digits plus the usual separators (+ - ( ) space), 6-20
                // chars. Was int.TryParse, which rejected long or country-prefixed numbers even
                // when they were all digits (DEF-41).
                "OnlyNumbers", new RuleValidation(
                    value => Regex.IsMatch(value ?? "", @"^\+?[0-9()\-\s]{6,20}$"),
                    "Teléfono inválido: use 6 a 20 dígitos (se admiten + - ( ) y espacios)")
            },
            {
                "ValidateEmail", new RuleValidation(
                    value => Regex.IsMatch(value, pattern),
                    "Correo inválido")
            },
            {
                "ValidateDocument", new RuleValidation(
                    value => DocumentValidator.IsValid(value),
                    "Documento inválido: use entre 3 y 20 letras, números, punto o guion")
            },
            {
                "ValidateMaxLength", new RuleValidation(
                    value => !(value.Length > 50),
                    "Superó el máximo de caracteres permitidos")
            },
            // Longer limits for the store profile fields, which hold a full "razón social" and
            // address (the store table columns were widened in migration 027).
            {
                "MaxLength120", new RuleValidation(
                    value => value.Length <= 120,
                    "Superó el máximo de 120 caracteres")
            },
            {
                "MaxLength150", new RuleValidation(
                    value => value.Length <= 150,
                    "Superó el máximo de 150 caracteres")
            },
            {
                "MaxLength200", new RuleValidation(
                    value => value.Length <= 200,
                    "Superó el máximo de 200 caracteres")
            }
        };
    }
}
