using System.Collections.Generic;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using Xunit;

namespace PharmacySystem.Tests.Presentation
{
    public class MainFormPresenterTests
    {
        private static MainFormPresenter CreatePresenter(FakeMainFormView view, FakeNotificationConfigService notificationService)
            => new MainFormPresenter(view, notificationService);

        private static CurrentUser User(string name, string roleDescription, params string[] permissions) =>
            new CurrentUser(
                new Person { name = name, oPersonType = new TypePerson { idPersonType = 1, description = roleDescription } },
                permissions);

        [Fact]
        public void OnLoad_SetsUserNameAndRole()
        {
            var view = new FakeMainFormView();
            var notificationService = new FakeNotificationConfigService();

            CreatePresenter(view, notificationService)
                .OnLoad(User("Juan Pérez", "Administrador General", "ventas.acceso"));

            Assert.Equal("Juan Pérez", view.UserName);
            Assert.Equal("Administrador General", view.UserRole);
        }

        [Fact]
        public void OnLoad_CashierPermissions_ShowsOnlyTheGrantedSections()
        {
            var view = new FakeMainFormView();
            var presenter = CreatePresenter(view, new FakeNotificationConfigService());

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
            Assert.False(p.CashCount);
        }

        [Fact]
        public void OnLoad_CajaAccesoPermission_ShowsTheCashCountButton()
        {
            var view = new FakeMainFormView();
            var presenter = CreatePresenter(view, new FakeNotificationConfigService());

            presenter.OnLoad(User("Admin", "Administrador", "caja.acceso"));

            Assert.True(view.AppliedSidebarPermissions.CashCount);
        }

        [Fact]
        public void OnLoad_ManagementButton_ShowsIfAnyOfItsTabsIsAllowed()
        {
            var view = new FakeMainFormView();
            var presenter = CreatePresenter(view, new FakeNotificationConfigService());

            presenter.OnLoad(User("Encargado", "Custom", "categorias.acceso"));

            Assert.True(view.AppliedSidebarPermissions.Management);
        }

        [Fact]
        public void OnLoad_ReportsButton_FollowsReportesAcceso()
        {
            var view = new FakeMainFormView();
            var presenter = CreatePresenter(view, new FakeNotificationConfigService());

            // A "reponedor" role: opens reports, but inside only sees purchases and products.
            presenter.OnLoad(User("Reponedor", "Custom", "reportes.acceso", "reportes.compras", "reportes.productos"));
            Assert.True(view.AppliedSidebarPermissions.Reports);

            // Holding an inner report permission without reportes.acceso does not show the button.
            var view2 = new FakeMainFormView();
            CreatePresenter(view2, new FakeNotificationConfigService())
                .OnLoad(User("Sin acceso", "Custom", "reportes.compras"));
            Assert.False(view2.AppliedSidebarPermissions.Reports);
        }

        [Fact]
        public void OnLoad_FullPermissions_ShowsEverySection()
        {
            var view = new FakeMainFormView();
            var presenter = CreatePresenter(view, new FakeNotificationConfigService());

            presenter.OnLoad(User("Admin", "Administrador General",
                "ventas.acceso", "compras.acceso", "clientes.acceso", "proveedores.acceso",
                "productos.acceso", "categorias.acceso", "tienda.acceso",
                "usuarios.acceso", "roles.gestionar", "alertas.acceso", "reportes.acceso"));

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

            CreatePresenter(view, notificationService).RefreshAlerts();

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

            CreatePresenter(view, notificationService).RefreshAlerts();

            Assert.Same(alerts[0], Assert.Single(view.ShownAlerts));
        }

        [Fact]
        public void RefreshAlerts_DatabaseUnavailable_IsSwallowed()
        {
            var view = new FakeMainFormView();
            var notificationService = new FakeNotificationConfigService { GetActiveAlertsThrows = true };

            // Runs off the UI thread on a 5-minute timer: a transient outage must not propagate
            // (it would surface as an unobserved task exception) nor pop a dialog. The badge is
            // left untouched - ShowAlerts is never called - and the next tick retries.
            CreatePresenter(view, notificationService).RefreshAlerts();

            Assert.Null(view.ShownAlerts);
        }
    }
}
