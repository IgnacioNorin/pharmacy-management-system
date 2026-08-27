using System.Linq;
using PharmacySystem.Data;
using Xunit;

namespace PharmacySystem.Tests.Integration
{
    // Read-only: exercises the permission catalogue and role_permission seed shipped by
    // Database/PharmacyDB.sql. No rows are created or deleted.
    [Collection("Database")]
    public class PermissionRepositoryTests
    {
        private static readonly IPermissionRepository Repository = new PermissionRepository(SqlConnectionFactory.FromConfiguration());

        [Fact]
        public void GetAll_ReturnsSeededCatalogueOrderedBySection()
        {
            var all = Repository.GetAll();

            Assert.Equal(23, all.Count);
            Assert.Contains(all, p => p.Code == "ventas.acceso");
            Assert.Contains(all, p => p.Code == "roles.gestionar");
            Assert.All(all, p => Assert.False(string.IsNullOrWhiteSpace(p.Section)));
            Assert.Equal(all.OrderBy(p => p.Section).ThenBy(p => p.Code).Select(p => p.Code),
                         all.Select(p => p.Code));
        }

        [Fact]
        public void GetCodesForRole_AdministradorGeneral_ReturnsEntireCatalogue()
        {
            Assert.Equal(Repository.GetAll().Count, Repository.GetCodesForRole(1).Count);
        }

        [Fact]
        public void GetCodesForRole_Administrador_ExcludesTiendaSection()
        {
            var codes = Repository.GetCodesForRole(2);

            Assert.NotEmpty(codes);
            Assert.DoesNotContain(codes, c => c.StartsWith("tienda."));
        }

        [Fact]
        public void GetCodesForRole_Empleado_ReturnsExactlyTheSeededSix()
        {
            var codes = Repository.GetCodesForRole(3).OrderBy(c => c);

            Assert.Equal(
                new[] { "alertas.acceso", "alertas.reconocer", "alertas.silenciar",
                        "clientes.acceso", "clientes.gestionar", "ventas.acceso" },
                codes);
        }

        [Fact]
        public void GetCodesForRole_Cliente_ReturnsEmpty()
        {
            Assert.Empty(Repository.GetCodesForRole(4));
        }
    }
}
