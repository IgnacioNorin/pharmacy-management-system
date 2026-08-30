using PharmacySystem.Business;
using PharmacySystem.Infrastructure;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // Drives ModalChangePassword for both entry points: the mandatory dialog after a
    // must-change-password login, and the always-available "Cambiar contraseña" self-service.
    public class ChangePasswordPresenter
    {
        private readonly IChangePasswordView _view;
        private readonly IPasswordChangeService _service;
        private readonly int _personId;

        public ChangePasswordPresenter(IChangePasswordView view, IPasswordChangeService service, int personId)
        {
            _view = view;
            _service = service;
            _personId = personId;
        }

        public void OnSave()
        {
            if (_view.NewPassword != _view.ConfirmPassword)
            {
                _view.ShowError("La nueva contraseña y su confirmación no coinciden.");
                return;
            }

            try
            {
                switch (_service.ChangeOwnPassword(_personId, _view.CurrentPassword, _view.NewPassword))
                {
                    case PasswordChangeResult.Ok:
                        _view.Close(true);
                        break;
                    case PasswordChangeResult.WrongCurrent:
                        _view.ShowError("La contraseña actual no es correcta.");
                        break;
                    case PasswordChangeResult.TooShort:
                        _view.ShowError($"La nueva contraseña debe tener al menos {PasswordRules.MinLength} caracteres.");
                        break;
                    case PasswordChangeResult.SameAsOld:
                        _view.ShowError("La nueva contraseña debe ser distinta de la actual.");
                        break;
                }
            }
            catch (DataUnavailableException ex)
            {
                _view.ShowError(ex.Message);
            }
        }

        public void OnCancel()
        {
            if (!_view.Mandatory)
            {
                _view.Close(false);
            }
        }
    }
}
