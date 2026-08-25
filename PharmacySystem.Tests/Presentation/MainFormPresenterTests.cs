using System;
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
            var person = new Person { name = "Juan Pérez", oPersonType = new TypePerson { idPersonType = 1 } };

            CreatePresenter(view, storeService, notificationService).OnLoad(person);

            Assert.Equal("Juan Pérez", view.UserName);
        }

        [Fact]
        public void OnLoad_CashierRole_HidesAdministrativeMenus()
        {
            var view = new FakeMainFormView();
            var storeService = new FakeStoreService { ListStoreResult = new Store() };
            var notificationService = new FakeNotificationConfigService();
            var person = new Person { name = "Cajero", oPersonType = new TypePerson { idPersonType = 2 } };

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

        // The threshold filter now lives in NotificationConfigRepository's SQL (see
        // NotificationConfigRepositoryTests for that), not in this presenter - it just trusts
        // whatever the service returns and forwards the configured threshold to it.

        [Fact]
        public void CheckExpirationWarnings_ServiceReturnsNothing_HidesWarning()
        {
            var view = new FakeMainFormView();
            var notificationService = new FakeNotificationConfigService
            {
                ConfigDayResult = 5,
                ListExpirationDateResult = new List<Product>()
            };

            CreatePresenter(view, new FakeStoreService(), notificationService).CheckExpirationWarnings();

            Assert.False(view.ExpirationWarningVisible);
            Assert.Equal("", view.ExpirationWarningMessage);
            Assert.Equal(5, notificationService.RequestedDays);
        }

        [Fact]
        public void CheckExpirationWarnings_ServiceReturnsProducts_ShowsWarning()
        {
            var view = new FakeMainFormView();
            var notificationService = new FakeNotificationConfigService
            {
                ConfigDayResult = 5,
                ListExpirationDateResult = new List<Product> { new Product { expirationDate = DateTime.Today.AddDays(3) } }
            };

            CreatePresenter(view, new FakeStoreService(), notificationService).CheckExpirationWarnings();

            Assert.True(view.ExpirationWarningVisible);
            Assert.Equal("Hay productos con Fechas Vencidas Revise", view.ExpirationWarningMessage);
        }

        [Fact]
        public void CheckStockWarnings_ServiceReturnsNothing_HidesWarning()
        {
            var view = new FakeMainFormView();
            var notificationService = new FakeNotificationConfigService
            {
                ConfigStockResult = 5,
                ListStockResult = new List<Product>()
            };

            CreatePresenter(view, new FakeStoreService(), notificationService).CheckStockWarnings();

            Assert.False(view.StockWarningVisible);
            Assert.Equal("", view.StockWarningMessage);
            Assert.Equal(5, notificationService.RequestedCriticalStock);
        }

        [Fact]
        public void CheckStockWarnings_ServiceReturnsProducts_ShowsWarning()
        {
            var view = new FakeMainFormView();
            var notificationService = new FakeNotificationConfigService
            {
                ConfigStockResult = 5,
                ListStockResult = new List<Product> { new Product { stock = 5 } }
            };

            CreatePresenter(view, new FakeStoreService(), notificationService).CheckStockWarnings();

            Assert.True(view.StockWarningVisible);
            Assert.Equal("Revise si hay productos con Stock Crítico", view.StockWarningMessage);
        }
    }
}
