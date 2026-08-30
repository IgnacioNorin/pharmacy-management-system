using System.Linq;
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
            var view = new FakeStoreManagementView { ValidationErrors = new System.Collections.Generic.List<string> { "El documento es requerido" } };
            var service = new FakeStoreService();

            CreatePresenter(view, service).OnSave();

            Assert.Equal(new System.Collections.Generic.List<string> { "El documento es requerido" }, view.ShownValidationErrors);
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

        [Fact]
        public void OnLoad_LoadsCountryPresetOptionsAndSelectsTheStoredCode()
        {
            var view = new FakeStoreManagementView();
            var service = new FakeStoreService { ListStoreResult = new Store { countryCode = "CL" } };

            CreatePresenter(view, service).OnLoad();

            Assert.Equal(new[] { "", "CL" }, view.LoadedCountryPresetOptions.Select(o => (string)o.Value).ToArray());
            Assert.Equal(1, view.LoadedCountryPresetSelectedIndex); // "CL"
        }

        [Fact]
        public void OnLoad_NoCountryCode_SelectsTheGenericPreset()
        {
            var view = new FakeStoreManagementView();
            var service = new FakeStoreService { ListStoreResult = new Store { countryCode = null } };

            CreatePresenter(view, service).OnLoad();

            Assert.Equal(0, view.LoadedCountryPresetSelectedIndex); // "" (Genérico)
        }

        [Fact]
        public void OnCountryPresetChanged_ConcretePreset_PreFillsTaxRateAndCurrency()
        {
            var view = new FakeStoreManagementView { SelectedCountryCode = "CL" };

            CreatePresenter(view, new FakeStoreService()).OnCountryPresetChanged();

            Assert.Equal("19", view.SetTaxRateValue);
            Assert.Equal("es-CL", view.SelectedCurrencyValue);
        }

        [Fact]
        public void OnCountryPresetChanged_GenericPreset_LeavesTheFieldsAlone()
        {
            var view = new FakeStoreManagementView { SelectedCountryCode = "" };

            CreatePresenter(view, new FakeStoreService()).OnCountryPresetChanged();

            Assert.Null(view.SetTaxRateValue);
            Assert.Null(view.SelectedCurrencyValue);
        }

        [Fact]
        public void OnCountryPresetChanged_RefreshesTheDocumentTypesAndKeepsAValidCurrentChoice()
        {
            var view = new FakeStoreManagementView { SelectedCountryCode = "CL", DefaultDocumentType = "Factura" };

            CreatePresenter(view, new FakeStoreService()).OnCountryPresetChanged();

            Assert.Equal(new[] { "Boleta", "Factura" }, view.LoadedDocumentTypeOptions);
            Assert.Equal("Factura", view.LoadedDocumentTypeSelected); // the current choice survives
        }

        [Fact]
        public void OnSave_PersistsTheSelectedCountryCode_NullForGeneric()
        {
            var chileView = new FakeStoreManagementView
            {
                Document = "1", CompanyName = "C", Email = "e@e.co", Phone = "9", Address = "A",
                SelectedCurrency = "es-CL", SelectedCountryCode = "CL"
            };
            var service = new FakeStoreService { UpdateStoreResult = true };
            CreatePresenter(chileView, service).OnSave();
            Assert.Equal("CL", service.UpdatedWith.countryCode);

            var genericView = new FakeStoreManagementView
            {
                Document = "1", CompanyName = "C", Email = "e@e.co", Phone = "9", Address = "A",
                SelectedCurrency = "en-US", SelectedCountryCode = ""
            };
            var service2 = new FakeStoreService { UpdateStoreResult = true };
            CreatePresenter(genericView, service2).OnSave();
            Assert.Null(service2.UpdatedWith.countryCode);
        }

        [Fact]
        public void OnLoad_SetsTaxRateFromStore()
        {
            var view = new FakeStoreManagementView();
            var service = new FakeStoreService { ListStoreResult = new Store { defaultTaxRate = 21m } };

            CreatePresenter(view, service).OnLoad();

            Assert.Equal("21", view.SetTaxRateValue);
        }

        [Fact]
        public void OnLoad_LoadsDocumentTypeOptionsWithTheStoreDefaultSelected()
        {
            var view = new FakeStoreManagementView();
            var service = new FakeStoreService { ListStoreResult = new Store { defaultDocumentType = "Factura" } };

            CreatePresenter(view, service).OnLoad();

            Assert.Equal(new[] { "Boleta", "Factura" }, view.LoadedDocumentTypeOptions);
            Assert.Equal("Factura", view.LoadedDocumentTypeSelected);
        }

        [Fact]
        public void OnSave_PersistsDefaultDocumentType()
        {
            var view = new FakeStoreManagementView { SelectedCurrency = "es-EC", DefaultDocumentType = "Factura" };
            var service = new FakeStoreService { UpdateStoreResult = true };

            CreatePresenter(view, service).OnSave();

            Assert.Equal("Factura", service.UpdatedWith.defaultDocumentType);
        }

        [Fact]
        public void OnSave_ValidTaxRate_PersistsIt()
        {
            var view = new FakeStoreManagementView { SelectedCurrency = "es-EC", TaxRate = "16" };
            var service = new FakeStoreService { UpdateStoreResult = true };

            CreatePresenter(view, service).OnSave();

            Assert.Equal(16m, service.UpdatedWith.defaultTaxRate);
            Assert.Contains(view.InfoMessages, m => m.Contains("actualizaron"));
        }

        [Fact]
        public void OnSave_InvalidOrOutOfRangeTaxRate_ShowsErrorAndDoesNotSave()
        {
            foreach (string bad in new[] { "abc", "-1", "150" })
            {
                var view = new FakeStoreManagementView { SelectedCurrency = "es-EC", TaxRate = bad };
                var service = new FakeStoreService();

                CreatePresenter(view, service).OnSave();

                Assert.Contains(view.ErrorMessages, m => m.Contains("tasa de IVA"));
                Assert.Null(service.UpdatedWith);
            }
        }
    }
}
