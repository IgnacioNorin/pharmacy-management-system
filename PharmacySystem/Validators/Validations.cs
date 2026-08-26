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
    // The check-digit algorithm it delegates to lives in PharmacySystem.Domain (RUCValidator).
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
                "OnlyNumbers", new RuleValidation(
                    value => int.TryParse(value, out _),
                    "Este campo solo debe contener números")
            },
            {
                "ValidateEmail", new RuleValidation(
                    value => Regex.IsMatch(value, pattern),
                    "Correo inválido")
            },
            {
                "ValidatorRUC/CI", new RuleValidation(
                    value => RUCValidator.ValidarIdentificacion(value.Trim()),
                    "Número inválido, ingrese nuevamente")
            },
            {
                "ValidateMaxLength", new RuleValidation(
                    value => !(value.Length > 50),
                    "Superó el máximo de caracteres permitidos")
            }
        };
    }
}
