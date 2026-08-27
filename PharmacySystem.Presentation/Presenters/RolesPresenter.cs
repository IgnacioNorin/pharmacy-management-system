using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Business;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // Roles admin (frmRoles): edit each role's permission set, and add / rename / delete custom
    // roles. The four built-in roles (IsSystem) can be re-permissioned but not renamed or deleted.
    public class RolesPresenter
    {
        private readonly IRolesView _view;
        private readonly IPermissionService _service;
        private readonly CurrentUser _currentUser;

        private List<Permission> _catalogue = new List<Permission>();
        private List<TypePerson> _roles = new List<TypePerson>();

        public RolesPresenter(IRolesView view, IPermissionService service, CurrentUser currentUser)
        {
            _view = view;
            _service = service;
            _currentUser = currentUser;
        }

        // Every mutating action re-checks the permission, not just the sidebar button that opened
        // this screen (same rule as the fase 3 presenters).
        private bool DeniedRoleAdmin()
        {
            if (_currentUser?.Can("roles.gestionar") ?? false)
            {
                return false;
            }
            _view.ShowMessage("No tiene permiso para administrar roles.");
            return true;
        }

        public void OnLoad()
        {
            _catalogue = _service.GetCatalogue();
            RefreshRoles();
            ClearPermissionPanel();
        }

        public void OnRoleSelected()
        {
            int? roleId = _view.SelectedRoleId;
            if (roleId == null)
            {
                ClearPermissionPanel();
                return;
            }

            var granted = new HashSet<int>(_service.GetPermissionIdsForRole(roleId.Value));
            _view.ShowRolePermissions(_catalogue.Select(p => new PermissionCheckItem
            {
                Id = p.Id,
                Section = p.Section,
                Description = p.Description,
                Checked = granted.Contains(p.Id)
            }));
            _view.SetPermissionsEditable(true);

            bool isSystem = IsSystem(roleId.Value);
            _view.SetRoleActionsEnabled(!isSystem, !isSystem);
        }

        public void OnSavePermissions()
        {
            if (DeniedRoleAdmin())
            {
                return;
            }

            int? roleId = _view.SelectedRoleId;
            if (roleId == null)
            {
                _view.ShowMessage("Seleccione un rol.");
                return;
            }

            _view.ShowMessage(_service.SaveRolePermissions(roleId.Value, _view.CheckedPermissionIds)
                ? "Permisos guardados."
                : "No se pudieron guardar los permisos.");
        }

        public void OnCreateRole()
        {
            if (DeniedRoleAdmin())
            {
                return;
            }

            string name = (_view.RoleNameInput ?? "").Trim();
            if (name.Length == 0)
            {
                _view.ShowMessage("Ingrese un nombre para el rol.");
                return;
            }

            if (_service.CreateRole(name) == 0)
            {
                _view.ShowMessage("Ya existe un rol con ese nombre.");
                return;
            }

            _view.ClearRoleNameInput();
            RefreshRoles();
            ClearPermissionPanel();
        }

        public void OnRenameRole()
        {
            if (DeniedRoleAdmin())
            {
                return;
            }

            int? roleId = _view.SelectedRoleId;
            if (roleId == null)
            {
                _view.ShowMessage("Seleccione un rol.");
                return;
            }
            if (IsSystem(roleId.Value))
            {
                _view.ShowMessage("Los roles del sistema no se pueden renombrar.");
                return;
            }

            string name = (_view.RoleNameInput ?? "").Trim();
            if (name.Length == 0)
            {
                _view.ShowMessage("Ingrese el nuevo nombre.");
                return;
            }

            if (_service.RenameRole(roleId.Value, name))
            {
                _view.ClearRoleNameInput();
                RefreshRoles();
                ClearPermissionPanel();
            }
            else
            {
                _view.ShowMessage("No se pudo renombrar: ya existe un rol con ese nombre.");
            }
        }

        public void OnDeleteRole()
        {
            if (DeniedRoleAdmin())
            {
                return;
            }

            int? roleId = _view.SelectedRoleId;
            if (roleId == null)
            {
                _view.ShowMessage("Seleccione un rol.");
                return;
            }
            if (IsSystem(roleId.Value))
            {
                _view.ShowMessage("Los roles del sistema no se pueden eliminar.");
                return;
            }
            if (!_view.ConfirmDeleteRole())
            {
                return;
            }

            if (_service.DeleteRole(roleId.Value))
            {
                RefreshRoles();
                ClearPermissionPanel();
            }
            else
            {
                _view.ShowMessage("No se pudo eliminar: el rol tiene usuarios asignados.");
            }
        }

        private void RefreshRoles()
        {
            _roles = _service.GetRoles();
            _view.LoadRoles(_roles.Select(r => new RoleRow
            {
                Id = r.idPersonType,
                Name = r.description,
                IsSystem = r.IsSystem
            }));
        }

        private void ClearPermissionPanel()
        {
            _view.ShowRolePermissions(Enumerable.Empty<PermissionCheckItem>());
            _view.SetPermissionsEditable(false);
            _view.SetRoleActionsEnabled(false, false);
        }

        private bool IsSystem(int roleId) => _roles.Any(r => r.idPersonType == roleId && r.IsSystem);
    }
}
