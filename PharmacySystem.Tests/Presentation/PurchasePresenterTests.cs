using System.Collections.Generic;
using PharmacySystem.Infrastructure;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using Xunit;

namespace PharmacySystem.Tests.Presentation
{
    public class PurchasePresenterTests
    {
        private static PurchasePresenter CreatePresenter(FakePurchaseView view, FakePurchaseService purchaseService, FakeProductService productService,
            int idPerson = 1, FakeStoreService storeService = null, CurrentUser session = null)
            => new PurchasePresenter(view, purchaseService, productService, storeService ?? new FakeStoreService(),
                session ?? TestUser.With("compras.acceso"), idPerson);

        // The cart lives inside the Presenter now, not the View, so tests that need an existing
        // cart line build it through the real OnAddProduct() path instead of pre-seeding view state.
        private static void AddLine(PurchasePresenter presenter, FakePurchaseView view, int productId, decimal amount, string pricePurchaseText)
        {
            view.SelectedProductId = productId;
            view.SelectedProductCode = "P" + productId;
            view.SelectedProductName = "Product " + productId;
            view.Amount = amount;
            view.PricePurchaseText = pricePurchaseText;
            presenter.OnAddProduct();
        }

        [Fact]
        public void OnProductCodeEntered_KnownCode_SelectsProduct()
        {
            var view = new FakePurchaseView();
            var productService = new FakeProductService
            {
                ListResult = new List<Product> { new Product { idProduct = 5, code = "P1", name = "Paracetamol" } }
            };

            CreatePresenter(view, new FakePurchaseService(), productService).OnProductCodeEntered("P1");

            Assert.Equal((5, "P1", "Paracetamol"), view.SelectedProductSetTo);
        }

        [Fact]
        public void OnProductCodeEntered_UnknownCode_DoesNothing()
        {
            var view = new FakePurchaseView();
            var productService = new FakeProductService { ListResult = new List<Product>() };

            CreatePresenter(view, new FakePurchaseService(), productService).OnProductCodeEntered("nope");

            Assert.Null(view.SelectedProductSetTo);
        }

        [Fact]
        public void OnAddProduct_ValidationErrors_ShowsThemAndNeverAdds()
        {
            var view = new FakePurchaseView { ValidationErrors = new List<string> { "Cantidad requerida" } };

            CreatePresenter(view, new FakePurchaseService(), new FakeProductService()).OnAddProduct();

            Assert.Equal(new[] { "Cantidad requerida" }, view.ShownValidationErrors);
            Assert.Empty(view.RenderedCartLines);
        }

        [Fact]
        public void OnAddProduct_NoProductSelected_ShowsMessage()
        {
            var view = new FakePurchaseView { SelectedProductId = 0 };

            CreatePresenter(view, new FakePurchaseService(), new FakeProductService()).OnAddProduct();

            Assert.Equal(new[] { "Debe seleccionar un producto primero" }, view.ShownMessages);
            Assert.Empty(view.RenderedCartLines);
        }

        [Fact]
        public void OnAddProduct_InvalidPurchasePrice_ShowsMessage()
        {
            var view = new FakePurchaseView { SelectedProductId = 1, PricePurchaseText = "not-a-number" };

            CreatePresenter(view, new FakePurchaseService(), new FakeProductService()).OnAddProduct();

            Assert.Equal(new[] { "Error al convertir el tipo de moneda - Precio Compra\nEjemplo Formato ##.##" }, view.ShownMessages);
            Assert.Empty(view.RenderedCartLines);
        }

        [Fact]
        public void OnAddProduct_ValidEntry_AddsLineRecalculatesTotalAndClearsEntry()
        {
            var view = new FakePurchaseView
            {
                SelectedProductId = 1,
                SelectedProductCode = "P1",
                SelectedProductName = "Paracetamol",
                Amount = 3,
                PricePurchaseText = "2.00"
            };

            CreatePresenter(view, new FakePurchaseService(), new FakeProductService()).OnAddProduct();

            Assert.Single(view.RenderedCartLines);
            Assert.Equal(1, view.RenderedCartLines[0].ProductId);
            Assert.Equal(6m, view.RenderedCartLines[0].SubTotal); // 3 * 2.00
            Assert.True(view.ProductEntryCleared);
            Assert.NotNull(view.TotalText);
        }

        [Fact]
        public void OnAddProduct_ProductAlreadyInCart_ShowsMessageAndDoesNotAddASecondLine()
        {
            var view = new FakePurchaseView();
            var presenter = CreatePresenter(view, new FakePurchaseService(), new FakeProductService());
            AddLine(presenter, view, productId: 1, amount: 1, pricePurchaseText: "2.00");

            view.SelectedProductId = 1;
            view.PricePurchaseText = "2.00";
            presenter.OnAddProduct();

            Assert.Single(view.RenderedCartLines); // unchanged - no second line rendered
            Assert.Contains(view.ShownMessages, m => m.Contains("ya está en la compra"));
        }

        [Fact]
        public void OnRemoveProduct_RemovesLineAndRecalculatesTotal()
        {
            var view = new FakePurchaseView();
            var presenter = CreatePresenter(view, new FakePurchaseService(), new FakeProductService());
            AddLine(presenter, view, productId: 1, amount: 1, pricePurchaseText: "10.00");
            AddLine(presenter, view, productId: 2, amount: 1, pricePurchaseText: "20.00");

            presenter.OnRemoveProduct(0);

            Assert.Single(view.RenderedCartLines);
            Assert.Equal(2, view.RenderedCartLines[0].ProductId);
            Assert.NotNull(view.TotalText);
        }

        [Fact]
        public void OnFinishPurchase_WithoutThePurchasesPermission_IsDeniedAndNeverRegisters()
        {
            var view = new FakePurchaseView { DocumentNumber = "001", SelectedSupplierId = 3 };
            var service = new FakePurchaseService { RegisterResult = true };
            var presenter = CreatePresenter(view, service, new FakeProductService(), session: TestUser.With());
            AddLine(presenter, view, productId: 1, amount: 1, pricePurchaseText: "10.00");

            presenter.OnFinishPurchase();

            Assert.Contains(view.ShownMessages, m => m.Contains("No tiene permiso"));
            Assert.Null(service.RegisteredWith);
        }

        [Fact]
        public void OnFinishPurchase_NoDocumentNumber_ShowsMessageAndFocuses()
        {
            var view = new FakePurchaseView { DocumentNumber = "" };

            CreatePresenter(view, new FakePurchaseService(), new FakeProductService()).OnFinishPurchase();

            Assert.Equal(new[] { "Debe ingresar el numero de documento\npara registrar una compra" }, view.ShownMessages);
            Assert.True(view.DocumentNumberFocused);
        }

        [Fact]
        public void OnFinishPurchase_NoSupplierSelected_ShowsMessage()
        {
            var view = new FakePurchaseView { DocumentNumber = "001", SelectedSupplierId = 0 };

            CreatePresenter(view, new FakePurchaseService(), new FakeProductService()).OnFinishPurchase();

            Assert.Equal(new[] { "Debe seleccionar un proveedor\npara registrar una compra" }, view.ShownMessages);
        }

        [Fact]
        public void OnFinishPurchase_NoProductsInCart_ShowsMessage()
        {
            var view = new FakePurchaseView { DocumentNumber = "001", SelectedSupplierId = 3 };

            CreatePresenter(view, new FakePurchaseService(), new FakeProductService()).OnFinishPurchase();

            Assert.Equal(new[] { "Debe ingresar un producto como minimo\npara registrar una compra" }, view.ShownMessages);
        }

        [Fact]
        public void OnFinishPurchase_Succeeds_RegistersPurchaseClearsAndShowsMessage()
        {
            var view = new FakePurchaseView { DocumentType = "Factura" };
            var purchaseService = new FakePurchaseService { RegisterResult = true };
            var presenter = CreatePresenter(view, purchaseService, new FakeProductService(), idPerson: 42);
            AddLine(presenter, view, productId: 1, amount: 2, pricePurchaseText: "5.00");

            view.DocumentNumber = " 001 ";
            view.SelectedSupplierId = 3;
            presenter.OnFinishPurchase();

            Assert.NotNull(purchaseService.RegisteredWith);
            Assert.Equal("001", purchaseService.RegisteredWith.documentNumber);
            Assert.Equal(3, purchaseService.RegisteredWith.oSupplier.idSupplier);
            Assert.Equal(42, purchaseService.RegisteredWith.oPerson.idPerson);
            Assert.Equal(10m, purchaseService.RegisteredWith.totalAmount);
            Assert.Single(purchaseService.RegisteredWith.oPurchaseDetail);
            Assert.True(view.PurchaseCleared);
            Assert.Equal(new[] { "La compra fue registrada" }, view.ShownMessages);
        }

        [Fact]
        public void OnFinishPurchase_ServiceFails_ShowsErrorAndDoesNotClear()
        {
            var view = new FakePurchaseView();
            var purchaseService = new FakePurchaseService { RegisterResult = false };
            var presenter = CreatePresenter(view, purchaseService, new FakeProductService());
            AddLine(presenter, view, productId: 1, amount: 1, pricePurchaseText: "10.00");

            view.DocumentNumber = "001";
            view.SelectedSupplierId = 3;
            presenter.OnFinishPurchase();

            Assert.Equal(new[] { "No se pudo registrar la compra" }, view.ShownMessages);
            Assert.False(view.PurchaseCleared);
        }

        [Fact]
        public void OnFinishPurchase_DuplicateInvoice_ShowsSpecificMessageAndDoesNotClear()
        {
            var view = new FakePurchaseView();
            var purchaseService = new FakePurchaseService { RegisterThrows = new DuplicateInvoiceException() };
            var presenter = CreatePresenter(view, purchaseService, new FakeProductService());
            AddLine(presenter, view, productId: 1, amount: 1, pricePurchaseText: "10.00");

            view.DocumentNumber = "001";
            view.SelectedSupplierId = 3;
            presenter.OnFinishPurchase();

            Assert.Equal(new[] { DuplicateInvoiceException.DefaultMessage }, view.ShownMessages);
            Assert.False(view.PurchaseCleared);
        }

        [Fact]
        public void OnFinishPurchase_DatabaseUnavailable_ShowsConnectionErrorAndDoesNotClear()
        {
            var view = new FakePurchaseView();
            var purchaseService = new FakePurchaseService { RegisterThrows = new DataUnavailableException() };
            var presenter = CreatePresenter(view, purchaseService, new FakeProductService());
            AddLine(presenter, view, productId: 1, amount: 1, pricePurchaseText: "10.00");

            view.DocumentNumber = "001";
            view.SelectedSupplierId = 3;
            presenter.OnFinishPurchase();

            Assert.Equal(new[] { DataUnavailableException.DefaultMessage }, view.ShownMessages);
            Assert.False(view.PurchaseCleared);
        }

        // Fase 2 of the alerts rework: a successful purchase must let MainForm know stock just
        // changed, instead of it waiting up to 5 minutes for the next safety-net timer tick.
        [Fact]
        public void OnFinishPurchase_Succeeds_RaisesInventoryChangedNotification()
        {
            var view = new FakePurchaseView { DocumentNumber = "001", SelectedSupplierId = 3 };
            var purchaseService = new FakePurchaseService { RegisterResult = true };
            var presenter = CreatePresenter(view, purchaseService, new FakeProductService());
            AddLine(presenter, view, productId: 1, amount: 1, pricePurchaseText: "10.00");

            bool raised = false;
            void Handler() => raised = true;
            InventoryChangeNotifier.StockChanged += Handler;
            try
            {
                presenter.OnFinishPurchase();
            }
            finally
            {
                InventoryChangeNotifier.StockChanged -= Handler;
            }

            Assert.True(raised);
        }

        [Fact]
        public void OnFinishPurchase_ServiceFails_DoesNotRaiseInventoryChangedNotification()
        {
            var view = new FakePurchaseView { DocumentNumber = "001", SelectedSupplierId = 3 };
            var purchaseService = new FakePurchaseService { RegisterResult = false };
            var presenter = CreatePresenter(view, purchaseService, new FakeProductService());
            AddLine(presenter, view, productId: 1, amount: 1, pricePurchaseText: "10.00");

            bool raised = false;
            void Handler() => raised = true;
            InventoryChangeNotifier.StockChanged += Handler;
            try
            {
                presenter.OnFinishPurchase();
            }
            finally
            {
                InventoryChangeNotifier.StockChanged -= Handler;
            }

            Assert.False(raised);
        }

        [Fact]
        public void OnAddProduct_PushesTheVatBreakdownToTheView_PricesAreVatIncluded()
        {
            var view = new FakePurchaseView();
            var store = new FakeStoreService { ListStoreResult = new Store { defaultTaxRate = 19m } };
            var presenter = CreatePresenter(view, new FakePurchaseService(), new FakeProductService(), storeService: store);

            AddLine(presenter, view, productId: 1, amount: 1, pricePurchaseText: "1190.00");

            Assert.NotNull(view.VatBreakdown);
            Assert.Equal(1000m, view.VatBreakdown.Value.Net);   // 1190 / 1.19
            Assert.Equal(190m, view.VatBreakdown.Value.Tax);
            Assert.Equal(0m, view.VatBreakdown.Value.Exempt);
        }

        [Fact]
        public void OnFinishPurchase_StoresTheVatBreakdownAndRateOnThePurchase()
        {
            var view = new FakePurchaseView { DocumentType = "Factura" };
            var purchaseService = new FakePurchaseService { RegisterResult = true };
            var store = new FakeStoreService { ListStoreResult = new Store { defaultTaxRate = 19m } };
            var presenter = CreatePresenter(view, purchaseService, new FakeProductService(), storeService: store);
            AddLine(presenter, view, productId: 1, amount: 2, pricePurchaseText: "1190.00");

            view.DocumentNumber = "001";
            view.SelectedSupplierId = 3;
            presenter.OnFinishPurchase();

            Purchase registered = purchaseService.RegisteredWith;
            Assert.Equal(2380m, registered.totalAmount);   // gross unchanged: 2 * 1190
            Assert.Equal(2000m, registered.netAmount);
            Assert.Equal(380m, registered.taxAmount);
            Assert.Equal(0m, registered.exemptAmount);
            Assert.Equal(19m, registered.taxRate);
        }

        [Fact]
        public void OnFinishPurchase_ExemptProduct_GoesToExemptAmountNotTheTaxableBase()
        {
            var view = new FakePurchaseView { DocumentType = "Factura" };
            var purchaseService = new FakePurchaseService { RegisterResult = true };
            var store = new FakeStoreService { ListStoreResult = new Store { defaultTaxRate = 19m } };
            var productService = new FakeProductService
            {
                ListResult = new List<Product>
                {
                    new Product { idProduct = 1, code = "P1", name = "Product 1", taxAffected = true },
                    new Product { idProduct = 2, code = "P2", name = "Product 2", taxAffected = false }
                }
            };
            var presenter = CreatePresenter(view, purchaseService, productService, storeService: store);
            AddLine(presenter, view, productId: 1, amount: 1, pricePurchaseText: "1190.00");
            AddLine(presenter, view, productId: 2, amount: 1, pricePurchaseText: "500.00");

            view.DocumentNumber = "001";
            view.SelectedSupplierId = 3;
            presenter.OnFinishPurchase();

            Purchase registered = purchaseService.RegisteredWith;
            Assert.Equal(1690m, registered.totalAmount);
            Assert.Equal(1000m, registered.netAmount);
            Assert.Equal(190m, registered.taxAmount);
            Assert.Equal(500m, registered.exemptAmount);
        }

        [Fact]
        public void OnFinishPurchase_Succeeds_ClearsThePresenterOwnedCartToo()
        {
            var view = new FakePurchaseView { DocumentNumber = "001", SelectedSupplierId = 3 };
            var purchaseService = new FakePurchaseService { RegisterResult = true };
            var presenter = CreatePresenter(view, purchaseService, new FakeProductService());
            AddLine(presenter, view, productId: 1, amount: 1, pricePurchaseText: "10.00");

            presenter.OnFinishPurchase();

            // A second finish attempt with nothing re-added must behave as an empty cart again -
            // proves the cart was actually cleared, not just the grid the View renders.
            view.ShownMessages.Clear();
            presenter.OnFinishPurchase();

            Assert.Equal(new[] { "Debe ingresar un producto como minimo\npara registrar una compra" }, view.ShownMessages);
        }
    }
}
