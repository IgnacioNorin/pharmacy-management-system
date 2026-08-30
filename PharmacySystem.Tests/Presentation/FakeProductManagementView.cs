using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Model;
using PharmacySystem.Presentation;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakeProductManagementView : IProductManagementView
    {
        public int SelectedIndex { get; set; }
        public int RowCount { get; set; }
        public int ProductId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int SelectedCategoryId { get; set; }
        public string SelectedCategoryText { get; set; }
        public bool TaxAffected { get; set; } = true;
        public string SearchText { get; set; } = "";
        public List<string> ValidationErrors { get; set; } = new List<string>();
        public bool ConfirmDeleteResult { get; set; } = true;

        List<string> IProductManagementView.Validate() => ValidationErrors;
        public bool ConfirmDelete() => ConfirmDeleteResult;

        public List<ComboBoxItem> LoadedCategoryOptions { get; private set; }
        public List<ManagementProductRow> LoadedProducts { get; private set; }
        public int LoadProductsCallCount { get; private set; }
        public (int CurrentPage, int TotalPages, int TotalCount)? LastPageInfo { get; private set; }
        public bool ClearFormCalled { get; private set; }
        public List<string> ShownMessages { get; } = new List<string>();
        public List<string> ShownValidationErrors { get; private set; }

        public void LoadCategoryOptions(IEnumerable<ComboBoxItem> options) => LoadedCategoryOptions = options.ToList();

        public void LoadProducts(IEnumerable<ManagementProductRow> products)
        {
            LoadedProducts = products.ToList();
            LoadProductsCallCount++;
        }

        public void SetPageInfo(int currentPage, int totalPages, int totalCount) =>
            LastPageInfo = (currentPage, totalPages, totalCount);

        public void ClearForm() => ClearFormCalled = true;
        public void ShowMessage(string message) => ShownMessages.Add(message);
        public void ShowValidationErrors(IReadOnlyList<string> errors) => ShownValidationErrors = errors.ToList();
    }
}
