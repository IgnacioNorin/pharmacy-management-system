namespace PharmacySystem.Model
{
    public enum PasswordChangeResult
    {
        Ok,
        // The "current password" entered does not match what is stored (self-service change only).
        WrongCurrent,
        // The new password is shorter than PasswordRules.MinLength.
        TooShort,
        // The new password equals the current one.
        SameAsOld
    }

    public static class PasswordRules
    {
        // Length only, no complexity rules - a counter operator has to be able to type it fast.
        public const int MinLength = 6;
    }
}
