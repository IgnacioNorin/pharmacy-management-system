using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using PharmacySystem.Business;
using PharmacySystem.Data;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using PharmacySystem.Tests.Presentation;
using Xunit;

namespace PharmacySystem.Tests.Integration
{
    // End-to-end permission check: creates a real person, logs in through LoginPresenter, resolves
    // the permission set from the shipped role_permission seed, and then drives every section of the
    // app the way MainForm would. It proves the seeded permission codes and the strings the
    // presenters/sidebar guard on cannot drift apart - a mismatch (seed grants "productos.acceso"
    // but a guard checks "product.acceso") would only ever surface at runtime otherwise.
    [Collection("Database")]
    public class RolePermissionWalkthroughTests
    {
        private static readonly ISqlConnectionFactory Factory = SqlConnectionFactory.FromConfiguration();
        private static readonly IPermissionService Permissions = new PermissionService(new PermissionRepository(Factory));
        private static readonly IPersonService People = new PersonService(new PersonRepository(Factory));

        private const string Password = "Walkthrough.123";

        // Registers a person with the given role, signs them in, and returns the resolved session.
        private static CurrentUser LogIn(int roleId, out string document)
        {
            document = SqlTestHelper.NewTag();

            Assert.True(People.Register(new Person
            {
                document = document,
                name = "Walkthrough " + document,
                address = "-",
                phone = "-",
                password = Password,
                oPersonType = new TypePerson { idPersonType = roleId }
            }) > 0);

            var loginView = new FakeLoginView { Document = document, Password = Password };
            new LoginPresenter(loginView, People).OnLogin();

            Assert.Null(loginView.ShownError);
            Assert.NotNull(loginView.LoggedInPerson);

            return new CurrentUser(
                loginView.LoggedInPerson,
                Permissions.GetPermissionsForRole(loginView.LoggedInPerson.oPersonType.idPersonType));
        }

        private static void DeletePerson(string document) =>
            SqlTestHelper.ExecuteNonQuery("DELETE FROM person WHERE document_number = @doc", new SqlParameter("@doc", document));

        private static SidebarPermissions Sidebar(CurrentUser user)
        {
            var view = new FakeMainFormView();
            new MainFormPresenter(view, new FakeStoreService(), new FakeNotificationConfigService()).OnLoad(user);
            return view.AppliedSidebarPermissions;
        }

        [Fact]
        public void Empleado_SignsInWithExactlyTheSeededPermissions()
        {
            string doc = null;
            try
            {
                var user = LogIn(3, out doc);

                Assert.Equal(
                    new[] { "alertas.acceso", "alertas.reconocer", "alertas.silenciar",
                            "clientes.acceso", "clientes.gestionar", "ventas.acceso" },
                    user.Permissions.OrderBy(c => c).ToArray());
            }
            finally
            {
                if (doc != null) DeletePerson(doc);
            }
        }

        [Fact]
        public void Empleado_SidebarHidesPurchasesProductsSuppliersUsersRolesAndReports()
        {
            string doc = null;
            try
            {
                var s = Sidebar(LogIn(3, out doc));

                Assert.True(s.Sales);
                Assert.True(s.Clients);
                Assert.True(s.Alerts);

                Assert.False(s.Purchases);
                Assert.False(s.Suppliers);
                Assert.False(s.Management);
                Assert.False(s.Users);
                Assert.False(s.Roles);
                Assert.False(s.Reports);
            }
            finally
            {
                if (doc != null) DeletePerson(doc);
            }
        }

        [Fact]
        public void Empleado_EverySensitiveActionOutsideSalesAndClientsIsRejected()
        {
            string doc = null;
            try
            {
                var user = LogIn(3, out doc);

                // Products. SelectedIndex = 1 so OnDelete gets past its "nothing selected" check
                // and actually reaches the permission guard.
                var productView = new FakeProductManagementView { SelectedIndex = 1, RowCount = 1, ProductId = 7 };
                var products = new ProductManagementPresenter(productView, new FakeProductService(), new FakeCategoryService(), user);
                products.OnSave();
                products.OnDelete();
                Assert.Equal(2, productView.ShownMessages.Count(m => m.Contains("No tiene permiso")));
                Assert.Equal(0, productView.LoadProductsCallCount);

                // Categories
                var categoryView = new FakeCategoryManagementView { SelectedIndex = 1, RowCount = 1, CategoryId = 7 };
                var categories = new CategoryManagementPresenter(categoryView, new FakeCategoryService(), user);
                categories.OnSave();
                categories.OnDelete();
                Assert.Equal(2, categoryView.ShownMessages.Count(m => m.Contains("No tiene permiso")));
                Assert.Empty(categoryView.AddedRows);
                Assert.Empty(categoryView.RemovedIndexes);

                // Suppliers
                var supplierView = new FakeSupplierView { SelectedIndex = 1, RowCount = 1, SupplierId = 7 };
                var suppliers = new SupplierPresenter(supplierView, new FakeSupplierService(), user);
                suppliers.OnSave();
                suppliers.OnDelete();
                Assert.Equal(2, supplierView.ShownMessages.Count(m => m.Contains("No tiene permiso")));
                Assert.Equal(0, supplierView.LoadSuppliersCallCount);

                // Users
                var userView = new FakeUserView { SelectedIndex = 1, RowCount = 1, UserId = 7 };
                var users = new UserPresenter(userView, People, user, Permissions);
                users.OnSave();
                users.OnDelete();
                Assert.Equal(2, userView.ShownMessages.Count(m => m.Contains("No tiene permiso")));
                Assert.Empty(userView.AddedRows);
                Assert.Empty(userView.RemovedIndexes);

                // Store profile
                var storeView = new FakeStoreManagementView();
                new StoreManagementPresenter(storeView, new FakeStoreService(), user).OnSave();
                Assert.Contains(storeView.ErrorMessages, m => m.Contains("No tiene permiso"));

                // Alert thresholds
                var configView = new FakeNotificationConfigView { DaysText = "5", StockText = "5" };
                new NotificationConfigPresenter(configView, new FakeNotificationConfigService(), user).OnSave();
                Assert.Contains("No tiene permiso", configView.ShownMessage ?? "");
                Assert.Equal(0, configView.SaveSucceededCount);

                // Role administration
                var rolesView = new FakeRolesView { SelectedRoleId = 100, RoleNameInput = "X" };
                var roles = new RolesPresenter(rolesView, new FakePermissionService(), user);
                roles.OnSavePermissions();
                roles.OnCreateRole();
                roles.OnRenameRole();
                roles.OnDeleteRole();
                Assert.Equal(4, rolesView.ShownMessages.Count);
                Assert.All(rolesView.ShownMessages, m => Assert.Contains("No tiene permiso", m));
            }
            finally
            {
                if (doc != null) DeletePerson(doc);
            }
        }

        [Fact]
        public void Empleado_CanStillManageClients()
        {
            string doc = null;
            try
            {
                var user = LogIn(3, out doc);

                var clientView = new FakeClientView
                {
                    SelectedIndex = 1,
                    PersonId = 7,
                    ConfirmDeleteResult = false, // stop OnDelete right after the permission check
                    ValidationErrors = new List<string> { "stop OnSave right after the permission check" }
                };
                var clients = new ClientPresenter(clientView, People, user);
                clients.OnSave();
                clients.OnDelete();

                Assert.DoesNotContain(clientView.ShownMessages, m => m.Contains("No tiene permiso"));
            }
            finally
            {
                if (doc != null) DeletePerson(doc);
            }
        }

        [Fact]
        public void AdministradorGeneral_SidebarExposesEverySection()
        {
            string doc = null;
            try
            {
                var s = Sidebar(LogIn(1, out doc));

                Assert.True(s.Sales && s.Purchases && s.Clients && s.Suppliers &&
                            s.Management && s.Users && s.Roles && s.Reports && s.Alerts);
            }
            finally
            {
                if (doc != null) DeletePerson(doc);
            }
        }

        [Fact]
        public void Administrador_SeesMostSectionsButNotRoleAdministration()
        {
            string doc = null;
            try
            {
                var user = LogIn(2, out doc);
                var s = Sidebar(user);

                Assert.True(s.Sales && s.Purchases && s.Clients && s.Suppliers &&
                            s.Management && s.Users && s.Reports && s.Alerts);
                Assert.False(s.Roles);

                // Even reaching RolesPresenter directly, every mutating action is refused.
                var rolesView = new FakeRolesView { SelectedRoleId = 100, RoleNameInput = "X" };
                var roles = new RolesPresenter(rolesView, new FakePermissionService(), user);
                roles.OnSavePermissions();
                roles.OnCreateRole();
                roles.OnRenameRole();
                roles.OnDeleteRole();
                Assert.All(rolesView.ShownMessages, m => Assert.Contains("No tiene permiso", m));

                // And the store profile stays read-only for a plain Administrador.
                var storeView = new FakeStoreManagementView();
                new StoreManagementPresenter(storeView, new FakeStoreService(), user).OnSave();
                Assert.Contains(storeView.ErrorMessages, m => m.Contains("No tiene permiso"));
            }
            finally
            {
                if (doc != null) DeletePerson(doc);
            }
        }
    }
}
