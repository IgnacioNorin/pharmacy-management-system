using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    public interface ILoginView
    {
        string Document { get; }
        string Password { get; }

        void LoginSucceeded(Person person);
        void ShowError(string message);
    }
}
