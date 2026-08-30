using PharmacySystem.Model;
using PharmacySystem.Presentation;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakeLoginView : ILoginView
    {
        public string Document { get; set; }
        public string Password { get; set; }

        public Person LoggedInPerson { get; private set; }
        public Person PasswordChangeRequiredFor { get; private set; }
        public string ShownError { get; private set; }

        public void LoginSucceeded(Person person) => LoggedInPerson = person;
        public void RequirePasswordChange(Person person) => PasswordChangeRequiredFor = person;
        public void ShowError(string message) => ShownError = message;
    }
}
