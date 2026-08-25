using System.Collections.Generic;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using Xunit;

namespace PharmacySystem.Tests.Presentation
{
    public class PurchasePresenterTests
    {
        private static PurchasePresenter CreatePresenter(FakePurchaseView view, FakePurchaseService purchaseService, FakeProductService productService, int idPerson = 1)
            => new PurchasePresenter(view, purchaseService, productService, idPerson);

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
            Assert.Empty(view.CartLinesList);
        }

        [Fact]
        public void OnAddProduct_NoProductSelected_ShowsMessage()
        {
            var view = new FakePurchaseView { SelectedProductId = 0 };

            CreatePresenter(view, new FakePurchaseService(), new FakeProductService()).OnAddProduct();

            Assert.Equal(new[] { "Debe seleccionar un producto primero" }, view.ShownMessages);
            Assert.Empty(view.CartLinesList);
        }

        [Fact]
        public void OnAddProduct_InvalidPurchasePrice_ShowsMessage()
        {
            var view = new FakePurchaseView { SelectedProductId = 1, PricePurchaseText = "not-a-number", PriceSaleText = "5.00" };

            CreatePresenter(view, new FakePurchaseService(), new FakeProductService()).OnAddProduct();

            Assert.Equal(new[] { "Error al convertir el tipo de moneda - Precio Compra\nEjemplo Formato ##.##" }, view.ShownMessages);
            Assert.Empty(view.CartLinesList);
        }

        [Fact]
        public void OnAddProduct_InvalidSalePrice_ShowsMessage()
        {
            var view = new FakePurchaseView { SelectedProductId = 1, PricePurchaseText = "5.00", PriceSaleText = "not-a-number" };

            CreatePresenter(view, new FakePurchaseService(), new FakeProductService()).OnAddProduct();

            Assert.Equal(new[] { "Error al convertir el tipo de moneda - Precio Venta\nEjemplo Formato ##.##" }, view.ShownMessages);
            Assert.Empty(view.CartLinesList);
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
                PricePurchaseText = "2.00",
                PriceSaleText = "5.00"
            };

            CreatePresenter(view, new FakePurchaseService(), new FakeProductService()).OnAddProduct();

            Assert.Single(view.CartLinesList);
            Assert.Equal(1, view.CartLinesList[0].ProductId);
            Assert.Equal(6m, view.CartLinesList[0].SubTotal); // 3 * 2.00
            Assert.True(view.ProductEntryCleared);
            Assert.NotNull(view.TotalText);
        }

        [Fact]
        public void OnAddProduct_ProductAlreadyInCart_DoesNothingSilently()
        {
            var view = new FakePurchaseView
            {
                SelectedProductId = 1,
                PricePurchaseText = "2.00",
                PriceSaleText = "5.00",
                CartLinesList = new List<PurchaseCartLine> { new PurchaseCartLine { ProductId = 1 } }
            };

            CreatePresenter(view, new FakePurchaseService(), new FakeProductService()).OnAddProduct();

            Assert.Single(view.CartLinesList); // unchanged
            Assert.Empty(view.ShownMessages);
            Assert.False(view.ProductEntryCleared);
        }

        [Fact]
        public void OnRemoveProduct_RemovesLineAndRecalculatesTotal()
        {
            var view = new FakePurchaseView
            {
                CartLinesList = new List<PurchaseCartLine>
                {
                    new PurchaseCartLine { ProductId = 1, SubTotal = 10m },
                    new PurchaseCartLine { ProductId = 2, SubTotal = 20m }
                }
            };

            CreatePresenter(view, new FakePurchaseService(), new FakeProductService()).OnRemoveProduct(0);

            Assert.Single(view.CartLinesList);
            Assert.Equal(2, view.CartLinesList[0].ProductId);
            Assert.NotNull(view.TotalText);
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
            var view = new FakePurchaseView
            {
                DocumentNumber = " 001 ",
                SelectedSupplierId = 3,
                DocumentType = "Factura",
                CartLinesList = new List<PurchaseCartLine>
                {
                    new PurchaseCartLine { ProductId = 1, Quantity = 2, PurchasePrice = 5m, SalePrice = 8m, SubTotal = 10m }
                }
            };
            var purchaseService = new FakePurchaseService { RegisterResult = true };

            CreatePresenter(view, purchaseService, new FakeProductService(), idPerson: 42).OnFinishPurchase();

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
            var view = new FakePurchaseView
            {
                DocumentNumber = "001",
                SelectedSupplierId = 3,
                CartLinesList = new List<PurchaseCartLine> { new PurchaseCartLine { ProductId = 1, SubTotal = 10m } }
            };
            var purchaseService = new FakePurchaseService { RegisterResult = false };

            CreatePresenter(view, purchaseService, new FakeProductService()).OnFinishPurchase();

            Assert.Equal(new[] { "No se pudo registrar la compra" }, view.ShownMessages);
            Assert.False(view.PurchaseCleared);
        }
    }
}
