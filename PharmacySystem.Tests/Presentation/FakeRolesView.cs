using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Presentation;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakeRolesView : IRolesView
    {
        public int? SelectedRoleId { get; set; }
        public string RoleNameInput { get; set; }
        public IReadOnlyCollection<int> CheckedPermissionIds { get; set; } = new List<int>();
        public bool ConfirmDeleteRoleResult { get; set; } = true;

        public List<RoleRow> LoadedRoles { get; private set; }
        public List<PermissionCheckItem> ShownPermissions { get; private set; }
        public bool? PermissionsEditable { get; private set; }
        public (bool CanRename, bool CanDelete)? RoleActionsEnabled { get; private set; }
        public int ClearRoleNameInputCount { get; private set; }
        public List<string> ShownMessages { get; } = new List<string>();

        public bool ConfirmDeleteRole() => ConfirmDeleteRoleResult;

        public void LoadRoles(IEnumerable<RoleRow> roles) => LoadedRoles = roles.ToList();
        public void ShowRolePermissions(IEnumerable<PermissionCheckItem> permissions) => ShownPermissions = permissions.ToList();
        public void SetPermissionsEditable(bool editable) => PermissionsEditable = editable;
        public void SetRoleActionsEnabled(bool canRename, bool canDelete) => RoleActionsEnabled = (canRename, canDelete);
        public void ClearRoleNameInput() => ClearRoleNameInputCount++;
        public void ShowMessage(string message) => ShownMessages.Add(message);
    }
}
