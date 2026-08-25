using System.Collections.Generic;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using Xunit;

namespace PharmacySystem.Tests.Presentation
{
    public class SalePresenterTests
    {
        private static SalePresenter CreatePresenter(FakeSaleView view, FakeSaleService saleService, FakeProductService productService, int idPerson = 1)
            => new SalePresenter(view, saleService, productService, idPerson);

        // The cart lives inside the Presenter now, not the View, so tests that need an existing
        // cart line build it through the real OnAddProduct() path instead of pre-seeding view state.
        private static void AddLine(SalePresenter presenter, FakeSaleView view, int productId, decimal amount, string priceSaleText, int stock = 100)
        {
            view.SelectedProductId = productId;
            view.SelectedProductName = "Product " + productId;
            view.Stock = stock;
            view.Amount = amount;
            view.PriceSaleText = priceSaleText;
            presenter.OnAddProduct();
        }

        [Fact]
        public void OnProductCodeEntered_KnownCode_SelectsProduct()
        {
            var view = new FakeSaleView();
            var productService = new FakeProductService
            {
                ListResult = new List<Product> { new Product { idProduct = 5, code = "P1", name = "Paracetamol", stock = 20, salePrice = 3.5m } }
            };

            CreatePresenter(view, new FakeSaleService(), productService).OnProductCodeEntered("P1");

            Assert.Equal(5, view.SelectedProductSetTo.Value.Id);
            Assert.Equal(20, view.SelectedProductSetTo.Value.Stock);
        }

        [Fact]
        public void OnAddProduct_NoProductSelected_ShowsMessage()
        {
            var view = new FakeSaleView { SelectedProductId = 0 };

            CreatePresenter(view, new FakeSaleService(), new FakeProductService()).OnAddProduct();

            Assert.Equal(new[] { "Debe seleccionar un producto primero" }, view.ShownMessages);
            Assert.Empty(view.RenderedCartLines);
        }

        [Fact]
        public void OnAddProduct_InsufficientStock_ShowsMessage()
        {
            var view = new FakeSaleView { SelectedProductId = 1, Stock = 2, Amount = 5 };

            CreatePresenter(view, new FakeSaleService(), new FakeProductService()).OnAddProduct();

            Assert.Equal(new[] { "No hay suficiente stock del producto" }, view.ShownMessages);
            Assert.Empty(view.RenderedCartLines);
        }

        [Fact]
        public void OnAddProduct_InvalidSalePrice_ShowsMessage()
        {
            var view = new FakeSaleView { SelectedProductId = 1, Stock = 10, Amount = 1, PriceSaleText = "not-a-number" };

            CreatePresenter(view, new FakeSaleService(), new FakeProductService()).OnAddProduct();

            Assert.Equal(new[] { "Error al convertir el tipo de moneda - Precio Venta\nEjemplo Formato ##.##" }, view.ShownMessages);
            Assert.Empty(view.RenderedCartLines);
        }

        [Fact]
        public void OnAddProduct_ValidEntry_AddsLineRecalculatesTotalAndClearsEntry()
        {
            var view = new FakeSaleView { SelectedProductId = 1, SelectedProductName = "Paracetamol", Stock = 10, Amount = 3, PriceSaleText = "2.00" };

            CreatePresenter(view, new FakeSaleService(), new FakeProductService()).OnAddProduct();

            Assert.Single(view.RenderedCartLines);
            Assert.Equal(6m, view.RenderedCartLines[0].SubTotal);
            Assert.True(view.ProductEntryCleared);
            Assert.NotNull(view.TotalText);
        }

        [Fact]
        public void OnAddProduct_ProductAlreadyInCart_ShowsMessageAndDoesNotAdd()
        {
            var view = new FakeSaleView();
            var presenter = CreatePresenter(view, new FakeSaleService(), new FakeProductService());
            AddLine(presenter, view, productId: 1, amount: 1, priceSaleText: "2.00");

            view.SelectedProductId = 1;
            view.Stock = 10;
            view.Amount = 1;
            view.PriceSaleText = "2.00";
            presenter.OnAddProduct();

            Assert.Equal(new[] { "El producto ya fue agregado\nElimínelo e ingrese el nuevo si quiere cambiar la cantidad." }, view.ShownMessages);
            Assert.Single(view.RenderedCartLines);
        }

        [Fact]
        public void OnRemoveProduct_RemovesLineAndRecalculatesTotal()
        {
            var view = new FakeSaleView();
            var presenter = CreatePresenter(view, new FakeSaleService(), new FakeProductService());
            AddLine(presenter, view, productId: 1, amount: 1, priceSaleText: "10.00");
            AddLine(presenter, view, productId: 2, amount: 1, priceSaleText: "20.00");

            presenter.OnRemoveProduct(0);

            Assert.Single(view.RenderedCartLines);
            Assert.Equal(2, view.RenderedCartLines[0].ProductId);
        }

        // Regression test: CalculateChange() used to parse "Paga con" with Convert.ToDecimal
        // instead of the culture-aware converter used for totalPay right next to it, and never
        // actually returned false, so this error message was unreachable dead code. Now a bad
        // "Paga con" value is caught and reported like every other currency-parsing error on this
        // screen.
        [Fact]
        public void OnCalculateChangeRequested_InvalidPayWith_ShowsMessage()
        {
            var view = new FakeSaleView { PayWithText = "not-a-number", TotalPayText = "10.00" };

            CreatePresenter(view, new FakeSaleService(), new FakeProductService()).OnCalculateChangeRequested();

            Assert.Equal(new[] { "Error al convertir el tipo de moneda - Paga con\nEjemplo Formato ##.##" }, view.ShownMessages);
            Assert.Null(view.ChangeTextSet);
        }

        [Fact]
        public void OnCalculateChangeRequested_ValidPayWith_SetsChangeText()
        {
            var view = new FakeSaleView { PayWithText = "15.00", TotalPayText = "10.00" };

            CreatePresenter(view, new FakeSaleService(), new FakeProductService()).OnCalculateChangeRequested();

            Assert.Empty(view.ShownMessages);
            Assert.NotNull(view.ChangeTextSet);
        }

        [Fact]
        public void OnFinishSale_InvalidPayWith_ShowsMessage()
        {
            var view = new FakeSaleView { DocumentClient = "123", NameClient = "Juan" };
            var presenter = CreatePresenter(view, new FakeSaleService(), new FakeProductService());
            AddLine(presenter, view, productId: 1, amount: 1, priceSaleText: "10.00");

            view.PayWithText = "not-a-number";
            view.TotalPayText = "10.00";
            presenter.OnFinishSale();

            Assert.Equal(new[] { "Error al convertir el tipo de moneda - Paga con\nEjemplo Formato ##.##" }, view.ShownMessages);
        }

        [Fact]
        public void OnFinishSale_MissingClientData_ShowsMessage()
        {
            var view = new FakeSaleView { DocumentClient = "", NameClient = "" };

            CreatePresenter(view, new FakeSaleService(), new FakeProductService()).OnFinishSale();

            Assert.Equal(new[] { "Debe ingresar todos los datos del cliente" }, view.ShownMessages);
        }

        [Fact]
        public void OnFinishSale_EmptyCart_ShowsMessage()
        {
            var view = new FakeSaleView { DocumentClient = "123", NameClient = "Juan" };

            CreatePresenter(view, new FakeSaleService(), new FakeProductService()).OnFinishSale();

            Assert.Equal(new[] { "Debe ingresar un producto como minimo\npara registrar una venta" }, view.ShownMessages);
        }

        [Fact]
        public void OnFinishSale_NoPayment_ShowsMessage()
        {
            var view = new FakeSaleView { DocumentClient = "123", NameClient = "Juan" };
            var presenter = CreatePresenter(view, new FakeSaleService(), new FakeProductService());
            AddLine(presenter, view, productId: 1, amount: 1, priceSaleText: "10.00");

            view.PayWithText = "0";
            presenter.OnFinishSale();

            Assert.Equal(new[] { "Debe ingresar con cuanto paga el cliente" }, view.ShownMessages);
        }

        [Fact]
        public void OnFinishSale_NotEnoughMoney_ShowsMessage()
        {
            var view = new FakeSaleView { DocumentClient = "123", NameClient = "Juan" };
            var presenter = CreatePresenter(view, new FakeSaleService(), new FakeProductService());
            AddLine(presenter, view, productId: 1, amount: 1, priceSaleText: "10.00");

            view.PayWithText = "5.00";
            view.TotalPayText = "10.00";
            presenter.OnFinishSale();

            Assert.Equal(new[] { "Falta dinero para pagar" }, view.ShownMessages);
        }

        // Regression test: OnFinishSale used to run an extra ControlStock() check against the
        // product-entry fields (SelectedProductId/Amount) before touching the cart, and those are
        // "0"/1 after the last CleanProduct(). A real cashier hits this on every sale - after
        // adding their last item, the entry fields reset, yet the sale must still register. Fixed
        // by removing that check; only the per-line ControlStock in the loop below gates the sale now.
        [Fact]
        public void OnFinishSale_ProductEntryFieldsResetAfterLastAdd_StillRegistersSale()
        {
            var view = new FakeSaleView { DocumentClient = "123", NameClient = "Juan" };
            var saleService = new FakeSaleService { ControlStockResult = true, RegisterResult = 5 };
            var productService = new FakeProductService { VerifyResult = true };
            var presenter = CreatePresenter(view, saleService, productService);
            AddLine(presenter, view, productId: 1, amount: 1, priceSaleText: "10.00");

            // Mirrors CleanProduct() having reset the entry fields after the add above.
            view.SelectedProductId = 0;
            view.Amount = 1;
            view.PayWithText = "10.00";
            view.TotalPayText = "10.00";
            presenter.OnFinishSale();

            Assert.NotNull(saleService.RegisteredWith);
            Assert.True(view.SaleCleared);
            Assert.DoesNotContain(saleService.ControlStockCalls, c => c.IdProduct == 0);
        }

        [Fact]
        public void OnFinishSale_LineProductNoLongerExists_ShowsMessage()
        {
            var view = new FakeSaleView { DocumentClient = "123", NameClient = "Juan" };
            var saleService = new FakeSaleService { ControlStockResult = true };
            var productService = new FakeProductService { VerifyResult = false };
            var presenter = CreatePresenter(view, saleService, productService);
            AddLine(presenter, view, productId: 1, amount: 1, priceSaleText: "10.00");

            view.PayWithText = "10.00";
            view.TotalPayText = "10.00";
            presenter.OnFinishSale();

            Assert.Equal(new[] { "No se pudo registrar la venta\n Problema con producto" }, view.ShownMessages);
            Assert.Null(saleService.RegisteredWith);
        }

        [Fact]
        public void OnFinishSale_SubtractStockFails_ShowsMessage()
        {
            var view = new FakeSaleView { DocumentClient = "123", NameClient = "Juan" };
            var saleService = new FakeSaleService();
            var productService = new FakeProductService { VerifyResult = true };
            var presenter = CreatePresenter(view, saleService, productService);
            AddLine(presenter, view, productId: 1, amount: 1, priceSaleText: "10.00");

            saleService.ControlStockResult = false;
            view.PayWithText = "10.00";
            view.TotalPayText = "10.00";
            presenter.OnFinishSale();

            Assert.Equal(new[] { "No se pudo registrar la venta\n Problema con Stock" }, view.ShownMessages);
            Assert.Null(saleService.RegisteredWith);
        }

        [Fact]
        public void OnFinishSale_Succeeds_RegistersSaleClearsAndNotifiesView()
        {
            var view = new FakeSaleView { DocumentClient = "123", NameClient = "Juan", DocumentType = "Factura" };
            var saleService = new FakeSaleService { ControlStockResult = true, RegisterResult = 99 };
            var productService = new FakeProductService { VerifyResult = true };
            var presenter = CreatePresenter(view, saleService, productService, idPerson: 7);
            AddLine(presenter, view, productId: 1, amount: 2, priceSaleText: "5.00");

            view.DocumentClient = " 123 ";
            view.NameClient = " Juan ";
            view.PayWithText = "10.00";
            view.TotalPayText = "10.00";
            view.ChangeText = "0.00";
            presenter.OnFinishSale();

            Assert.NotNull(saleService.RegisteredWith);
            Assert.Equal("123", saleService.RegisteredWith.documentClient);
            Assert.Equal("Juan", saleService.RegisteredWith.nameClient);
            Assert.Equal(7, saleService.RegisteredWith.oPerson.idPerson);
            Assert.Single(saleService.RegisteredWith.oSaleDetail);
            Assert.True(view.SaleCleared);
            Assert.Equal(99, view.RegisteredSaleId);
        }

        [Fact]
        public void OnFinishSale_RegisterFails_ShowsMessage()
        {
            var view = new FakeSaleView { DocumentClient = "123", NameClient = "Juan" };
            var saleService = new FakeSaleService { ControlStockResult = true, RegisterResult = 0 };
            var productService = new FakeProductService { VerifyResult = true };
            var presenter = CreatePresenter(view, saleService, productService);
            AddLine(presenter, view, productId: 1, amount: 1, priceSaleText: "10.00");

            view.PayWithText = "10.00";
            view.TotalPayText = "10.00";
            presenter.OnFinishSale();

            Assert.Equal(new[] { "No se pudo registrar la venta" }, view.ShownMessages);
            Assert.False(view.SaleCleared);
        }

        [Fact]
        public void OnFinishSale_Succeeds_ClearsThePresenterOwnedCartToo()
        {
            var view = new FakeSaleView { DocumentClient = "123", NameClient = "Juan" };
            var saleService = new FakeSaleService { ControlStockResult = true, RegisterResult = 5 };
            var productService = new FakeProductService { VerifyResult = true };
            var presenter = CreatePresenter(view, saleService, productService);
            AddLine(presenter, view, productId: 1, amount: 1, priceSaleText: "10.00");

            view.PayWithText = "10.00";
            view.TotalPayText = "10.00";
            presenter.OnFinishSale();

            // A second finish attempt with nothing re-added must behave as an empty cart again -
            // proves the cart was actually cleared, not just the grid the View renders.
            view.ShownMessages.Clear();
            presenter.OnFinishSale();

            Assert.Equal(new[] { "Debe ingresar un producto como minimo\npara registrar una venta" }, view.ShownMessages);
        }
    }
}
