using System.Collections.Generic;
using PharmacySystem.Data;
using PharmacySystem.Model;

namespace PharmacySystem.Tests.Business
{
    internal class FakePermissionRepository : IPermissionRepository
    {
        public List<Permission> AllResult { get; set; } = new List<Permission>();
        public List<string> CodesForRoleResult { get; set; } = new List<string>();
        public int? RequestedRoleId { get; private set; }

        public List<Permission> GetAll() => AllResult;

        public List<string> GetCodesForRole(int personTypeId)
        {
            RequestedRoleId = personTypeId;
            return CodesForRoleResult;
        }
    }
}
