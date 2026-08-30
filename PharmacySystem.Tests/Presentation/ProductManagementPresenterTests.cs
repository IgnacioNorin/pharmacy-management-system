using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using Xunit;

namespace PharmacySystem.Tests.Presentation
{
    // ToRow() formats expirationDate via DateTime.ToShortDateString(), which depends on the
    // running thread's culture, and compares it against a hardcoded "01/01/0001" (slash-separated)
    // to decide whether to blank it out - preserved verbatim from the original frmManagement.cs.
    // Pin the thread culture to Invariant (slash-separated, matching that hardcoded literal) so
    // these tests aren't at the mercy of whatever culture the machine running them defaults to.
    //
    // The grid is server-paged: OnLoad / OnSearch / the page-navigation methods all call
    // IProductService.ListPaged and repaint the whole grid, and a successful save or delete
    // reloads the current page.
    public class ProductManagementPresenterTests
    {
        private static void WithInvariantCulture(Action action)
        {
            var original = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            try
            {
                action();
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = original;
            }
        }

        private static ProductManagementPresenter CreatePresenter(FakeProductManagementView view, FakeProductService productService, FakeCategoryService categoryService)
            => new ProductManagementPresenter(view, productService, categoryService, TestUser.With("productos.gestionar", "productos.eliminar"));

        private static Product Prod(int id, string code, string name, string description = "") => new Product
        {
            idProduct = id,
            code = code,
            name = name,
            description = description,
            oCategory = new Categories { IdCategory = 1, description = "Cat" },
            stock = 1
        };

        private static List<Product> ManyProducts(int count) =>
            Enumerable.Range(1, count).Select(i => Prod(i, "P" + i, "Producto " + i)).ToList();

        [Fact]
        public void OnSave_WithoutManagePermission_ShowsDeniedAndDoesNotTouchTheGrid()
        {
            var view = new FakeProductManagementView { ProductId = 0, Code = "P", Name = "N", Description = "D", SelectedCategoryId = 1 };
            new ProductManagementPresenter(view, new FakeProductService(), new FakeCategoryService(), TestUser.With()).OnSave();

            Assert.Contains(view.ShownMessages, m => m.Contains("No tiene permiso"));
            Assert.Equal(0, view.LoadProductsCallCount);
        }

        [Fact]
        public void OnDelete_WithoutDeletePermission_ShowsDeniedAndDoesNotTouchTheGrid()
        {
            var view = new FakeProductManagementView { SelectedIndex = 2, ProductId = 5 };
            new ProductManagementPresenter(view, new FakeProductService(), new FakeCategoryService(), TestUser.With("productos.gestionar")).OnDelete();

            Assert.Contains(view.ShownMessages, m => m.Contains("No tiene permiso"));
            Assert.Equal(0, view.LoadProductsCallCount);
        }

        [Fact]
        public void OnLoad_PopulatesCategoryOptionsAndFirstPage()
        {
            var view = new FakeProductManagementView();
            var productService = new FakeProductService
            {
                ListResult = new List<Product>
                {
                    new Product
                    {
                        idProduct = 1, code = "P1", name = "Paracetamol", description = "Analgésico",
                        oCategory = new Categories { IdCategory = 1, description = "Analgésicos" },
                        stock = 10, expirationDate = new DateTime(2027, 1, 1)
                    }
                }
            };
            var categoryService = new FakeCategoryService
            {
                ListResult = new List<Categories> { new Categories { IdCategory = 1, description = "Analgésicos" } }
            };

            WithInvariantCulture(() => CreatePresenter(view, productService, categoryService).OnLoad());

            Assert.Single(view.LoadedCategoryOptions);
            Assert.Single(view.LoadedProducts);
            Assert.Equal("Paracetamol", view.LoadedProducts[0].Name);
            Assert.Equal("01/01/2027", view.LoadedProducts[0].ExpirationDateText);
            Assert.Equal((1, 1, 1), view.LastPageInfo);
            Assert.Equal(1, productService.LastPagedCall?.Page);
            Assert.Equal("", productService.LastPagedCall?.Search);
        }

        [Fact]
        public void OnLoad_ProductWithoutExpirationDate_ShowsBlankText()
        {
            var view = new FakeProductManagementView();
            var productService = new FakeProductService
            {
                ListResult = new List<Product>
                {
                    new Product
                    {
                        idProduct = 1, code = "P1", name = "Alcohol", description = "Antiséptico",
                        oCategory = new Categories { IdCategory = 1, description = "Insumos" },
                        stock = 5, expirationDate = default(DateTime)
                    }
                }
            };

            WithInvariantCulture(() => CreatePresenter(view, productService, new FakeCategoryService()).OnLoad());

            Assert.Equal("", view.LoadedProducts[0].ExpirationDateText);
        }

        [Fact]
        public void OnLoad_MoreThanOnePage_ReportsTotalsAndOnlyReturnsTheFirstPage()
        {
            var view = new FakeProductManagementView();
            var productService = new FakeProductService { ListResult = ManyProducts(120) };

            CreatePresenter(view, productService, new FakeCategoryService()).OnLoad();

            Assert.Equal(50, view.LoadedProducts.Count);
            Assert.Equal((1, 3, 120), view.LastPageInfo); // page 1 of 3, 120 rows
        }

        [Fact]
        public void OnNextPage_ThenOnPreviousPage_MoveOnePageAtATime_AndClampAtTheEnds()
        {
            var view = new FakeProductManagementView();
            var productService = new FakeProductService { ListResult = ManyProducts(120) };
            var presenter = CreatePresenter(view, productService, new FakeCategoryService());
            presenter.OnLoad();

            presenter.OnNextPage();
            Assert.Equal(2, view.LastPageInfo?.CurrentPage);

            presenter.OnNextPage();
            Assert.Equal(3, view.LastPageInfo?.CurrentPage);

            presenter.OnNextPage(); // already on the last page
            Assert.Equal(3, view.LastPageInfo?.CurrentPage);

            presenter.OnFirstPage();
            Assert.Equal(1, view.LastPageInfo?.CurrentPage);

            presenter.OnPreviousPage(); // already on the first page
            Assert.Equal(1, view.LastPageInfo?.CurrentPage);
        }

        [Fact]
        public void OnSearch_QueriesWithTheTermAndResetsToPageOne()
        {
            var view = new FakeProductManagementView();
            var productService = new FakeProductService
            {
                ListResult = ManyProducts(120).Concat(new[] { Prod(999, "ASPIRINA", "Aspirina") }).ToList()
            };
            var presenter = CreatePresenter(view, productService, new FakeCategoryService());
            presenter.OnLoad();
            presenter.OnNextPage(); // now on page 2

            view.SearchText = "Aspirina";
            presenter.OnSearch();

            Assert.Equal("Aspirina", productService.LastPagedCall?.Search);
            Assert.Equal(1, productService.LastPagedCall?.Page);
            Assert.Single(view.LoadedProducts);
            Assert.Equal((1, 1, 1), view.LastPageInfo);
        }

        [Fact]
        public void OnSave_ValidationErrors_ShowsThemAndNeverCallsService()
        {
            var view = new FakeProductManagementView { ValidationErrors = new List<string> { "El código es requerido" } };

            CreatePresenter(view, new FakeProductService(), new FakeCategoryService()).OnSave();

            Assert.Equal(new List<string> { "El código es requerido" }, view.ShownValidationErrors);
            Assert.Equal(0, view.LoadProductsCallCount);
        }

        [Fact]
        public void OnSave_NewProduct_Succeeds_ReloadsThePageAndClearsForm()
        {
            var view = new FakeProductManagementView
            {
                ProductId = 0, Code = "P2", Name = "Ibuprofeno", Description = "Antiinflamatorio",
                SelectedCategoryId = 2, SelectedCategoryText = "Antiinflamatorios"
            };
            var productService = new FakeProductService { RegisterResult = 7, ListResult = ManyProducts(3) };

            CreatePresenter(view, productService, new FakeCategoryService()).OnSave();

            Assert.True(view.ClearFormCalled);
            Assert.Equal(1, view.LoadProductsCallCount);
            Assert.Equal(3, view.LoadedProducts.Count);
        }

        [Fact]
        public void OnSave_NewProduct_Fails_DoesNothingSilently()
        {
            var view = new FakeProductManagementView { ProductId = 0 };
            var productService = new FakeProductService { RegisterResult = 0 };

            CreatePresenter(view, productService, new FakeCategoryService()).OnSave();

            Assert.Equal(0, view.LoadProductsCallCount);
            Assert.False(view.ClearFormCalled);
            Assert.Empty(view.ShownMessages);
        }

        [Fact]
        public void OnSave_ExistingProduct_UpdateFails_DoesNothingSilently()
        {
            var view = new FakeProductManagementView { ProductId = 4, SelectedIndex = 1 };
            var productService = new FakeProductService { UpdateResult = false };

            CreatePresenter(view, productService, new FakeCategoryService()).OnSave();

            Assert.Equal(0, view.LoadProductsCallCount);
            Assert.False(view.ClearFormCalled);
            Assert.Empty(view.ShownMessages);
        }

        [Fact]
        public void OnSave_ExistingProduct_UpdateSucceeds_ReloadsThePageAndClearsForm()
        {
            var view = new FakeProductManagementView { ProductId = 4, SelectedIndex = 2, Name = "Actualizado" };
            var productService = new FakeProductService { UpdateResult = true, ListResult = ManyProducts(2) };

            CreatePresenter(view, productService, new FakeCategoryService()).OnSave();

            Assert.Equal(1, view.LoadProductsCallCount);
            Assert.True(view.ClearFormCalled);
        }

        [Fact]
        public void OnDelete_NoSelection_NeverCallsService()
        {
            var view = new FakeProductManagementView { SelectedIndex = 0 };

            CreatePresenter(view, new FakeProductService(), new FakeCategoryService()).OnDelete();

            Assert.Equal(0, view.LoadProductsCallCount);
        }

        [Fact]
        public void OnDelete_ServiceFails_ShowsMessageAndDoesNotReload()
        {
            var view = new FakeProductManagementView { SelectedIndex = 1 };
            var productService = new FakeProductService { DeleteResult = false };

            CreatePresenter(view, productService, new FakeCategoryService()).OnDelete();

            Assert.Equal(new[] { "No se pudo eliminar el registro\nRevise los datos" }, view.ShownMessages);
            Assert.Equal(0, view.LoadProductsCallCount);
        }

        [Fact]
        public void OnDelete_Succeeds_ReloadsThePageAndClearsForm()
        {
            var view = new FakeProductManagementView { SelectedIndex = 3 };
            var productService = new FakeProductService { DeleteResult = true, ListResult = ManyProducts(4) };

            CreatePresenter(view, productService, new FakeCategoryService()).OnDelete();

            Assert.Equal(1, view.LoadProductsCallCount);
            Assert.True(view.ClearFormCalled);
        }

        [Fact]
        public void OnDelete_LastRowOfLastPage_FallsBackToTheNewLastPage()
        {
            var view = new FakeProductManagementView();
            var productService = new FakeProductService { DeleteResult = true, ListResult = ManyProducts(51) };
            var presenter = CreatePresenter(view, productService, new FakeCategoryService());
            presenter.OnLoad();     // page 1 of 2 (50 + 1)
            presenter.OnNextPage(); // page 2 of 2, a single row
            Assert.Equal(2, view.LastPageInfo?.CurrentPage);

            productService.ListResult = ManyProducts(50); // that row is gone
            view.SelectedIndex = 1;
            presenter.OnDelete();

            Assert.Equal(1, view.LastPageInfo?.CurrentPage); // page 2 no longer exists
            Assert.Equal((1, 1, 50), view.LastPageInfo);
        }
    }
}
