namespace PharmacySystem.Data
{
    // Authentication audit trail. Failed rows drive the lockout; a row with success = true
    // (a good login, an admin unlock or an admin reset) resets the running failure count.
    public interface ILoginAttemptRepository
    {
        void Record(string document, bool success, string reason, int? actorId, string station);

        // Failed attempts for this document inside the window that are newer than its most
        // recent success row.
        int CountFailuresSinceLastReset(string document, int windowMinutes);

        // Minutes left until the oldest counting failure ages out of the window, or null if
        // there is none. Computed entirely on the server so it does not depend on the app and
        // SQL Server sharing a time zone.
        int? MinutesUntilUnlock(string document, int windowMinutes);
    }
}
