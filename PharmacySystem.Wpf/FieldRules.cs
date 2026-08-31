using System.Collections.Generic;
using System.Text.RegularExpressions;
using PharmacySystem.Validators;

namespace PharmacySystem.Wpf
{
    // WPF-side mirror of PharmacySystem.Validators.Validations.rules (which lives in the WinForms
    // exe and can't be referenced here). Same predicates and same user-facing messages, so a
    // migrated screen validates identically to its WinForms original.
    internal static class FieldRules
    {
        private const string EmailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        private const string PhonePattern = @"^\+?[0-9()\-\s]{6,20}$";

        // Adds one message per failed rule to errors. label is the field caption shown to the user.
        public static void Check(List<string> errors, string label, string value, params string[] ruleKeys)
        {
            value = value ?? "";
            foreach (string key in ruleKeys)
            {
                if (!Passes(key, value))
                {
                    errors.Add(label + " : " + MessageFor(key));
                }
            }
        }

        private static bool Passes(string key, string value)
        {
            switch (key)
            {
                case "NotEmpty":
                case "ComboNotEmpty": return !string.IsNullOrWhiteSpace(value);
                case "OnlyNumbers": return Regex.IsMatch(value, PhonePattern);
                case "ValidateEmail": return Regex.IsMatch(value, EmailPattern);
                case "ValidateDocument": return DocumentValidator.IsValid(value);
                case "ValidateMaxLength": return value.Length <= 50;
                case "MaxLength120": return value.Length <= 120;
                case "MaxLength150": return value.Length <= 150;
                case "MaxLength200": return value.Length <= 200;
                default: return true;
            }
        }

        private static string MessageFor(string key)
        {
            switch (key)
            {
                case "NotEmpty": return "Este campo no puede estar vacío";
                case "ComboNotEmpty": return "No hay registros para seleccionar, creé uno";
                case "OnlyNumbers": return "Teléfono inválido: use 6 a 20 dígitos (se admiten + - ( ) y espacios)";
                case "ValidateEmail": return "Correo inválido";
                case "ValidateDocument": return "Documento inválido: use entre 3 y 20 letras, números, punto o guion";
                case "ValidateMaxLength": return "Superó el máximo de caracteres permitidos";
                case "MaxLength120": return "Superó el máximo de 120 caracteres";
                case "MaxLength150": return "Superó el máximo de 150 caracteres";
                case "MaxLength200": return "Superó el máximo de 200 caracteres";
                default: return "Valor inválido";
            }
        }
    }
}
