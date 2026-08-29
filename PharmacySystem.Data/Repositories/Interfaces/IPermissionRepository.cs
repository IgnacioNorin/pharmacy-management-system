using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Data
{
    public interface IPermissionRepository
    {
        // The whole catalogue, ordered by section then code (for the roles admin screen).
        List<Permission> GetAll();

        // The permission codes granted by one role.
        List<string> GetCodesForRole(int personTypeId);

        // Ids of the roles that currently grant the given permission code.
        List<int> GetRolesGranting(string permissionCode);

        // --- roles admin (frmRoles) ---

        List<TypePerson> GetRoles();

        List<int> GetPermissionIdsForRole(int personTypeId);

        // Replaces the role's whole permission set in one transaction.
        bool SetRolePermissions(int personTypeId, IEnumerable<int> permissionIds);

        // New custom role. Returns the new id, or 0 if the description already exists.
        int CreateRole(string description);

        // Rename a custom role. Returns false for a system role or a taken name.
        bool RenameRole(int personTypeId, string description);

        // Delete a custom role. Returns false for a system role or one still assigned to a user.
        bool DeleteRole(int personTypeId);
    }
}
