using PharmacySystem.Model;
using PharmacySystem.Presentation;
using Xunit;

namespace PharmacySystem.Tests.Presentation
{
    // StoreManagementPresenter calls CultureInfoHelper.SupportedCurrencies/SetCurrency, which read
    // and mutate a process-wide static field. Shares the "Database" collection with
    // CultureInfoHelperTests and ReportPresenterTests for the same reason: these can never run
    // concurrently without racing on that shared state.
    [Collection("Database")]
    public class StoreManagementPresenterTests
    {
        private static StoreManagementPresenter CreatePresenter(FakeStoreManagementView view, FakeStoreService service)
            => new StoreManagementPresenter(view, service, TestUser.With("tienda.editar"));

        [Fact]
        public void OnSave_WithoutEditPermission_ShowsDeniedAndDoesNotSave()
        {
            var view = new FakeStoreManagementView { Document = "1", CompanyName = "C", Email = "e@e.co", Phone = "9", Address = "A", SelectedCurrency = "es-EC" };
            new StoreManagementPresenter(view, new FakeStoreService(), TestUser.With()).OnSave();

            Assert.Contains(view.ErrorMessages, m => m.Contains("No tiene permiso"));
            Assert.Empty(view.InfoMessages);
        }

        [Fact]
        public void OnLoad_PopulatesFieldsAndCurrencyOptions()
        {
            var view = new FakeStoreManagementView();
            var service = new FakeStoreService
            {
                ListStoreResult = new Store
                {
                    document = "0102030405", companyName = "Farmacia Central", email = "contacto@farmacia.com",
                    phone = "0999999999", address = "Av. Siempre Viva", currencyCulture = "es-EC"
                },
                HasOperationalDataResult = false
            };

            CreatePresenter(view, service).OnLoad();

            Assert.Equal("Farmacia Central", view.LoadedCompanyName);
            Assert.NotNull(view.LoadedCurrencyOptions);
            Assert.NotEmpty(view.LoadedCurrencyOptions);
            Assert.True(view.CurrencyEditable);
        }

        [Fact]
        public void OnLoad_WithOperationalData_LocksCurrencyEditing()
        {
            var view = new FakeStoreManagementView();
            var service = new FakeStoreService
            {
                ListStoreResult = new Store(),
                HasOperationalDataResult = true
            };

            CreatePresenter(view, service).OnLoad();

            Assert.False(view.CurrencyEditable);
        }

        [Fact]
        public void OnSave_ValidationErrors_ShowsThemAndNeverCallsService()
        {
            var view = new FakeStoreManagementView { ValidationErrors = new System.Collections.Generic.List<string> { "El RUC/CI es requerido" } };
            var service = new FakeStoreService();

            CreatePresenter(view, service).OnSave();

            Assert.Equal(new System.Collections.Generic.List<string> { "El RUC/CI es requerido" }, view.ShownValidationErrors);
            Assert.Null(service.UpdatedWith);
        }

        [Fact]
        public void OnSave_Succeeds_ShowsUpdatedMessage()
        {
            var view = new FakeStoreManagementView
            {
                Document = "0102030405", CompanyName = "Farmacia Central", Email = "contacto@farmacia.com",
                Phone = "0999999999", Address = "Av. Siempre Viva", SelectedCurrency = "es-EC"
            };
            var service = new FakeStoreService { UpdateStoreResult = true };

            CreatePresenter(view, service).OnSave();

            Assert.Equal("0102030405", service.UpdatedWith.document);
            Assert.Equal(new[] { "Se actualizaron los datos ingresados exitosamente" }, view.InfoMessages);
            Assert.Empty(view.ErrorMessages);
        }

        [Fact]
        public void OnSave_Fails_ShowsErrorMessage()
        {
            var view = new FakeStoreManagementView { SelectedCurrency = "es-EC" };
            var service = new FakeStoreService { UpdateStoreResult = false };

            CreatePresenter(view, service).OnSave();

            Assert.Equal(new[] { "No se pudo guardar los datos ingresados\nRevise los datos" }, view.ErrorMessages);
            Assert.Empty(view.InfoMessages);
        }
    }
}
