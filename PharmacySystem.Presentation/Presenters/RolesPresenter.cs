using System;
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
        private const string RoleAdminCode = "roles.gestionar";

        private readonly IRolesView _view;
        private readonly IPermissionService _service;
        private readonly CurrentUser _currentUser;
        private readonly ISecurityAudit _audit;

        private List<Permission> _catalogue = new List<Permission>();
        private List<TypePerson> _roles = new List<TypePerson>();

        public RolesPresenter(IRolesView view, IPermissionService service, CurrentUser currentUser, ISecurityAudit audit)
        {
            _view = view;
            _service = service;
            _currentUser = currentUser;
            _audit = audit;
        }

        private int ActorId => _currentUser?.PersonId ?? 0;

        private string RoleName(int roleId) =>
            _roles.FirstOrDefault(r => r.idPersonType == roleId)?.description ?? roleId.ToString();

        private string PermissionCode(int permissionId) =>
            _catalogue.FirstOrDefault(p => p.Id == permissionId)?.Code ?? permissionId.ToString();

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
            _view.ShowRolePermissions(BuildTree(null, granted));
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

            // Store a consistent set: a checked permission always drags its ancestors in, so a
            // role can never end up with "ver ventas" but not "abrir reportes".
            var toSave = ExpandAncestors(_view.CheckedPermissionIds);

            if (WouldOrphanRoleAdmin(roleId.Value, toSave))
            {
                _view.ShowMessage("No puede quitar la administracion de roles del unico rol que la tiene. " +
                                  "Asigne ese permiso a otro rol antes de quitarlo de este.");
                return;
            }

            var before = new HashSet<int>(_service.GetPermissionIdsForRole(roleId.Value));

            if (_service.SaveRolePermissions(roleId.Value, toSave))
            {
                _audit.Record(ActorId, "role.permissions", "person_type", roleId.Value, PermissionDiff(roleId.Value, before, toSave));
                _view.ShowMessage("Permisos guardados.");
            }
            else
            {
                _view.ShowMessage("No se pudieron guardar los permisos.");
            }
        }

        private string PermissionDiff(int roleId, HashSet<int> before, IEnumerable<int> after)
        {
            var afterSet = new HashSet<int>(after);
            var added = afterSet.Where(id => !before.Contains(id)).Select(id => "+" + PermissionCode(id));
            var removed = before.Where(id => !afterSet.Contains(id)).Select(id => "-" + PermissionCode(id));
            string changes = string.Join(", ", added.Concat(removed));
            return $"rol '{RoleName(roleId)}': " + (changes.Length == 0 ? "sin cambios" : changes);
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

            int newId = _service.CreateRole(name);
            if (newId == 0)
            {
                _view.ShowMessage("Ya existe un rol con ese nombre.");
                return;
            }

            _audit.Record(ActorId, "role.create", "person_type", newId, $"rol '{name}'");
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

            string previous = RoleName(roleId.Value);
            if (_service.RenameRole(roleId.Value, name))
            {
                _audit.Record(ActorId, "role.rename", "person_type", roleId.Value, $"'{previous}' -> '{name}'");
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

            string deleted = RoleName(roleId.Value);
            if (_service.DeleteRole(roleId.Value))
            {
                _audit.Record(ActorId, "role.delete", "person_type", roleId.Value, $"rol '{deleted}'");
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
            _view.ShowRolePermissions(Enumerable.Empty<PermissionNode>());
            _view.SetPermissionsEditable(false);
            _view.SetRoleActionsEnabled(false, false);
        }

        private bool IsSystem(int roleId) => _roles.Any(r => r.idPersonType == roleId && r.IsSystem);

        // True if saving this set would strip "roles.gestionar" from the last role that grants it,
        // which would leave nobody able to reopen this screen. The stored procedure enforces the
        // same rule; this is here so the user gets a precise message instead of a generic failure.
        private bool WouldOrphanRoleAdmin(int roleId, IReadOnlyCollection<int> toSave)
        {
            Permission? roleAdmin = _catalogue.FirstOrDefault(
                p => string.Equals(p.Code, RoleAdminCode, StringComparison.OrdinalIgnoreCase));
            if (roleAdmin == null || toSave.Contains(roleAdmin.Id))
            {
                return false;
            }

            var holders = _service.GetRolesGranting(RoleAdminCode);
            return holders.Contains(roleId) && holders.Count <= 1;
        }

        // Builds the permission forest under parentCode (null = section roots), preserving the
        // catalogue order the repository already returns.
        private List<PermissionNode> BuildTree(string? parentCode, HashSet<int> granted)
        {
            List<PermissionNode> nodes = new List<PermissionNode>();
            foreach (Permission p in _catalogue.Where(p => IsChildOf(p, parentCode)))
            {
                PermissionNode node = new PermissionNode
                {
                    Id = p.Id,
                    Description = p.Description,
                    Checked = granted.Contains(p.Id)
                };
                node.Children.AddRange(BuildTree(p.Code, granted));
                nodes.Add(node);
            }
            return nodes;
        }

        private static bool IsChildOf(Permission p, string? parentCode) =>
            string.IsNullOrEmpty(parentCode)
                ? string.IsNullOrEmpty(p.ParentCode)
                : string.Equals(p.ParentCode, parentCode, StringComparison.OrdinalIgnoreCase);

        // Every checked id plus the id of each of its ancestors in the catalogue tree.
        private IReadOnlyCollection<int> ExpandAncestors(IEnumerable<int> checkedIds)
        {
            Dictionary<int, Permission> byId = _catalogue.ToDictionary(p => p.Id);
            Dictionary<string, Permission> byCode =
                _catalogue.GroupBy(p => p.Code, StringComparer.OrdinalIgnoreCase)
                          .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            HashSet<int> result = new HashSet<int>(checkedIds ?? Enumerable.Empty<int>());
            foreach (int id in result.ToList())
            {
                if (!byId.TryGetValue(id, out Permission? current))
                {
                    continue;
                }
                string? parent = current.ParentCode;
                while (!string.IsNullOrEmpty(parent) && byCode.TryGetValue(parent, out Permission? ancestor))
                {
                    result.Add(ancestor.Id);
                    parent = ancestor.ParentCode;
                }
            }
            return result.OrderBy(x => x).ToList();
        }
    }
}
