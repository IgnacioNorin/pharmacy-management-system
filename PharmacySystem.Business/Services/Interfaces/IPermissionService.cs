using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    public interface IPermissionService
    {
        // The full catalogue, for the roles admin screen.
        List<Permission> GetCatalogue();

        // The effective permission codes for a role, as a case-insensitive set.
        IReadOnlyCollection<string> GetPermissionsForRole(int personTypeId);

        // Ids of the roles that currently grant the given permission code.
        IReadOnlyCollection<int> GetRolesGranting(string permissionCode);

        // --- roles admin (frmRoles) ---

        List<TypePerson> GetRoles();

        List<int> GetPermissionIdsForRole(int personTypeId);

        bool SaveRolePermissions(int personTypeId, IEnumerable<int> permissionIds);

        int CreateRole(string description);

        bool RenameRole(int personTypeId, string description);

        bool DeleteRole(int personTypeId);
    }
}
