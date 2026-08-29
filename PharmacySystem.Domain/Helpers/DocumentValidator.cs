namespace PharmacySystem.Validators
{
    // Country-agnostic format check for an identification / tax number (person, supplier, user,
    // store). It deliberately does NOT know about any national scheme: a check-digit algorithm
    // (cedula/RUC, DNI, RUT, NIF, ...) belongs to a specific jurisdiction and would lock the
    // system to one country. Uniqueness is already enforced by the database unique indexes.
    // The namespace is kept as PharmacySystem.Validators so existing callers compile unchanged.
    public static class DocumentValidator
    {
        public const int MinLength = 3;
        public const int MaxLength = 20;

        public static bool IsValid(string document)
        {
            if (string.IsNullOrWhiteSpace(document))
            {
                return false;
            }

            string value = document.Trim();
            if (value.Length < MinLength || value.Length > MaxLength)
            {
                return false;
            }

            foreach (char c in value)
            {
                if (!char.IsLetterOrDigit(c) && c != '-' && c != '.')
                {
                    return false;
                }
            }

            return true;
        }
    }
}
