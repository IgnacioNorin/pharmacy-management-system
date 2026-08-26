using System.Collections.Generic;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using Xunit;

namespace PharmacySystem.Tests.Presentation
{
    // No SQL Server, no WinForms Form, no [Collection("Database")]: this is the actual payoff of
    // the MVP split. Every one of these ran in milliseconds; the equivalent check against the
    // pre-migration frmSupplier.cs would have needed a running Form instance, which is exactly
    // what could not be automated for frmManagement earlier in this project.
    public class SupplierPresenterTests
    {
        private static SupplierPresenter CreatePresenter(FakeSupplierView view, FakeSupplierService service)
            => new SupplierPresenter(view, service);

        [Fact]
        public void OnLoad_PopulatesViewFromService()
        {
            var view = new FakeSupplierView();
            var service = new FakeSupplierService
            {
                ListResult = new List<Supplier>
                {
                    new Supplier { idSupplier = 1, document = "123", companyName = "Acme", email = "a@a.com", phone = "111" }
                }
            };

            CreatePresenter(view, service).OnLoad();

            Assert.True(service.ListCalled);
            Assert.Single(view.LoadedSuppliers);
            Assert.Equal("Acme", view.LoadedSuppliers[0].CompanyName);
        }

        [Fact]
        public void OnSave_ValidationErrors_ShowsThemAndNeverCallsService()
        {
            var view = new FakeSupplierView { ValidationErrors = new List<string> { "El correo es inválido" } };
            var service = new FakeSupplierService();

            CreatePresenter(view, service).OnSave();

            Assert.Equal(new List<string> { "El correo es inválido" }, view.ShownValidationErrors);
            Assert.Null(service.RegisteredWith);
            Assert.Null(service.UpdatedWith);
        }

        [Fact]
        public void OnSave_NewSupplier_Succeeds_AddsRowAndClearsForm()
        {
            var view = new FakeSupplierView
            {
                SupplierId = 0,
                RowCount = 3,
                Document = " 0102030405 ",
                CompanyName = " Acme SA ",
                Email = "acme@test.local",
                Phone = "0999999999"
            };
            var service = new FakeSupplierService { RegisterResult = 42 };

            CreatePresenter(view, service).OnSave();

            Assert.Equal("0102030405", service.RegisteredWith.document); // trimmed before hitting the service
            Assert.Single(view.AddedRows);
            Assert.Equal(42, view.AddedRows[0].Id);
            Assert.True(view.ClearFormCalled);
            Assert.Empty(view.ShownMessages);
        }

        [Fact]
        public void OnSave_NewSupplier_DuplicateDocument_ShowsMessageAndDoesNotAddRow()
        {
            var view = new FakeSupplierView { SupplierId = 0, RowCount = 0 };
            var service = new FakeSupplierService { RegisterResult = 0 };

            CreatePresenter(view, service).OnSave();

            Assert.Equal(new[] { "Ya existe un proveedor con esa CI/RUC" }, view.ShownMessages);
            Assert.Empty(view.AddedRows);
            Assert.False(view.ClearFormCalled);
        }

        [Fact]
        public void OnSave_ExistingSupplier_UpdateSucceeds_ReplacesRowAndClearsForm()
        {
            var view = new FakeSupplierView { SupplierId = 7, SelectedIndex = 2, RowCount = 5, CompanyName = "Updated" };
            var service = new FakeSupplierService { UpdateResult = true };

            CreatePresenter(view, service).OnSave();

            Assert.Single(view.ReplacedRows);
            Assert.Equal(1, view.ReplacedRows[0].Index); // SelectedIndex (1-based) - 1
            Assert.Equal("Updated", view.ReplacedRows[0].Row.CompanyName);
            Assert.True(view.ClearFormCalled);
        }

        // Pins down a real quirk found in the original frmSupplier.cs: a failed update returns
        // silently, with no MessageBox, unlike every other failure path in this screen. Migrating
        // must not quietly change that - if it's ever fixed, this test should be the one edited.
        [Fact]
        public void OnSave_ExistingSupplier_UpdateFails_DoesNothingSilently()
        {
            var view = new FakeSupplierView { SupplierId = 7, SelectedIndex = 2, RowCount = 5 };
            var service = new FakeSupplierService { UpdateResult = false };

            CreatePresenter(view, service).OnSave();

            Assert.Empty(view.ReplacedRows);
            Assert.False(view.ClearFormCalled);
            Assert.Empty(view.ShownMessages);
        }

        [Fact]
        public void OnDelete_NoSelection_ShowsMessageAndNeverCallsService()
        {
            var view = new FakeSupplierView { SelectedIndex = 0 };
            var service = new FakeSupplierService();

            CreatePresenter(view, service).OnDelete();

            Assert.Equal(new[] { "No se pudo eliminar, seleccione un proveedor" }, view.ShownMessages);
            Assert.Null(service.DeletedId);
        }

        [Fact]
        public void OnDelete_UserCancelsConfirmation_NeverCallsService()
        {
            var view = new FakeSupplierView { SelectedIndex = 1, ConfirmDeleteResult = false };
            var service = new FakeSupplierService();

            CreatePresenter(view, service).OnDelete();

            Assert.Null(service.DeletedId);
            Assert.Empty(view.ShownMessages);
        }

        [Fact]
        public void OnDelete_ServiceFails_ShowsMessageAndDoesNotRemoveRow()
        {
            var view = new FakeSupplierView { SelectedIndex = 1, SupplierId = 9 };
            var service = new FakeSupplierService { DeleteResult = false };

            CreatePresenter(view, service).OnDelete();

            Assert.Equal(new[] { "No se pudo eliminar el registro\nRevise los datos" }, view.ShownMessages);
            Assert.Empty(view.RemovedIndexes);
        }

        [Fact]
        public void OnDelete_Succeeds_RemovesRowAndClearsForm()
        {
            var view = new FakeSupplierView { SelectedIndex = 3, SupplierId = 9 };
            var service = new FakeSupplierService { DeleteResult = true };

            CreatePresenter(view, service).OnDelete();

            Assert.Equal(9, service.DeletedId);
            Assert.Equal(new[] { 2 }, view.RemovedIndexes); // SelectedIndex (1-based) - 1
            Assert.True(view.ClearFormCalled);
        }
    }
}
