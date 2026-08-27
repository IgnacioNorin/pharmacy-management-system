using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using PharmacySystem.Data;
using Xunit;

namespace PharmacySystem.Tests.Integration
{
    // Exercises the permission catalogue and role_permission seed shipped by
    // Database/PharmacyDB.sql. The catalogue tests are read-only; the roles-admin tests below
    // only ever touch custom roles they create and delete themselves (never the four built-ins).
    [Collection("Database")]
    public class PermissionRepositoryTests
    {
        private static readonly IPermissionRepository Repository = new PermissionRepository(SqlConnectionFactory.FromConfiguration());

        private static void DeleteRoleRow(int id) =>
            SqlTestHelper.ExecuteNonQuery("DELETE FROM person_type WHERE id = @id", new SqlParameter("@id", id));

        [Fact]
        public void GetAll_ReturnsSeededCatalogueOrderedBySection()
        {
            var all = Repository.GetAll();

            Assert.Equal(30, all.Count);
            Assert.Contains(all, p => p.Code == "ventas.acceso");
            Assert.Contains(all, p => p.Code == "roles.gestionar");
            Assert.Contains(all, p => p.Code == "reportes.acceso");
            Assert.Contains(all, p => p.Code == "reportes.ventas");
            Assert.Contains(all, p => p.Code == "reportes.alertas.exportar");
            Assert.All(all, p => Assert.False(string.IsNullOrWhiteSpace(p.Section)));
            Assert.Equal(all.OrderBy(p => p.Section).ThenBy(p => p.Code).Select(p => p.Code),
                         all.Select(p => p.Code));
        }

        [Fact]
        public void GetAll_ParentCode_GivesEachSectionArootAndNestsTheChildren()
        {
            var all = Repository.GetAll();

            // Section roots have no parent and their code is "<section>.acceso".
            Assert.All(all.Where(p => p.ParentCode == null), p => Assert.EndsWith(".acceso", p.Code));
            Assert.Null(all.Single(p => p.Code == "reportes.acceso").ParentCode);

            Assert.Equal("productos.acceso", all.Single(p => p.Code == "productos.eliminar").ParentCode);
            Assert.Equal("reportes.acceso", all.Single(p => p.Code == "reportes.ventas").ParentCode);
            Assert.Equal("reportes.ventas", all.Single(p => p.Code == "reportes.ventas.exportar").ParentCode);

            // Every non-root parent_code points at a real permission.
            var codes = new HashSet<string>(all.Select(p => p.Code));
            Assert.All(all.Where(p => p.ParentCode != null), p => Assert.Contains(p.ParentCode, codes));
        }

        [Fact]
        public void GetCodesForRole_AdministradorGeneral_ReturnsEntireCatalogue()
        {
            Assert.Equal(Repository.GetAll().Count, Repository.GetCodesForRole(1).Count);
        }

        [Fact]
        public void GetCodesForRole_Administrador_ExcludesTiendaSectionAndRolesGestionar()
        {
            var codes = Repository.GetCodesForRole(2);

            Assert.NotEmpty(codes);
            Assert.DoesNotContain(codes, c => c.StartsWith("tienda."));
            // Only Administrador General administers roles, otherwise a regular Administrador
            // could re-permission its own role past the Tienda boundary.
            Assert.DoesNotContain("roles.gestionar", codes);
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

        // --- roles admin ---

        [Fact]
        public void GetRoles_ReturnsTheFourBuiltInsMarkedAsSystem()
        {
            var roles = Repository.GetRoles();

            Assert.All(new[] { 1, 2, 3, 4 }, id => Assert.Contains(roles, r => r.idPersonType == id && r.IsSystem));
        }

        [Fact]
        public void GetPermissionIdsForRole_Empleado_ReturnsSixIds()
        {
            Assert.Equal(6, Repository.GetPermissionIdsForRole(3).Count);
        }

        [Fact]
        public void SetRolePermissions_ReplacesTheWholeSet()
        {
            int roleId = Repository.CreateRole("SetPerms_" + SqlTestHelper.NewTag());
            try
            {
                var ids = Repository.GetAll().Select(p => p.Id).Take(3).ToList();

                Assert.True(Repository.SetRolePermissions(roleId, ids));
                Assert.Equal(ids.OrderBy(x => x), Repository.GetPermissionIdsForRole(roleId).OrderBy(x => x));

                Assert.True(Repository.SetRolePermissions(roleId, new int[0]));
                Assert.Empty(Repository.GetPermissionIdsForRole(roleId));
            }
            finally
            {
                DeleteRoleRow(roleId);
            }
        }

        [Fact]
        public void CreateRole_NewName_ReturnsIdAtLeast100_DuplicateReturnsZero()
        {
            string name = "NewRole_" + SqlTestHelper.NewTag();
            int id = Repository.CreateRole(name);
            try
            {
                Assert.True(id >= 100);
                Assert.Equal(0, Repository.CreateRole(name));
            }
            finally
            {
                DeleteRoleRow(id);
            }
        }

        [Fact]
        public void RenameRole_SystemRoleIsRejected_CustomRoleSucceeds()
        {
            Assert.False(Repository.RenameRole(1, "No permitido"));

            int id = Repository.CreateRole("Rename_" + SqlTestHelper.NewTag());
            try
            {
                string newName = "Renamed_" + SqlTestHelper.NewTag();
                Assert.True(Repository.RenameRole(id, newName));
                Assert.Contains(Repository.GetRoles(), r => r.idPersonType == id && r.description == newName);
            }
            finally
            {
                DeleteRoleRow(id);
            }
        }

        [Fact]
        public void DeleteRole_SystemRoleIsRejected()
        {
            Assert.False(Repository.DeleteRole(1));
            Assert.Contains(Repository.GetRoles(), r => r.idPersonType == 1);
        }

        [Fact]
        public void DeleteRole_CustomRoleWithNoUsers_Succeeds()
        {
            int id = Repository.CreateRole("Delete_" + SqlTestHelper.NewTag());

            Assert.True(Repository.DeleteRole(id));
            Assert.DoesNotContain(Repository.GetRoles(), r => r.idPersonType == id);
        }

        [Fact]
        public void DeleteRole_CustomRoleWithUserAssigned_IsRejected()
        {
            int roleId = Repository.CreateRole("InUse_" + SqlTestHelper.NewTag());
            string document = SqlTestHelper.NewTag();
            SqlTestHelper.ExecuteNonQuery(
                "INSERT INTO person(document_number, name, person_type_id, status) VALUES (@doc, 'Role user', @role, 1)",
                new SqlParameter("@doc", document), new SqlParameter("@role", roleId));

            try
            {
                Assert.False(Repository.DeleteRole(roleId));
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM person WHERE document_number = @doc", new SqlParameter("@doc", document));
                DeleteRoleRow(roleId);
            }
        }
    }
}
