using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using Xunit;

namespace PharmacySystem.Tests.Presentation
{
    public class ProductPricePresenterTests
    {
        private static ProductPricePresenter Create(FakeProductPriceView view, FakeProductService productService, params string[] permissions)
            => new ProductPricePresenter(view, productService, TestUser.With(permissions.Length == 0 ? new[] { "productos.editar_precios" } : permissions));

        private static Product Prod(int id, string code, int stock, bool released, decimal cost = 0m, decimal salePrice = 0m, decimal averageCost = 0m) => new Product
        {
            idProduct = id,
            code = code,
            name = "P" + id,
            stock = stock,
            isReleased = released,
            purchasePrice = cost,
            averageCost = averageCost,
            salePrice = salePrice,
            oCategory = new Categories { description = "cat" }
        };

        [Fact]
        public void OnLoad_SplitsProductsIntoReleasableAndCommercialized()
        {
            var view = new FakeProductPriceView();
            var service = new FakeProductService
            {
                ListResult = new List<Product>
                {
                    Prod(1, "A", stock: 5, released: false, cost: 10m),   // in stock, not released -> releasable
                    Prod(2, "B", stock: 0, released: false),             // no stock -> hidden
                    Prod(3, "C", stock: 3, released: true, cost: 6m, salePrice: 10m) // released -> commercialized
                }
            };

            Create(view, service).OnLoad();

            Assert.Equal(new[] { 1 }, view.Releasable.Select(r => r.Id));
            Assert.Equal(new[] { 3 }, view.Commercialized.Select(r => r.Id));
            Assert.Null(view.Releasable[0].SalePrice);
            Assert.Equal(40m, view.Commercialized[0].MarginPercent); // (10 - 6) / 10 * 100
        }

        [Fact]
        public void OnLoad_MarginUsesTheWeightedAverageCostWhenItIsSet()
        {
            var view = new FakeProductPriceView();
            var service = new FakeProductService
            {
                ListResult = new List<Product>
                {
                    // last purchase price 6, but the moving average is 8 -> margin is over 8
                    Prod(1, "A", stock: 3, released: true, cost: 6m, salePrice: 10m, averageCost: 8m)
                }
            };

            Create(view, service).OnLoad();

            Assert.Equal(8m, view.Commercialized[0].Cost);
            Assert.Equal(20m, view.Commercialized[0].MarginPercent); // (10 - 8) / 10 * 100
        }

        [Fact]
        public void OnApplyPrice_ValidPrice_CallsSetSalePriceWithReasonAndUser_ThenReloads()
        {
            var view = new FakeProductPriceView { SelectedProductId = 7, NewPriceText = "1500", Reason = "lanzamiento" };
            var service = new FakeProductService();

            Create(view, service).OnApplyPrice();

            Assert.Equal(7, service.SetSalePriceCall.Value.Id);
            Assert.Equal(1500m, service.SetSalePriceCall.Value.Price);
            Assert.Equal("lanzamiento", service.SetSalePriceCall.Value.Reason);
            Assert.Equal(1, service.SetSalePriceCall.Value.UserId);   // TestUser person id
            Assert.True(view.EntryCleared);
            Assert.NotNull(view.Releasable); // reloaded
        }

        [Fact]
        public void OnApplyPrice_NoProductSelected_ShowsMessageAndDoesNotCallService()
        {
            var view = new FakeProductPriceView { SelectedProductId = 0, NewPriceText = "100" };
            var service = new FakeProductService();

            Create(view, service).OnApplyPrice();

            Assert.Null(service.SetSalePriceCall);
            Assert.Contains(view.ShownMessages, m => m.Contains("Seleccione"));
        }

        [Theory]
        [InlineData("")]
        [InlineData("abc")]
        [InlineData("0")]
        [InlineData("-5")]
        public void OnApplyPrice_InvalidOrNonPositivePrice_ShowsValidationErrorAndDoesNotCallService(string priceText)
        {
            var view = new FakeProductPriceView { SelectedProductId = 7, NewPriceText = priceText };
            var service = new FakeProductService();

            Create(view, service).OnApplyPrice();

            Assert.Null(service.SetSalePriceCall);
            Assert.NotNull(view.ShownValidationErrors);
        }

        [Fact]
        public void OnApplyPrice_WithoutPermission_IsBlocked()
        {
            var view = new FakeProductPriceView { SelectedProductId = 7, NewPriceText = "100" };
            var service = new FakeProductService();

            new ProductPricePresenter(view, service, TestUser.With()).OnApplyPrice();

            Assert.Null(service.SetSalePriceCall);
            Assert.Contains(view.ShownMessages, m => m.Contains("permiso"));
        }

        [Fact]
        public void OnApplyPrice_ServiceFails_ShowsMessageAndDoesNotClear()
        {
            var view = new FakeProductPriceView { SelectedProductId = 7, NewPriceText = "100" };
            var service = new FakeProductService { SetSalePriceResult = false };

            Create(view, service).OnApplyPrice();

            Assert.False(view.EntryCleared);
            Assert.Contains(view.ShownMessages, m => m.Contains("No se pudo"));
        }

        [Fact]
        public void OnUnrelease_CallsServiceAndReloads()
        {
            var view = new FakeProductPriceView { SelectedProductId = 9, Reason = "discontinuado" };
            var service = new FakeProductService();

            Create(view, service).OnUnrelease();

            Assert.Equal(9, service.UnreleaseCall.Value.Id);
            Assert.Equal("discontinuado", service.UnreleaseCall.Value.Reason);
            Assert.True(view.EntryCleared);
        }

        [Fact]
        public void OnSelectProduct_LoadsHistoryForThatProduct()
        {
            var view = new FakeProductPriceView();
            var service = new FakeProductService
            {
                PriceHistoryResult = new List<ProductPriceHistoryEntry>
                {
                    new ProductPriceHistoryEntry { EventType = "liberacion", SalePrice = 100m, Cost = 60m, UserName = "Ana", Reason = "alta" }
                }
            };

            Create(view, service).OnSelectProduct(42);

            Assert.Equal(42, service.PriceHistoryRequestedFor);
            Assert.Single(view.History);
            Assert.Equal("Liberación", view.History[0].EventText);
        }
    }
}
