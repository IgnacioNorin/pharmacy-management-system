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
    }
}
