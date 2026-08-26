using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using Xunit;

namespace PharmacySystem.Tests.Presentation
{
    public class ProductPickerPresenterTests
    {
        private class FakeView : IProductPickerView
        {
            public List<ProductPickerRow> Loaded { get; private set; }
            public void LoadProducts(IEnumerable<ProductPickerRow> products) => Loaded = products.ToList();
        }

        private static FakeProductService ServiceWithTwoProducts() => new FakeProductService
        {
            ListResult = new List<Product>
            {
                new Product { idProduct = 1, name = "In stock", stock = 5, oCategory = new Categories() },
                new Product { idProduct = 2, name = "Out of stock", stock = 0, oCategory = new Categories() }
            }
        };

        [Fact]
        public void OnLoad_OriginFrmPurchase_IncludesAllProducts()
        {
            var view = new FakeView();
            var service = ServiceWithTwoProducts();

            new ProductPickerPresenter(view, service, "frmPurchase").OnLoad();

            Assert.Equal(2, view.Loaded.Count);
        }

        [Fact]
        public void OnLoad_OriginFrmSale_OnlyIncludesProductsWithStock()
        {
            var view = new FakeView();
            var service = ServiceWithTwoProducts();

            new ProductPickerPresenter(view, service, "frmSale").OnLoad();

            Assert.Single(view.Loaded);
            Assert.Equal("In stock", view.Loaded[0].Name);
        }

        [Fact]
        public void OnLoad_UnknownOrigin_LoadsNothing()
        {
            var view = new FakeView();
            var service = ServiceWithTwoProducts();

            new ProductPickerPresenter(view, service, "somethingElse").OnLoad();

            Assert.Empty(view.Loaded);
        }
    }
}
