using System.Collections.Generic;
using System.Linq;
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
            => new SupplierPresenter(view, service, TestUser.With("proveedores.gestionar"), new FakeSecurityAudit());

        [Fact]
        public void OnSave_And_OnDelete_AreAudited()
        {
            var audit = new FakeSecurityAudit();
            var createView = new FakeSupplierView { SupplierId = 0, RowCount = 3, Document = "76.1-2", CompanyName = "Acme", Email = "a@b.cl", Phone = "9" };
            new SupplierPresenter(createView, new FakeSupplierService { RegisterResult = 42 }, TestUser.With("proveedores.gestionar"), audit).OnSave();

            var deleteView = new FakeSupplierView { SelectedIndex = 3, SupplierId = 42, CompanyName = "Acme", Document = "76.1-2" };
            new SupplierPresenter(deleteView, new FakeSupplierService { DeleteResult = true }, TestUser.With("proveedores.gestionar"), audit).OnDelete();

            Assert.Equal(new[] { "supplier.create", "supplier.delete" }, audit.Recorded.Select(e => e.Action));
            Assert.Equal(42, audit.Recorded[0].EntityId);
            Assert.Contains("Acme", audit.Recorded[0].Summary);
        }

        [Fact]
        public void OnSave_WithoutManagePermission_ShowsDeniedAndDoesNotRegister()
        {
            var view = new FakeSupplierView { SupplierId = 0, SelectedIndex = 0, RowCount = 0, Document = "1", CompanyName = "C", Email = "e@e.co", Phone = "9" };
            new SupplierPresenter(view, new FakeSupplierService(), TestUser.With(), new FakeSecurityAudit()).OnSave();

            Assert.Contains(view.ShownMessages, m => m.Contains("No tiene permiso"));
            Assert.Equal(0, view.LoadSuppliersCallCount);
        }

        [Fact]
        public void OnDelete_WithoutManagePermission_ShowsDeniedAndDoesNotRemove()
        {
            var view = new FakeSupplierView { SelectedIndex = 2, SupplierId = 7 };
            new SupplierPresenter(view, new FakeSupplierService(), TestUser.With(), new FakeSecurityAudit()).OnDelete();

            Assert.Contains(view.ShownMessages, m => m.Contains("No tiene permiso"));
            Assert.Equal(0, view.LoadSuppliersCallCount);
        }

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

            Assert.Single(view.LoadedSuppliers);
            Assert.Equal("Acme", view.LoadedSuppliers[0].CompanyName);
            Assert.Equal((1, 1, 1), view.LastPageInfo);
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
            Assert.True(view.ClearFormCalled);
            Assert.Equal(1, view.LoadSuppliersCallCount);
            Assert.Empty(view.ShownMessages);
        }

        [Fact]
        public void OnSave_NewSupplier_DuplicateDocument_ShowsMessageAndDoesNotAddRow()
        {
            var view = new FakeSupplierView { SupplierId = 0, RowCount = 0 };
            var service = new FakeSupplierService { RegisterResult = 0 };

            CreatePresenter(view, service).OnSave();

            Assert.Equal(new[] { "Ya existe un proveedor con ese documento" }, view.ShownMessages);
            Assert.Equal(0, view.LoadSuppliersCallCount);
            Assert.False(view.ClearFormCalled);
        }

        [Fact]
        public void OnSave_ExistingSupplier_UpdateSucceeds_ReloadsPageAndClearsForm()
        {
            var view = new FakeSupplierView { SupplierId = 7, SelectedIndex = 2, RowCount = 5, CompanyName = "Updated" };
            var service = new FakeSupplierService { UpdateResult = true };

            CreatePresenter(view, service).OnSave();

            Assert.Equal("Updated", service.UpdatedWith.companyName);
            Assert.Equal(1, view.LoadSuppliersCallCount);
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

            Assert.Equal(0, view.LoadSuppliersCallCount);
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
            Assert.Equal(0, view.LoadSuppliersCallCount);
        }

        [Fact]
        public void OnDelete_Succeeds_ReloadsPageAndClearsForm()
        {
            var view = new FakeSupplierView { SelectedIndex = 3, SupplierId = 9 };
            var service = new FakeSupplierService { DeleteResult = true };

            CreatePresenter(view, service).OnDelete();

            Assert.Equal(9, service.DeletedId);
            Assert.Equal(1, view.LoadSuppliersCallCount);
            Assert.True(view.ClearFormCalled);
        }

        [Fact]
        public void OnSearch_QueriesWithTheTermAndResetsToPageOne()
        {
            var view = new FakeSupplierView();
            var service = new FakeSupplierService
            {
                ListResult = new List<Supplier>
                {
                    new Supplier { idSupplier = 1, companyName = "Acme" },
                    new Supplier { idSupplier = 2, companyName = "Globex" }
                }
            };
            var presenter = CreatePresenter(view, service);
            presenter.OnLoad();

            view.SearchText = "Globex";
            presenter.OnSearch();

            Assert.Equal("Globex", service.LastPagedCall?.Search);
            Assert.Equal(1, service.LastPagedCall?.Page);
            Assert.Single(view.LoadedSuppliers);
            Assert.Equal("Globex", view.LoadedSuppliers[0].CompanyName);
        }

        [Fact]
        public void OnNextPage_ThenPrevious_MoveOnePageAndClampAtEnds()
        {
            var view = new FakeSupplierView();
            var many = new List<Supplier>();
            for (int i = 1; i <= 60; i++) many.Add(new Supplier { idSupplier = i, companyName = "S" + i.ToString("D2") });
            var service = new FakeSupplierService { ListResult = many };
            var presenter = CreatePresenter(view, service);
            presenter.OnLoad();

            presenter.OnNextPage();
            Assert.Equal(2, view.LastPageInfo?.CurrentPage);

            presenter.OnNextPage(); // already last
            Assert.Equal(2, view.LastPageInfo?.CurrentPage);

            presenter.OnPreviousPage();
            Assert.Equal(1, view.LastPageInfo?.CurrentPage);
        }
    }
}
