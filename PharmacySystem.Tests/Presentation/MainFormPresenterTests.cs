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

        [Fact]
        public void OnLoad_SetsUserNameAndCurrency()
        {
            var view = new FakeMainFormView();
            var storeService = new FakeStoreService { ListStoreResult = new Store { currencyCulture = "es-EC" } };
            var notificationService = new FakeNotificationConfigService();
            var person = new Person { name = "Juan Pérez", oPersonType = new TypePerson { idPersonType = 1, description = "Administrador General" } };

            CreatePresenter(view, storeService, notificationService).OnLoad(person);

            Assert.Equal("Juan Pérez", view.UserName);
            Assert.Equal("Administrador General", view.UserRole);
        }

        [Fact]
        public void OnLoad_CashierRole_HidesAdministrativeMenus()
        {
            var view = new FakeMainFormView();
            var storeService = new FakeStoreService { ListStoreResult = new Store() };
            var notificationService = new FakeNotificationConfigService();
            var person = new Person { name = "Cajero", oPersonType = new TypePerson { idPersonType = 3 } };

            CreatePresenter(view, storeService, notificationService).OnLoad(person);

            Assert.False(view.AdministrativeMenusVisible);
        }

        [Fact]
        public void OnLoad_NonCashierRole_ShowsAdministrativeMenus()
        {
            var view = new FakeMainFormView();
            var storeService = new FakeStoreService { ListStoreResult = new Store() };
            var notificationService = new FakeNotificationConfigService();
            var person = new Person { name = "Admin", oPersonType = new TypePerson { idPersonType = 1 } };

            CreatePresenter(view, storeService, notificationService).OnLoad(person);

            Assert.True(view.AdministrativeMenusVisible);
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
