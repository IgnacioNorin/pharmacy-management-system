using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Business;
using PharmacySystem.Model;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakePermissionService : IPermissionService
    {
        public List<Permission> Catalogue { get; set; } = new List<Permission>();
        public List<TypePerson> Roles { get; set; } = new List<TypePerson>();
        public Dictionary<int, List<int>> PermissionIdsByRole { get; } = new Dictionary<int, List<int>>();
        public Dictionary<string, List<int>> RolesGrantingByCode { get; } = new Dictionary<string, List<int>>();

        public bool SaveRolePermissionsResult { get; set; } = true;
        public int CreateRoleResult { get; set; } = 100;
        public bool RenameRoleResult { get; set; } = true;
        public bool DeleteRoleResult { get; set; } = true;

        public (int RoleId, List<int> Ids)? SavedRolePermissions { get; private set; }
        public string CreatedRoleName { get; private set; }
        public (int RoleId, string Name)? RenamedRole { get; private set; }
        public int? DeletedRoleId { get; private set; }

        // Permission codes returned per role id by GetPermissionsForRole. Empty by default.
        public Dictionary<int, List<string>> PermissionCodesByRole { get; } = new Dictionary<int, List<string>>();

        public List<Permission> GetCatalogue() => Catalogue;

        public IReadOnlyCollection<string> GetPermissionsForRole(int personTypeId) =>
            PermissionCodesByRole.TryGetValue(personTypeId, out var codes) ? new HashSet<string>(codes) : new HashSet<string>();

        public IReadOnlyCollection<int> GetRolesGranting(string permissionCode) =>
            RolesGrantingByCode.TryGetValue(permissionCode, out var ids) ? ids : new List<int>();

        public List<TypePerson> GetRoles() => Roles;

        public List<int> GetPermissionIdsForRole(int personTypeId) =>
            PermissionIdsByRole.TryGetValue(personTypeId, out var ids) ? ids : new List<int>();

        public bool SaveRolePermissions(int personTypeId, IEnumerable<int> permissionIds)
        {
            SavedRolePermissions = (personTypeId, permissionIds.ToList());
            return SaveRolePermissionsResult;
        }

        public int CreateRole(string description)
        {
            CreatedRoleName = description;
            return CreateRoleResult;
        }

        public bool RenameRole(int personTypeId, string description)
        {
            RenamedRole = (personTypeId, description);
            return RenameRoleResult;
        }

        public bool DeleteRole(int personTypeId)
        {
            DeletedRoleId = personTypeId;
            return DeleteRoleResult;
        }
    }
}
