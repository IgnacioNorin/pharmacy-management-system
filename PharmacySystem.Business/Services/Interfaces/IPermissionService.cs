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
    }
}
