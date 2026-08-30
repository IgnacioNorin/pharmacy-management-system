namespace PharmacySystem.Model
{
    public enum AuthStatus
    {
        // Wrong document, wrong password or a disabled account - all reported the same way
        // so the message never reveals whether an account exists.
        InvalidCredentials,
        Ok,
        // Too many recent failures for this document; RetryAfterMinutes says how long to wait.
        LockedOut,
        // Credentials are valid but person.must_change_password is set: the caller must run
        // the password change before letting the user in.
        MustChangePassword
    }

    // Outcome of AuthenticationService.Authenticate. Person is set on Ok and MustChangePassword;
    // RetryAfterMinutes is set on LockedOut.
    public class AuthResult
    {
        public AuthStatus Status { get; private set; }
        public Person Person { get; private set; }
        public int RetryAfterMinutes { get; private set; }

        public static AuthResult Invalid() => new AuthResult { Status = AuthStatus.InvalidCredentials };

        public static AuthResult Locked(int retryAfterMinutes) =>
            new AuthResult { Status = AuthStatus.LockedOut, RetryAfterMinutes = retryAfterMinutes };

        public static AuthResult Success(Person person) =>
            new AuthResult { Status = AuthStatus.Ok, Person = person };

        public static AuthResult NeedsPasswordChange(Person person) =>
            new AuthResult { Status = AuthStatus.MustChangePassword, Person = person };
    }
}
