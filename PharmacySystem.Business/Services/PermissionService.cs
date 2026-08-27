using System;
using System.Collections.Generic;
using PharmacySystem.Data;
using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    public class PermissionService : IPermissionService
    {
        private readonly IPermissionRepository _repository;

        public PermissionService(IPermissionRepository repository)
        {
            _repository = repository;
        }

        public List<Permission> GetCatalogue() => _repository.GetAll();

        public IReadOnlyCollection<string> GetPermissionsForRole(int personTypeId) =>
            new HashSet<string>(_repository.GetCodesForRole(personTypeId), StringComparer.OrdinalIgnoreCase);

        public List<TypePerson> GetRoles() => _repository.GetRoles();

        public List<int> GetPermissionIdsForRole(int personTypeId) => _repository.GetPermissionIdsForRole(personTypeId);

        public bool SaveRolePermissions(int personTypeId, IEnumerable<int> permissionIds) =>
            _repository.SetRolePermissions(personTypeId, permissionIds);

        public int CreateRole(string description) => _repository.CreateRole(description?.Trim());

        public bool RenameRole(int personTypeId, string description) => _repository.RenameRole(personTypeId, description?.Trim());

        public bool DeleteRole(int personTypeId) => _repository.DeleteRole(personTypeId);
    }
}
