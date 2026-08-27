using System.Collections.Generic;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using Xunit;

namespace PharmacySystem.Tests.Presentation
{
    // MainFormPresenter.OnLoad calls CultureInfoHelper.SetCurrency, which mutates a process-wide
    // static field. Shares the "Database" collection with CultureInfoHelperTests for the same
    // reason as ReportPresenterTests/StoreManagementPresenterTests.
    [Collection("Database")]
    public class MainFormPresenterTests
    {
        private static MainFormPresenter CreatePresenter(FakeMainFormView view, FakeStoreService storeService, FakeNotificationConfigService notificationService)
            => new MainFormPresenter(view, storeService, notificationService);

        private static CurrentUser User(string name, string roleDescription, params string[] permissions) =>
            new CurrentUser(
                new Person { name = name, oPersonType = new TypePerson { idPersonType = 1, description = roleDescription } },
                permissions);

        [Fact]
        public void OnLoad_SetsUserNameAndRole()
        {
            var view = new FakeMainFormView();
            var storeService = new FakeStoreService { ListStoreResult = new Store { currencyCulture = "es-EC" } };
            var notificationService = new FakeNotificationConfigService();

            CreatePresenter(view, storeService, notificationService)
                .OnLoad(User("Juan Pérez", "Administrador General", "ventas.acceso"));

            Assert.Equal("Juan Pérez", view.UserName);
            Assert.Equal("Administrador General", view.UserRole);
        }

        [Fact]
        public void OnLoad_CashierPermissions_ShowsOnlyTheGrantedSections()
        {
            var view = new FakeMainFormView();
            var presenter = CreatePresenter(view, new FakeStoreService { ListStoreResult = new Store() }, new FakeNotificationConfigService());

            // The seeded "Empleado" permission set.
            presenter.OnLoad(User("Cajero", "Empleado",
                "ventas.acceso", "clientes.acceso", "clientes.gestionar",
                "alertas.acceso", "alertas.reconocer", "alertas.silenciar"));

            var p = view.AppliedSidebarPermissions;
            Assert.True(p.Sales);
            Assert.True(p.Clients);
            Assert.True(p.Alerts);
            Assert.False(p.Purchases);
            Assert.False(p.Suppliers);
            Assert.False(p.Management);
            Assert.False(p.Users);
            Assert.False(p.Roles);
            Assert.False(p.Reports);
        }

        [Fact]
        public void OnLoad_ManagementButton_ShowsIfAnyOfItsTabsIsAllowed()
        {
            var view = new FakeMainFormView();
            var presenter = CreatePresenter(view, new FakeStoreService { ListStoreResult = new Store() }, new FakeNotificationConfigService());

            presenter.OnLoad(User("Encargado", "Custom", "categorias.acceso"));

            Assert.True(view.AppliedSidebarPermissions.Management);
        }

        [Fact]
        public void OnLoad_FullPermissions_ShowsEverySection()
        {
            var view = new FakeMainFormView();
            var presenter = CreatePresenter(view, new FakeStoreService { ListStoreResult = new Store() }, new FakeNotificationConfigService());

            presenter.OnLoad(User("Admin", "Administrador General",
                "ventas.acceso", "compras.acceso", "clientes.acceso", "proveedores.acceso",
                "productos.acceso", "categorias.acceso", "tienda.acceso",
                "usuarios.acceso", "roles.gestionar", "reportes.acceso", "alertas.acceso"));

            var p = view.AppliedSidebarPermissions;
            Assert.True(p.Sales && p.Purchases && p.Clients && p.Suppliers && p.Management && p.Users && p.Roles && p.Reports && p.Alerts);
        }

        // Fase 3: severity classification itself is tested at the Business layer
        // (NotificationConfigServiceTests) - here we only confirm the presenter fetches the
        // itemized alert list and forwards it to the View untouched.

        [Fact]
        public void RefreshAlerts_NoActiveAlerts_ShowsEmptyList()
        {
            var view = new FakeMainFormView();
            var notificationService = new FakeNotificationConfigService { GetActiveAlertsResult = new List<ProductAlert>() };

            CreatePresenter(view, new FakeStoreService(), notificationService).RefreshAlerts();

            Assert.Empty(view.ShownAlerts);
        }

        [Fact]
        public void RefreshAlerts_ForwardsAlertsFromServiceToView()
        {
            var view = new FakeMainFormView();
            var alerts = new List<ProductAlert>
            {
                new ProductAlert { ProductId = 1, Code = "P1", Name = "Paracetamol", Severity = AlertSeverity.Critical, Detail = "Sin stock" }
            };
            var notificationService = new FakeNotificationConfigService { GetActiveAlertsResult = alerts };

            CreatePresenter(view, new FakeStoreService(), notificationService).RefreshAlerts();

            Assert.Same(alerts[0], Assert.Single(view.ShownAlerts));
        }
    }
}
