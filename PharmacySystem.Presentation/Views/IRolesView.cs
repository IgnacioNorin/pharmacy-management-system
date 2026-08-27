using System.Collections.Generic;

namespace PharmacySystem.Presentation
{
    public interface IRolesView
    {
        int? SelectedRoleId { get; }
        string RoleNameInput { get; }
        IReadOnlyCollection<int> CheckedPermissionIds { get; }

        bool ConfirmDeleteRole();

        void LoadRoles(IEnumerable<RoleRow> roles);
        void ShowRolePermissions(IEnumerable<PermissionCheckItem> permissions);
        void SetPermissionsEditable(bool editable);
        void SetRoleActionsEnabled(bool canRename, bool canDelete);
        void ClearRoleNameInput();
        void ShowMessage(string message);
    }
}
