using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Business;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // Ported from frmUser.cs. Preserves the original's asymmetry: a failed Register shows
    // "Ya existe un usuario..." and returns; a failed Update returns completely silently (same
    // shape as SupplierPresenter, unlike ClientPresenter which always shows a message).
    //
    // Administrador General is a protected role: a user whose current role is not Administrador
    // General cannot create, edit, delete or assign one, and nobody (not even an Administrador
    // General) can remove the last active one - otherwise there is no way back to role/tienda
    // administration. sp_delete_person / sp_update_person enforce the last-one rule at the DB
    // too; these checks just fail earlier with a clear message.
    public class UserPresenter
    {
        private const int AdminGeneralRoleId = (int)PersonType.AdministradorGeneral;

        private readonly IUserView _view;
        private readonly IPersonService _service;
        private readonly CurrentUser _currentUser;
        private readonly IPermissionService _permissionService;
        private readonly IPasswordChangeService _passwordChangeService;
        private readonly IAuthenticationService _authService;

        private List<Person> _users = new List<Person>();

        public UserPresenter(IUserView view, IPersonService service, CurrentUser currentUser,
            IPermissionService permissionService, IPasswordChangeService passwordChangeService,
            IAuthenticationService authService)
        {
            _view = view;
            _service = service;
            _currentUser = currentUser;
            _permissionService = permissionService;
            _passwordChangeService = passwordChangeService;
            _authService = authService;
        }

        private bool Can(string permission) => _currentUser?.Can(permission) ?? false;

        private bool CurrentIsAdminGeneral => (_currentUser?.RoleId ?? 0) == AdminGeneralRoleId;

        private int RoleOf(int userId) =>
            _users.FirstOrDefault(u => u.idPerson == userId)?.oPersonType?.idPersonType ?? 0;

        private string DocumentOf(int userId) =>
            _users.FirstOrDefault(u => u.idPerson == userId)?.document;

        private int ActorId => _currentUser?.PersonId ?? 0;

        private int ActiveAdminGeneralCount =>
            _users.Count(u => (u.oPersonType?.idPersonType ?? 0) == AdminGeneralRoleId && u.Estado);

        public void OnLoad()
        {
            // Roles come from person_type: the built-ins plus any custom role. Administrador
            // General is only offered to another Administrador General.
            var roleOptions = _permissionService.GetRoles()
                .Where(r => CurrentIsAdminGeneral || r.idPersonType != AdminGeneralRoleId)
                .Select(r => new ComboBoxItem { Value = r.idPersonType, Text = r.description });
            _view.LoadRoleOptions(roleOptions);

            _users = _service.List();

            _view.LoadUsers(_users.Select(p => new UserRow
            {
                Id = p.idPerson,
                Document = p.document,
                Name = p.name,
                RoleText = p.oPersonType.description
            }));
        }

        public void OnSave()
        {
            if (!Can("usuarios.gestionar"))
            {
                _view.ShowMessage("No tiene permiso para crear o editar usuarios.");
                return;
            }

            if (!EnsureAdminGeneralRulesForSave())
            {
                return;
            }

            var errors = _view.Validate();
            if (errors.Count > 0)
            {
                _view.ShowValidationErrors(errors);
                return;
            }

            if (_view.Password != _view.ConfirmPassword)
            {
                _view.ShowPasswordMismatch();
                return;
            }

            if (_view.SelectedIndex < 0 || _view.SelectedIndex > _view.RowCount)
            {
                return;
            }

            bool isNewUser = _view.UserId == 0;

            // On a new user the password is mandatory; on an edit a blank field means "keep the
            // current password" and PersonService.Update turns it into a no-op.
            if (isNewUser && string.IsNullOrWhiteSpace(_view.Password))
            {
                _view.ShowMessage("Debe ingresar una contraseña para el usuario nuevo");
                return;
            }

            Person person = new Person
            {
                idPerson = _view.UserId,
                document = _view.Document?.Trim(),
                name = _view.Name?.Trim(),
                address = "",
                phone = "",
                password = _view.Password,
                oPersonType = new TypePerson { idPersonType = _view.RoleId }
            };

            if (person.idPerson == 0)
            {
                int newId = _service.Register(person);
                if (newId == 0)
                {
                    _view.ShowMessage("Ya existe un usuario con ese documento");
                    return;
                }

                person.idPerson = newId;
                _view.AddRow(new UserRow
                {
                    Id = person.idPerson,
                    Document = person.document,
                    Name = person.name,
                    RoleText = _view.RoleText
                });
                _view.ClearForm();
            }
            else
            {
                if (!_service.Update(person))
                {
                    return;
                }

                _view.ReplaceRow(_view.SelectedIndex - 1, new UserRow
                {
                    Id = person.idPerson,
                    Document = person.document,
                    Name = person.name,
                    RoleText = _view.RoleText
                });
                _view.ClearForm();
            }
        }

        // Guards around the Administrador General role, run after the usuarios.gestionar check.
        private bool EnsureAdminGeneralRulesForSave()
        {
            bool isNewUser = _view.UserId == 0;
            int currentRoleOfTarget = isNewUser ? 0 : RoleOf(_view.UserId);

            if (!CurrentIsAdminGeneral)
            {
                if (_view.RoleId == AdminGeneralRoleId)
                {
                    _view.ShowMessage("Solo un Administrador General puede asignar el rol Administrador General.");
                    return false;
                }
                if (currentRoleOfTarget == AdminGeneralRoleId)
                {
                    _view.ShowMessage("No tiene permiso para modificar un Administrador General.");
                    return false;
                }
            }

            if (!isNewUser
                && currentRoleOfTarget == AdminGeneralRoleId
                && _view.RoleId != AdminGeneralRoleId
                && ActiveAdminGeneralCount <= 1)
            {
                _view.ShowMessage("No se puede quitar el rol al último Administrador General activo.");
                return false;
            }

            return true;
        }

        public void OnDelete()
        {
            if (_view.SelectedIndex <= 0)
            {
                _view.ShowMessage("No se pudo eliminar, seleccione un usuario");
                return;
            }

            if (!Can("usuarios.gestionar"))
            {
                _view.ShowMessage("No tiene permiso para eliminar usuarios.");
                return;
            }

            int targetRole = RoleOf(_view.UserId);
            if (targetRole == AdminGeneralRoleId && !CurrentIsAdminGeneral)
            {
                _view.ShowMessage("No tiene permiso para eliminar un Administrador General.");
                return;
            }
            if (targetRole == AdminGeneralRoleId && ActiveAdminGeneralCount <= 1)
            {
                _view.ShowMessage("No se puede eliminar al último Administrador General activo.");
                return;
            }

            if (!_view.ConfirmDelete())
            {
                return;
            }

            if (!_service.Delete(_view.UserId))
            {
                _view.ShowMessage("No se pudo eliminar el registro\nRevise los datos");
                return;
            }

            _view.RemoveRow(_view.SelectedIndex - 1);
            _view.ClearForm();
        }

        // Admin sets a temporary password for the selected user (typed into the same
        // Password / Confirm fields). The user is forced to change it on next login and the
        // reset is written to the audit trail.
        public void OnResetPassword()
        {
            if (_view.SelectedIndex <= 0)
            {
                _view.ShowMessage("Seleccione un usuario para restablecer su contraseña");
                return;
            }

            if (!Can("usuarios.gestionar"))
            {
                _view.ShowMessage("No tiene permiso para restablecer contraseñas.");
                return;
            }

            if (RoleOf(_view.UserId) == AdminGeneralRoleId && !CurrentIsAdminGeneral)
            {
                _view.ShowMessage("No tiene permiso para restablecer la contraseña de un Administrador General.");
                return;
            }

            if (string.IsNullOrWhiteSpace(_view.Password))
            {
                _view.ShowMessage("Ingrese la contraseña temporal en el campo de contraseña");
                return;
            }

            if (_view.Password != _view.ConfirmPassword)
            {
                _view.ShowPasswordMismatch();
                return;
            }

            switch (_passwordChangeService.AdminReset(_view.UserId, _view.Password, ActorId))
            {
                case PasswordChangeResult.Ok:
                    _view.ShowMessage("Contraseña restablecida. El usuario deberá cambiarla al ingresar.");
                    _view.ClearForm();
                    break;
                case PasswordChangeResult.TooShort:
                    _view.ShowMessage($"La contraseña temporal debe tener al menos {PasswordRules.MinLength} caracteres.");
                    break;
            }
        }

        // Clears the brute-force lockout for the selected user so they can try again now.
        public void OnUnlockUser()
        {
            if (_view.SelectedIndex <= 0)
            {
                _view.ShowMessage("Seleccione un usuario para desbloquear");
                return;
            }

            if (!Can("usuarios.gestionar"))
            {
                _view.ShowMessage("No tiene permiso para desbloquear usuarios.");
                return;
            }

            _authService.Unlock(DocumentOf(_view.UserId), ActorId);
            _view.ShowMessage("Cuenta desbloqueada. El usuario puede volver a intentar.");
        }
    }
}
