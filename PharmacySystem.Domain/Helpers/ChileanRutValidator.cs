namespace PharmacySystem.Validators
{
    // Chilean RUT / RUN check-digit validation (modulo 11). Used for the recipient's tax id on
    // a Factura - a Chilean fiscal document, so this check is jurisdiction-specific on purpose
    // (the country-neutral DocumentValidator still covers every other id field). Isolated in one
    // file so a different country's rule is a one-line swap at the call site.
    public static class ChileanRutValidator
    {
        public static bool IsValid(string rut)
        {
            if (string.IsNullOrWhiteSpace(rut))
            {
                return false;
            }

            string clean = rut.Trim().ToUpperInvariant().Replace(".", "").Replace("-", "");
            if (clean.Length < 2)
            {
                return false;
            }

            string body = clean.Substring(0, clean.Length - 1);
            char checkDigit = clean[clean.Length - 1];

            foreach (char c in body)
            {
                if (c < '0' || c > '9')
                {
                    return false;
                }
            }

            int sum = 0;
            int factor = 2;
            for (int i = body.Length - 1; i >= 0; i--)
            {
                sum += (body[i] - '0') * factor;
                factor = factor == 7 ? 2 : factor + 1;
            }

            int mod = 11 - (sum % 11);
            char expected = mod == 11 ? '0' : mod == 10 ? 'K' : (char)('0' + mod);

            return checkDigit == expected;
        }
    }
}
