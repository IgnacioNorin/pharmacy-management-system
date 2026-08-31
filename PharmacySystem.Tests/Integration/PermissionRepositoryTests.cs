using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using PharmacySystem.Data;
using Xunit;

namespace PharmacySystem.Tests.Integration
{
    // Exercises the permission catalogue and role_permission seed shipped by
    // Database/PharmacyDB.sql. The catalogue tests are read-only; the roles-admin tests below
    // only ever touch custom roles they create and delete themselves (never the three built-ins).
    [Collection("Database")]
    public class PermissionRepositoryTests
    {
        private static readonly IPermissionRepository Repository = new PermissionRepository(SqlConnectionFactory.FromConfiguration());

        private static void DeleteRoleRow(int id) =>
            SqlTestHelper.ExecuteNonQuery("DELETE FROM person_type WHERE id = @id", new SqlParameter("@id", id));

        private static int RoleAdminPermissionId() =>
            SqlTestHelper.ExecuteScalarInt("SELECT id FROM permission WHERE code = 'roles.gestionar'");

        // Puts roles.gestionar back on role 1 if a test removed it. Used in finally blocks so the
        // shipped seed is always intact afterwards, even if an assertion failed mid-test.
        private static void RestoreRoleAdminOnAdministradorGeneral(int roleAdminPermissionId) =>
            SqlTestHelper.ExecuteNonQuery(
                "IF NOT EXISTS (SELECT 1 FROM role_permission WHERE person_type_id = 1 AND permission_id = @p) " +
                "INSERT INTO role_permission (person_type_id, permission_id) VALUES (1, @p)",
                new SqlParameter("@p", roleAdminPermissionId));

        [Fact]
        public void GetAll_ReturnsSeededCatalogueOrderedBySection()
        {
            var all = Repository.GetAll();

            Assert.Equal(33, all.Count);
            Assert.Contains(all, p => p.Code == "ventas.acceso");
            Assert.Contains(all, p => p.Code == "ventas.nota_credito");
            Assert.Contains(all, p => p.Code == "roles.gestionar");
            Assert.Contains(all, p => p.Code == "caja.acceso");
            Assert.Contains(all, p => p.Code == "bitacora.acceso");
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
        public void GetCodesForRole_UnknownRole_ReturnsEmpty()
        {
            Assert.Empty(Repository.GetCodesForRole(999));
        }

        // --- roles admin ---

        [Fact]
        public void GetRoles_ReturnsTheThreeBuiltInsMarkedAsSystem()
        {
            var roles = Repository.GetRoles();

            Assert.All(new[] { 1, 2, 3 }, id => Assert.Contains(roles, r => r.idPersonType == id && r.IsSystem));
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
        public void GetRolesGranting_RolesGestionar_IncludesAdministradorGeneral()
        {
            var holders = Repository.GetRolesGranting("roles.gestionar");

            Assert.Contains(1, holders);            // Administrador General
            Assert.DoesNotContain(2, holders);      // Administrador does not administer roles
        }

        [Fact]
        public void SetRolePermissions_DroppingRolesGestionarWhenAnotherRoleStillHasIt_Succeeds()
        {
            int roleAdminPerm = RoleAdminPermissionId();
            int customRole = Repository.CreateRole("SecondAdmin_" + SqlTestHelper.NewTag());
            try
            {
                Assert.True(Repository.SetRolePermissions(customRole, new[] { roleAdminPerm }));
                // Role 1 keeps roles.gestionar in the seed, so the custom role is free to drop it.
                Assert.True(Repository.SetRolePermissions(customRole, new int[0]));
                Assert.Empty(Repository.GetPermissionIdsForRole(customRole));
            }
            finally
            {
                DeleteRoleRow(customRole);
            }
        }

        [Fact]
        public void SetRolePermissions_DroppingRolesGestionarFromTheLastRoleThatHasIt_IsRefused()
        {
            int roleAdminPerm = RoleAdminPermissionId();
            int customRole = Repository.CreateRole("LastAdmin_" + SqlTestHelper.NewTag());
            // Give the custom role roles.gestionar, then take it off the seed's role 1, so the
            // custom role is momentarily the only holder. Role 1 is restored in the finally block.
            Repository.SetRolePermissions(customRole, new[] { roleAdminPerm });
            SqlTestHelper.ExecuteNonQuery(
                "DELETE FROM role_permission WHERE person_type_id = 1 AND permission_id = @p",
                new SqlParameter("@p", roleAdminPerm));
            try
            {
                Assert.False(Repository.SetRolePermissions(customRole, new int[0]));
                Assert.Contains(roleAdminPerm, Repository.GetPermissionIdsForRole(customRole));
            }
            finally
            {
                RestoreRoleAdminOnAdministradorGeneral(roleAdminPerm);
                DeleteRoleRow(customRole);
            }
        }

        [Fact]
        public void DeleteRole_LastRoleThatHasRolesGestionar_IsRefused()
        {
            int roleAdminPerm = RoleAdminPermissionId();
            int customRole = Repository.CreateRole("LastAdminDel_" + SqlTestHelper.NewTag());
            Repository.SetRolePermissions(customRole, new[] { roleAdminPerm });
            SqlTestHelper.ExecuteNonQuery(
                "DELETE FROM role_permission WHERE person_type_id = 1 AND permission_id = @p",
                new SqlParameter("@p", roleAdminPerm));
            try
            {
                Assert.False(Repository.DeleteRole(customRole));
                Assert.Contains(Repository.GetRoles(), r => r.idPersonType == customRole);
            }
            finally
            {
                RestoreRoleAdminOnAdministradorGeneral(roleAdminPerm);
                DeleteRoleRow(customRole);
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
