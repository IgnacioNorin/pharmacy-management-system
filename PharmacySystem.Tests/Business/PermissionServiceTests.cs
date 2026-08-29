using System.Collections.Generic;
using PharmacySystem.Business;
using PharmacySystem.Model;
using Xunit;

namespace PharmacySystem.Tests.Business
{
    public class PermissionServiceTests
    {
        [Fact]
        public void GetPermissionsForRole_ReturnsCaseInsensitiveSet()
        {
            var repository = new FakePermissionRepository
            {
                CodesForRoleResult = new List<string> { "Ventas.Acceso", "productos.eliminar" }
            };
            var service = new PermissionService(repository);

            var perms = service.GetPermissionsForRole(2);

            Assert.Equal(2, repository.RequestedRoleId);
            Assert.Contains("ventas.acceso", perms);
            Assert.Contains("VENTAS.ACCESO", perms);
            Assert.Contains("productos.eliminar", perms);
        }

        [Fact]
        public void GetCatalogue_DelegatesToRepository()
        {
            var catalogue = new List<Permission>
            {
                new Permission { Id = 1, Code = "ventas.acceso", Section = "ventas", Description = "x" }
            };
            var service = new PermissionService(new FakePermissionRepository { AllResult = catalogue });

            Assert.Same(catalogue, service.GetCatalogue());
        }
    }
}
