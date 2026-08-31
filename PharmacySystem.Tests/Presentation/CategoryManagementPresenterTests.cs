using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using Xunit;

namespace PharmacySystem.Tests.Presentation
{
    public class CategoryManagementPresenterTests
    {
        private static CategoryManagementPresenter CreatePresenter(FakeCategoryManagementView view, FakeCategoryService service)
            => new CategoryManagementPresenter(view, service, TestUser.With("categorias.gestionar"), new FakeSecurityAudit());

        [Fact]
        public void OnSave_And_OnDelete_AreAudited()
        {
            var audit = new FakeSecurityAudit();
            var createView = new FakeCategoryManagementView { CategoryId = 0, Description = "Analgésicos" };
            new CategoryManagementPresenter(createView, new FakeCategoryService { RegisterResult = 7 }, TestUser.With("categorias.gestionar"), audit).OnSave();

            var deleteView = new FakeCategoryManagementView { SelectedIndex = 2, CategoryId = 7, Description = "Analgésicos" };
            new CategoryManagementPresenter(deleteView, new FakeCategoryService { DeleteResult = true }, TestUser.With("categorias.gestionar"), audit).OnDelete();

            Assert.Equal(new[] { "category.create", "category.delete" }, audit.Recorded.Select(e => e.Action));
            Assert.Equal(7, audit.Recorded[0].EntityId);
            Assert.Contains("Analgésicos", audit.Recorded[0].Summary);
        }

        [Fact]
        public void OnSave_WithoutManagePermission_ShowsDeniedAndDoesNotRegister()
        {
            var view = new FakeCategoryManagementView { CategoryId = 0, Description = "Analgesicos" };
            new CategoryManagementPresenter(view, new FakeCategoryService(), TestUser.With(), new FakeSecurityAudit()).OnSave();

            Assert.Contains(view.ShownMessages, m => m.Contains("No tiene permiso"));
            Assert.Empty(view.AddedRows);
        }

        [Fact]
        public void OnDelete_WithoutManagePermission_ShowsDeniedAndDoesNotRemove()
        {
            var view = new FakeCategoryManagementView { SelectedIndex = 2, CategoryId = 9 };
            new CategoryManagementPresenter(view, new FakeCategoryService(), TestUser.With(), new FakeSecurityAudit()).OnDelete();

            Assert.Contains(view.ShownMessages, m => m.Contains("No tiene permiso"));
            Assert.Empty(view.RemovedIndexes);
        }

        [Fact]
        public void OnLoad_PopulatesViewFromService()
        {
            var view = new FakeCategoryManagementView();
            var service = new FakeCategoryService
            {
                ListResult = new List<Categories> { new Categories { IdCategory = 1, description = "Analgésicos" } }
            };

            CreatePresenter(view, service).OnLoad();

            Assert.Single(view.LoadedCategories);
            Assert.Equal("Analgésicos", view.LoadedCategories[0].Description);
        }

        [Fact]
        public void OnSave_ValidationErrors_ShowsThemAndNeverCallsService()
        {
            var view = new FakeCategoryManagementView { ValidationErrors = new List<string> { "La descripción es requerida" } };
            var service = new FakeCategoryService();

            CreatePresenter(view, service).OnSave();

            Assert.Equal(new List<string> { "La descripción es requerida" }, view.ShownValidationErrors);
            Assert.Empty(view.AddedRows);
        }

        [Fact]
        public void OnSave_NewCategory_Succeeds_AddsRowRefreshesProductComboAndClearsForm()
        {
            var view = new FakeCategoryManagementView { CategoryId = 0, Description = " Antibióticos " };
            var service = new FakeCategoryService
            {
                RegisterResult = 5,
                ListResult = new List<Categories> { new Categories { IdCategory = 5, description = "Antibióticos" } }
            };

            CreatePresenter(view, service).OnSave();

            Assert.Single(view.AddedRows);
            Assert.Equal(5, view.AddedRows[0].Id);
            Assert.NotNull(view.RefreshedProductCategoryOptions);
            Assert.True(view.ClearFormCalled);
            Assert.Empty(view.ShownMessages);
        }

        [Fact]
        public void OnSave_NewCategory_Fails_ShowsMessageAndDoesNotAddRow()
        {
            var view = new FakeCategoryManagementView { CategoryId = 0 };
            var service = new FakeCategoryService { RegisterResult = 0 };

            CreatePresenter(view, service).OnSave();

            Assert.Equal(new[] { "No se pudo guardar los cambios\nRevise los datos" }, view.ShownMessages);
            Assert.Empty(view.AddedRows);
            Assert.False(view.ClearFormCalled);
        }

        [Fact]
        public void OnSave_ExistingCategory_UpdateFails_ShowsMessage()
        {
            var view = new FakeCategoryManagementView { CategoryId = 3, SelectedIndex = 1 };
            var service = new FakeCategoryService { UpdateResult = false };

            CreatePresenter(view, service).OnSave();

            Assert.Equal(new[] { "No se pudo guardar los cambios\nRevise los datos" }, view.ShownMessages);
            Assert.Empty(view.ReplacedRows);
        }

        [Fact]
        public void OnSave_ExistingCategory_UpdateSucceeds_ReplacesRowAndClearsForm()
        {
            var view = new FakeCategoryManagementView { CategoryId = 3, SelectedIndex = 2, Description = "Actualizada" };
            var service = new FakeCategoryService
            {
                UpdateResult = true,
                ListResult = new List<Categories> { new Categories { IdCategory = 3, description = "Actualizada" } }
            };

            CreatePresenter(view, service).OnSave();

            Assert.Single(view.ReplacedRows);
            Assert.Equal(1, view.ReplacedRows[0].Index); // SelectedIndex (1-based) - 1
            Assert.NotNull(view.RefreshedProductCategoryOptions);
            Assert.True(view.ClearFormCalled);
        }

        [Fact]
        public void OnDelete_NoSelection_DoesNothingSilently()
        {
            var view = new FakeCategoryManagementView { SelectedIndex = 0 };
            var service = new FakeCategoryService();

            CreatePresenter(view, service).OnDelete();

            Assert.Empty(view.ShownMessages);
            Assert.Empty(view.RemovedIndexes);
        }

        [Fact]
        public void OnDelete_UserCancelsConfirmation_NeverCallsService()
        {
            var view = new FakeCategoryManagementView { SelectedIndex = 1, ConfirmDeleteResult = false };
            var service = new FakeCategoryService();

            CreatePresenter(view, service).OnDelete();

            Assert.Empty(view.RemovedIndexes);
        }

        [Fact]
        public void OnDelete_ServiceFails_ShowsMessageAndDoesNotRemoveRow()
        {
            var view = new FakeCategoryManagementView { SelectedIndex = 1 };
            var service = new FakeCategoryService { DeleteResult = false };

            CreatePresenter(view, service).OnDelete();

            Assert.Equal(new[] { "No se pudo eliminar el registro\nRevise los datos" }, view.ShownMessages);
            Assert.Empty(view.RemovedIndexes);
        }

        [Fact]
        public void OnDelete_Succeeds_RemovesRowRefreshesProductComboAndClearsForm()
        {
            var view = new FakeCategoryManagementView { SelectedIndex = 3 };
            var service = new FakeCategoryService { DeleteResult = true };

            CreatePresenter(view, service).OnDelete();

            Assert.Equal(new[] { 2 }, view.RemovedIndexes); // SelectedIndex (1-based) - 1
            Assert.NotNull(view.RefreshedProductCategoryOptions);
            Assert.True(view.ClearFormCalled);
        }
    }
}
