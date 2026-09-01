using PharmacySystem.Business;
using PharmacySystem.Infrastructure;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // The login decision (brute-force lockout, credential check, legacy plain-text -> hash
    // migration, disabled-account check, must-change-password gate) lives in
    // AuthenticationService now. This presenter only turns the AuthResult into a view call.
    public class LoginPresenter
    {
        private readonly ILoginView _view;
        private readonly IAuthenticationService _authService;

        public LoginPresenter(ILoginView view, IAuthenticationService authService)
        {
            _view = view;
            _authService = authService;
        }

        public void OnLogin()
        {
            try
            {
                AuthResult result = _authService.Authenticate(_view.Document, _view.Password);

                switch (result.Status)
                {
                    case AuthStatus.Ok:
                        _view.LoginSucceeded(result.Person!);
                        break;
                    case AuthStatus.MustChangePassword:
                        _view.RequirePasswordChange(result.Person!);
                        break;
                    case AuthStatus.LockedOut:
                        _view.ShowError($"Cuenta bloqueada temporalmente por intentos fallidos. Reintente en {result.RetryAfterMinutes} minuto(s).");
                        break;
                    default:
                        _view.ShowError("No se encontraron coincidencias del usuario");
                        break;
                }
            }
            catch (DataUnavailableException ex)
            {
                _view.ShowError(ex.Message);
            }
        }
    }
}
