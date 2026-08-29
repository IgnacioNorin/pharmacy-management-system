using System.Collections.Generic;
using PharmacySystem.Data;
using PharmacySystem.Model;

namespace PharmacySystem.Tests.Business
{
    internal class FakePermissionRepository : IPermissionRepository
    {
        public List<Permission> AllResult { get; set; } = new List<Permission>();
        public List<string> CodesForRoleResult { get; set; } = new List<string>();
        public List<int> RolesGrantingResult { get; set; } = new List<int>();
        public List<TypePerson> RolesResult { get; set; } = new List<TypePerson>();
        public List<int> PermissionIdsForRoleResult { get; set; } = new List<int>();
        public bool SetRolePermissionsResult { get; set; } = true;
        public int CreateRoleResult { get; set; } = 100;
        public bool RenameRoleResult { get; set; } = true;
        public bool DeleteRoleResult { get; set; } = true;

        public int? RequestedRoleId { get; private set; }

        public List<Permission> GetAll() => AllResult;

        public List<string> GetCodesForRole(int personTypeId)
        {
            RequestedRoleId = personTypeId;
            return CodesForRoleResult;
        }

        public List<int> GetRolesGranting(string permissionCode) => RolesGrantingResult;

        public List<TypePerson> GetRoles() => RolesResult;

        public List<int> GetPermissionIdsForRole(int personTypeId) => PermissionIdsForRoleResult;

        public bool SetRolePermissions(int personTypeId, IEnumerable<int> permissionIds) => SetRolePermissionsResult;

        public int CreateRole(string description) => CreateRoleResult;

        public bool RenameRole(int personTypeId, string description) => RenameRoleResult;

        public bool DeleteRole(int personTypeId) => DeleteRoleResult;
    }
}
