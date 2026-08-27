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
    }
}
