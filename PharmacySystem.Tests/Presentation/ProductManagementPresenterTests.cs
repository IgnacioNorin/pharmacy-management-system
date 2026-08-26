using System;
using System.Collections.Generic;
using System.Globalization;
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
            => new ProductManagementPresenter(view, productService, categoryService);

        [Fact]
        public void OnLoad_PopulatesCategoryOptionsAndProducts()
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
            var categoryService = new FakeCategoryService();

            WithInvariantCulture(() => CreatePresenter(view, productService, categoryService).OnLoad());

            Assert.Equal("", view.LoadedProducts[0].ExpirationDateText);
        }

        [Fact]
        public void OnSave_ValidationErrors_ShowsThemAndNeverCallsService()
        {
            var view = new FakeProductManagementView { ValidationErrors = new List<string> { "El código es requerido" } };
            var productService = new FakeProductService();
            var categoryService = new FakeCategoryService();

            CreatePresenter(view, productService, categoryService).OnSave();

            Assert.Equal(new List<string> { "El código es requerido" }, view.ShownValidationErrors);
            Assert.Empty(view.AddedRows);
        }

        [Fact]
        public void OnSave_NewProduct_Succeeds_AddsRowAndClearsForm()
        {
            var view = new FakeProductManagementView
            {
                ProductId = 0,
                Code = "P2",
                Name = "Ibuprofeno",
                Description = "Antiinflamatorio",
                SelectedCategoryId = 2,
                SelectedCategoryText = "Antiinflamatorios"
            };
            var productService = new FakeProductService { RegisterResult = 7 };
            var categoryService = new FakeCategoryService();

            CreatePresenter(view, productService, categoryService).OnSave();

            Assert.Single(view.AddedRows);
            Assert.Equal(7, view.AddedRows[0].Id);
            Assert.Equal("0", view.AddedRows[0].Stock);
            Assert.True(view.ClearFormCalled);
        }

        [Fact]
        public void OnSave_NewProduct_Fails_DoesNothingSilently()
        {
            var view = new FakeProductManagementView { ProductId = 0 };
            var productService = new FakeProductService { RegisterResult = 0 };
            var categoryService = new FakeCategoryService();

            CreatePresenter(view, productService, categoryService).OnSave();

            Assert.Empty(view.AddedRows);
            Assert.False(view.ClearFormCalled);
            Assert.Empty(view.ShownMessages);
        }

        [Fact]
        public void OnSave_ExistingProduct_UpdateFails_DoesNothingSilently()
        {
            var view = new FakeProductManagementView { ProductId = 4, SelectedIndex = 1 };
            var productService = new FakeProductService { UpdateResult = false };
            var categoryService = new FakeCategoryService();

            CreatePresenter(view, productService, categoryService).OnSave();

            Assert.Empty(view.ReplacedRows);
            Assert.False(view.ClearFormCalled);
            Assert.Empty(view.ShownMessages);
        }

        [Fact]
        public void OnSave_ExistingProduct_UpdateSucceeds_ReplacesRowWithoutStockAndClearsForm()
        {
            var view = new FakeProductManagementView { ProductId = 4, SelectedIndex = 2, Name = "Actualizado" };
            var productService = new FakeProductService { UpdateResult = true };
            var categoryService = new FakeCategoryService();

            CreatePresenter(view, productService, categoryService).OnSave();

            Assert.Single(view.ReplacedRows);
            Assert.Equal(1, view.ReplacedRows[0].Index); // SelectedIndex (1-based) - 1
            Assert.Null(view.ReplacedRows[0].Row.Stock);
            Assert.Null(view.ReplacedRows[0].Row.ExpirationDateText);
            Assert.True(view.ClearFormCalled);
        }

        [Fact]
        public void OnDelete_NoSelection_NeverCallsService()
        {
            var view = new FakeProductManagementView { SelectedIndex = 0 };
            var productService = new FakeProductService();
            var categoryService = new FakeCategoryService();

            CreatePresenter(view, productService, categoryService).OnDelete();

            Assert.Empty(view.RemovedIndexes);
        }

        [Fact]
        public void OnDelete_ServiceFails_ShowsMessageAndDoesNotRemoveRow()
        {
            var view = new FakeProductManagementView { SelectedIndex = 1 };
            var productService = new FakeProductService { DeleteResult = false };
            var categoryService = new FakeCategoryService();

            CreatePresenter(view, productService, categoryService).OnDelete();

            Assert.Equal(new[] { "No se pudo eliminar el registro\nRevise los datos" }, view.ShownMessages);
            Assert.Empty(view.RemovedIndexes);
        }

        [Fact]
        public void OnDelete_Succeeds_RemovesRowAndClearsForm()
        {
            var view = new FakeProductManagementView { SelectedIndex = 3 };
            var productService = new FakeProductService { DeleteResult = true };
            var categoryService = new FakeCategoryService();

            CreatePresenter(view, productService, categoryService).OnDelete();

            Assert.Equal(new[] { 2 }, view.RemovedIndexes); // SelectedIndex (1-based) - 1
            Assert.True(view.ClearFormCalled);
        }
    }
}
