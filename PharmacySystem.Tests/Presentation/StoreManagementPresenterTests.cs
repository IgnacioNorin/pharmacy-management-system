using System.Collections.Generic;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using Xunit;

namespace PharmacySystem.Tests.Presentation
{
    public class StoreManagementPresenterTests
    {
        private static StoreManagementPresenter CreatePresenter(FakeStoreManagementView view, FakeStoreService service)
            => new StoreManagementPresenter(view, service, TestUser.With("tienda.editar"));

        [Fact]
        public void OnSave_WithoutEditPermission_ShowsDeniedAndDoesNotSave()
        {
            var view = new FakeStoreManagementView { Document = "1", CompanyName = "C", Email = "e@e.co", Phone = "9", Address = "A" };
            new StoreManagementPresenter(view, new FakeStoreService(), TestUser.With()).OnSave();

            Assert.Contains(view.ErrorMessages, m => m.Contains("No tiene permiso"));
            Assert.Empty(view.InfoMessages);
        }

        [Fact]
        public void OnLoad_PopulatesTheStoreFields()
        {
            var view = new FakeStoreManagementView();
            var service = new FakeStoreService
            {
                ListStoreResult = new Store
                {
                    document = "0102030405", companyName = "Farmacia Central", email = "contacto@farmacia.com",
                    phone = "0999999999", address = "Av. Siempre Viva"
                }
            };

            CreatePresenter(view, service).OnLoad();

            Assert.Equal("Farmacia Central", view.LoadedCompanyName);
        }

        [Fact]
        public void OnSave_ValidationErrors_ShowsThemAndNeverCallsService()
        {
            var view = new FakeStoreManagementView { ValidationErrors = new List<string> { "El documento es requerido" } };
            var service = new FakeStoreService();

            CreatePresenter(view, service).OnSave();

            Assert.Equal(new List<string> { "El documento es requerido" }, view.ShownValidationErrors);
            Assert.Null(service.UpdatedWith);
        }

        [Fact]
        public void OnSave_Succeeds_ShowsUpdatedMessage()
        {
            var view = new FakeStoreManagementView
            {
                Document = "0102030405", CompanyName = "Farmacia Central", Email = "contacto@farmacia.com",
                Phone = "0999999999", Address = "Av. Siempre Viva"
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
            var view = new FakeStoreManagementView();
            var service = new FakeStoreService { UpdateStoreResult = false };

            CreatePresenter(view, service).OnSave();

            Assert.Equal(new[] { "No se pudo guardar los datos ingresados\nRevise los datos" }, view.ErrorMessages);
            Assert.Empty(view.InfoMessages);
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
            var view = new FakeStoreManagementView { DefaultDocumentType = "Factura" };
            var service = new FakeStoreService { UpdateStoreResult = true };

            CreatePresenter(view, service).OnSave();

            Assert.Equal("Factura", service.UpdatedWith.defaultDocumentType);
        }

        [Fact]
        public void OnSave_ValidTaxRate_PersistsIt()
        {
            var view = new FakeStoreManagementView { TaxRate = "16" };
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
                var view = new FakeStoreManagementView { TaxRate = bad };
                var service = new FakeStoreService();

                CreatePresenter(view, service).OnSave();

                Assert.Contains(view.ErrorMessages, m => m.Contains("tasa de IVA"));
                Assert.Null(service.UpdatedWith);
            }
        }
    }
}
