using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    public interface ILoginView
    {
        string Document { get; }
        string Password { get; }

        void LoginSucceeded(Person person);
        // Credentials are valid but the user must change the password before the app opens.
        // The view runs a mandatory change dialog and, only if it succeeds, proceeds as a login.
        void RequirePasswordChange(Person person);
        void ShowError(string message);
    }
}
