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
        private readonly ISecurityAudit _audit;

        private List<Person> _users = new List<Person>();
        private ISet<string> _lockedDocuments = new HashSet<string>();

        public UserPresenter(IUserView view, IPersonService service, CurrentUser currentUser,
            IPermissionService permissionService, IPasswordChangeService passwordChangeService,
            IAuthenticationService authService, ISecurityAudit audit)
        {
            _view = view;
            _service = service;
            _currentUser = currentUser;
            _permissionService = permissionService;
            _passwordChangeService = passwordChangeService;
            _authService = authService;
            _audit = audit;
        }

        private string RoleNameOf(int userId) =>
            _users.FirstOrDefault(u => u.idPerson == userId)?.oPersonType?.description ?? "";

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

            LoadUserList();
        }

        // Reloads the grid from the service, refreshing the cached lock set so the Estado column
        // is current. Called on load and after any action that can change a user's state.
        private void LoadUserList()
        {
            _users = _service.List();
            _lockedDocuments = _authService.GetLockedDocuments();

            _view.LoadUsers(_users.Select(p => new UserRow
            {
                Id = p.idPerson,
                Document = p.document,
                Name = p.name,
                RoleText = p.oPersonType.description,
                StatusText = StatusFor(p.Estado, p.document)
            }));
        }

        private string StatusFor(bool active, string document)
        {
            if (!active) return "Inactivo";
            return document != null && _lockedDocuments.Contains(document) ? "Bloqueado" : "Activo";
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
                _audit.Record(ActorId, "user.create", "person", newId,
                    $"'{person.name}' (doc {person.document}), rol {_view.RoleText}");
                _view.AddRow(new UserRow
                {
                    Id = person.idPerson,
                    Document = person.document,
                    Name = person.name,
                    RoleText = _view.RoleText,
                    StatusText = "Activo" // a brand-new user is active and has no failed attempts
                });
                _view.ClearForm();
            }
            else
            {
                string previousRole = RoleNameOf(person.idPerson);

                if (!_service.Update(person))
                {
                    return;
                }

                string roleChange = previousRole != _view.RoleText && previousRole.Length > 0
                    ? $"; rol {previousRole} -> {_view.RoleText}"
                    : "";
                _audit.Record(ActorId, "user.update", "person", person.idPerson,
                    $"'{person.name}' (doc {person.document}){roleChange}");

                // An edit does not change status or lock state - keep whatever the row had.
                bool wasActive = _users.FirstOrDefault(u => u.idPerson == person.idPerson)?.Estado ?? true;
                _view.ReplaceRow(_view.SelectedIndex - 1, new UserRow
                {
                    Id = person.idPerson,
                    Document = person.document,
                    Name = person.name,
                    RoleText = _view.RoleText,
                    StatusText = StatusFor(wasActive, person.document)
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

            string name = _users.FirstOrDefault(u => u.idPerson == _view.UserId)?.name ?? "";
            string doc = DocumentOf(_view.UserId);

            if (!_service.Delete(_view.UserId))
            {
                _view.ShowMessage("No se pudo eliminar el registro\nRevise los datos");
                return;
            }

            _audit.Record(ActorId, "user.delete", "person", _view.UserId, $"'{name}' (doc {doc})");
            _view.RemoveRow(_view.SelectedIndex - 1);
            _view.ClearForm();
        }

        // Admin reset: the system generates the temporary password (the admin never picks it
        // and never needs the current one). It is shown once so the admin can hand it over;
        // the user is forced to change it on next login. Also clears any lockout.
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

            string tempPassword = _passwordChangeService.AdminReset(_view.UserId, ActorId);
            _view.ShowTemporaryPassword(tempPassword);
            LoadUserList(); // a reset also clears the lockout - refresh the Estado column
        }

        // Suspends or reactivates the selected account (flips person.status). No password
        // involved; also the way to undo an accidental soft-delete.
        public void OnSuspendUser()
        {
            if (_view.SelectedIndex <= 0)
            {
                _view.ShowMessage("Seleccione un usuario para suspender o reactivar");
                return;
            }

            if (!Can("usuarios.gestionar"))
            {
                _view.ShowMessage("No tiene permiso para suspender usuarios.");
                return;
            }

            Person target = _users.FirstOrDefault(u => u.idPerson == _view.UserId);
            if (target == null)
            {
                return;
            }

            bool targetIsAdminGeneral = (target.oPersonType?.idPersonType ?? 0) == AdminGeneralRoleId;
            if (targetIsAdminGeneral && !CurrentIsAdminGeneral)
            {
                _view.ShowMessage("No tiene permiso para suspender un Administrador General.");
                return;
            }
            if (targetIsAdminGeneral && target.Estado && ActiveAdminGeneralCount <= 1)
            {
                _view.ShowMessage("No se puede suspender al último Administrador General activo.");
                return;
            }

            bool newActive = !target.Estado;
            _service.SetActive(_view.UserId, newActive);
            _authService.RecordSuspension(DocumentOf(_view.UserId), suspended: !newActive, actorId: ActorId);
            _view.ShowMessage(newActive
                ? "Cuenta reactivada."
                : "Cuenta suspendida. El usuario no podrá iniciar sesión.");
            LoadUserList();
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
            LoadUserList(); // the row should stop showing "Bloqueado"
        }
    }
}
