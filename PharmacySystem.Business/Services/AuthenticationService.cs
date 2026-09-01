using System;
using PharmacySystem.Data;
using PharmacySystem.Helpers;
using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    // Owns the whole login decision: brute-force lockout, credential check (including the
    // legacy plain-text -> hash migration), the disabled-account check and the
    // must-change-password gate. LoginPresenter only switches on the AuthResult.
    //
    // The lockout is derived from login_attempt, not a counter column: an account is locked
    // while it has >= MaxFailures failed attempts inside the last LockWindowMinutes that are
    // newer than its last success row. It clears by waiting the window out (with no new
    // attempts) or by any success row - a good login or an admin unlock/reset.
    public class AuthenticationService : IAuthenticationService
    {
        public const int MaxFailures = 5;
        public const int LockWindowMinutes = 15;

        private readonly IPersonRepository _personRepository;
        private readonly ILoginAttemptRepository _attempts;

        public AuthenticationService(IPersonRepository personRepository, ILoginAttemptRepository attempts)
        {
            _personRepository = personRepository;
            _attempts = attempts;
        }

        private static string Station => Environment.MachineName;

        public AuthResult Authenticate(string document, string password)
        {
            document = document?.Trim() ?? string.Empty;

            if (document.Length == 0)
            {
                _attempts.Record(document, false, "login", null, Station);
                return AuthResult.Invalid();
            }

            if (_attempts.CountFailuresSinceLastReset(document, LockWindowMinutes) >= MaxFailures)
            {
                _attempts.Record(document, false, "login", null, Station);
                return AuthResult.Locked(RemainingMinutes(document));
            }

            Person? person = _personRepository.GetByDocument(document);

            if (person == null || !PasswordMatches(person, password) || !person.Estado)
            {
                _attempts.Record(document, false, "login", null, Station);
                return AuthResult.Invalid();
            }

            _attempts.Record(document, true, "login", null, Station);

            return person.mustChangePassword
                ? AuthResult.NeedsPasswordChange(person)
                : AuthResult.Success(person);
        }

        public void Unlock(string document, int actorId)
        {
            _attempts.Record(document?.Trim() ?? string.Empty, true, "admin_unlock", actorId, Station);
        }

        public void RecordSuspension(string document, bool suspended, int actorId)
        {
            // A reactivation also clears the failure count (fresh start); a suspension is neutral.
            _attempts.Record(document?.Trim() ?? string.Empty, !suspended,
                suspended ? "admin_suspend" : "admin_reactivate", actorId, Station);
        }

        public System.Collections.Generic.ISet<string> GetLockedDocuments() =>
            _attempts.ListLockedDocuments(LockWindowMinutes, MaxFailures);

        // Verifies the password and, for a legacy plain-text row, rewrites it as a hash on the
        // spot - the same migration LoginPresenter used to do. It must NOT touch
        // must_change_password, so it goes through UpdatePassword, not SetPasswordAndFlag.
        private bool PasswordMatches(Person person, string enteredPassword)
        {
            if (PasswordHasher.IsHashed(person.password))
            {
                return PasswordHasher.Verify(enteredPassword, person.password);
            }

            if (person.password == enteredPassword)
            {
                _personRepository.UpdatePassword(person.idPerson, PasswordHasher.Hash(enteredPassword));
                return true;
            }

            return false;
        }

        private int RemainingMinutes(string document)
        {
            int? remaining = _attempts.MinutesUntilUnlock(document, LockWindowMinutes);
            if (remaining == null)
            {
                return LockWindowMinutes;
            }

            return remaining.Value < 1 ? 1 : remaining.Value;
        }
    }
}
