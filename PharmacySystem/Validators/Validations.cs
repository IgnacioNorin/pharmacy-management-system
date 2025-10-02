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

    public class RUCValidator
    {
        public static bool ValidarIdentificacion(string identificacion)
        {
            if (string.IsNullOrWhiteSpace(identificacion) || identificacion.Length < 10)
                return false;

            if (!long.TryParse(identificacion, out _))
                return false;

            if (identificacion.Length == 10)
                return ValidarCedula(identificacion);

            if (identificacion.Length == 13)
                return EsRUCValido(identificacion);

            return false;
        }

        public static bool EsRUCValido(string ruc)
        {
            if (ruc.Length != 13)
                return false;

            int provincia = int.Parse(ruc.Substring(0, 2));
            if (provincia < 1 || provincia > 24)
                return false;

            int tercerDigito = int.Parse(ruc.Substring(2, 1));

            switch (tercerDigito)
            {
                case int n when (n >= 0 && n <= 5):
                    return ValidarPersonaNatural(ruc);

                case 6:
                    return ValidarSociedadPublica(ruc);

                case 9:
                    return ValidarSociedadPrivada(ruc);

                default:
                    return false;
            }
        }

        private static bool ValidarPersonaNatural(string ruc)
        {
            string cedula = ruc.Substring(0, 10);

            if (!ValidarCedula(cedula))
                return false;

            return ruc.EndsWith("001");
        }

        private static bool ValidarCedula(string cedula)
        {
            if (cedula.Length != 10)
                return false;

            int provincia = int.Parse(cedula.Substring(0, 2));
            if (provincia < 1 || provincia > 24)
                return false;

            int tercerDigito = int.Parse(cedula.Substring(2, 1));
            if (tercerDigito < 0 || tercerDigito > 5)
                return false;

            int[] coef = { 2, 1, 2, 1, 2, 1, 2, 1, 2 };
            int suma = 0;

            for (int i = 0; i < 9; i++)
            {
                int digito = int.Parse(cedula[i].ToString());
                int producto = digito * coef[i];
                if (producto >= 10)
                    producto -= 9;
                suma += producto;
            }

            int verificador = (10 - (suma % 10)) % 10;

            return verificador == int.Parse(cedula[9].ToString());
        }

        private static bool ValidarSociedadPublica(string ruc)
        {
            if (ruc.Length != 13)
                return false;

            int provincia = int.Parse(ruc.Substring(0, 2));
            if (provincia < 1 || provincia > 24)
                return false;

            if (ruc[2] != '6')
                return false;

            int[] coef = { 3, 2, 7, 6, 5, 4, 3, 2 };
            int suma = 0;

            for (int i = 0; i < coef.Length; i++)
            {
                int digito = int.Parse(ruc[i].ToString());
                suma += digito * coef[i];
            }

            int verificador = 11 - (suma % 11);
            if (verificador == 11) verificador = 0;
            if (verificador == 10) return false;

            int digitoVerificador = int.Parse(ruc[8].ToString());

            return verificador == digitoVerificador && ruc.EndsWith("0001");
        }

        private static bool ValidarSociedadPrivada(string ruc)
        {
            if (ruc.Length != 13)
                return false;

            int provincia = int.Parse(ruc.Substring(0, 2));
            if (provincia < 1 || provincia > 24)
                return false;

            if (ruc[2] != '9')
                return false;

            int[] coef = { 4, 3, 2, 7, 6, 5, 4, 3, 2 };
            int suma = 0;

            for (int i = 0; i < coef.Length; i++)
            {
                int digito = int.Parse(ruc[i].ToString());
                suma += digito * coef[i];
            }

            int verificador = 11 - (suma % 11);
            if (verificador == 11) verificador = 0;
            if (verificador == 10) return false;

            int digitoVerificador = int.Parse(ruc[9].ToString());

            return verificador == digitoVerificador && ruc.EndsWith("001");
        }
    }

}
