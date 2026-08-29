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
        public List<PermissionNode> ShownPermissionRoots { get; private set; }
        // The whole tree flattened, for tests that just look one permission up by Id.
        public List<PermissionNode> ShownPermissions { get; private set; }
        public bool? PermissionsEditable { get; private set; }
        public (bool CanRename, bool CanDelete)? RoleActionsEnabled { get; private set; }
        public int ClearRoleNameInputCount { get; private set; }
        public List<string> ShownMessages { get; } = new List<string>();

        public bool ConfirmDeleteRole() => ConfirmDeleteRoleResult;

        public void LoadRoles(IEnumerable<RoleRow> roles) => LoadedRoles = roles.ToList();

        public void ShowRolePermissions(IEnumerable<PermissionNode> permissionTree)
        {
            ShownPermissionRoots = permissionTree.ToList();
            ShownPermissions = Flatten(ShownPermissionRoots).ToList();
        }

        private static IEnumerable<PermissionNode> Flatten(IEnumerable<PermissionNode> nodes)
        {
            foreach (PermissionNode node in nodes)
            {
                yield return node;
                foreach (PermissionNode child in Flatten(node.Children))
                {
                    yield return child;
                }
            }
        }
        public void SetPermissionsEditable(bool editable) => PermissionsEditable = editable;
        public void SetRoleActionsEnabled(bool canRename, bool canDelete) => RoleActionsEnabled = (canRename, canDelete);
        public void ClearRoleNameInput() => ClearRoleNameInputCount++;
        public void ShowMessage(string message) => ShownMessages.Add(message);
    }
}
