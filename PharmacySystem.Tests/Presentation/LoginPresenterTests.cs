using PharmacySystem.Infrastructure;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using Xunit;

namespace PharmacySystem.Tests.Presentation
{
    // The login decision (lockout, credential check, plain-text migration, disabled account,
    // must-change gate) lives in AuthenticationService now; this presenter only maps the
    // AuthResult to a view call.
    public class LoginPresenterTests
    {
        private static LoginPresenter CreatePresenter(FakeLoginView view, FakeAuthenticationService auth)
            => new LoginPresenter(view, auth);

        [Fact]
        public void OnLogin_Ok_LogsInWithTheReturnedPerson()
        {
            var person = new Person { idPerson = 1 };
            var view = new FakeLoginView { Document = "123", Password = "correct" };
            var auth = new FakeAuthenticationService { Result = AuthResult.Success(person) };

            CreatePresenter(view, auth).OnLogin();

            Assert.Same(person, view.LoggedInPerson);
            Assert.Null(view.ShownError);
        }

        [Fact]
        public void OnLogin_MustChangePassword_AsksTheViewToRunTheChangeInsteadOfLoggingIn()
        {
            var person = new Person { idPerson = 7 };
            var view = new FakeLoginView { Document = "123", Password = "temp" };
            var auth = new FakeAuthenticationService { Result = AuthResult.NeedsPasswordChange(person) };

            CreatePresenter(view, auth).OnLogin();

            Assert.Same(person, view.PasswordChangeRequiredFor);
            Assert.Null(view.LoggedInPerson);
        }

        [Fact]
        public void OnLogin_LockedOut_ShowsTheLockMessageWithTheRemainingMinutes()
        {
            var view = new FakeLoginView { Document = "123", Password = "x" };
            var auth = new FakeAuthenticationService { Result = AuthResult.Locked(7) };

            CreatePresenter(view, auth).OnLogin();

            Assert.Contains("7", view.ShownError);
            Assert.Contains("bloqueada", view.ShownError);
            Assert.Null(view.LoggedInPerson);
        }

        [Fact]
        public void OnLogin_InvalidCredentials_ShowsTheGenericError()
        {
            var view = new FakeLoginView { Document = "123", Password = "wrong" };
            var auth = new FakeAuthenticationService { Result = AuthResult.Invalid() };

            CreatePresenter(view, auth).OnLogin();

            Assert.Equal("No se encontraron coincidencias del usuario", view.ShownError);
            Assert.Null(view.LoggedInPerson);
        }

        [Fact]
        public void OnLogin_PassesTheEnteredDocumentAndPasswordToTheService()
        {
            var view = new FakeLoginView { Document = "  123  ", Password = "pw" };
            var auth = new FakeAuthenticationService { Result = AuthResult.Invalid() };

            CreatePresenter(view, auth).OnLogin();

            Assert.Equal(("  123  ", "pw"), auth.AuthenticatedWith);
        }

        [Fact]
        public void OnLogin_DatabaseUnavailable_ShowsConnectionErrorInsteadOfNoMatches()
        {
            var view = new FakeLoginView { Document = "123", Password = "x" };
            var auth = new FakeAuthenticationService { AuthenticateThrows = new DataUnavailableException() };

            CreatePresenter(view, auth).OnLogin();

            Assert.Equal(DataUnavailableException.DefaultMessage, view.ShownError);
            Assert.Null(view.LoggedInPerson);
        }
    }
}
