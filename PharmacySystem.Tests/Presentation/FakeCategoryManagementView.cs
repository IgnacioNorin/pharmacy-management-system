using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Model;
using PharmacySystem.Presentation;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakeCategoryManagementView : ICategoryManagementView
    {
        public int SelectedIndex { get; set; }
        public int RowCount { get; set; }
        public int CategoryId { get; set; }
        public string Description { get; set; }
        public List<string> ValidationErrors { get; set; } = new List<string>();
        public bool ConfirmDeleteResult { get; set; } = true;

        List<string> ICategoryManagementView.Validate() => ValidationErrors;
        public bool ConfirmDelete() => ConfirmDeleteResult;

        public List<CategoryRow> LoadedCategories { get; private set; }
        public List<CategoryRow> AddedRows { get; } = new List<CategoryRow>();
        public List<(int Index, CategoryRow Row)> ReplacedRows { get; } = new List<(int, CategoryRow)>();
        public List<int> RemovedIndexes { get; } = new List<int>();
        public bool ClearFormCalled { get; private set; }
        public List<string> ShownMessages { get; } = new List<string>();
        public List<string> ShownValidationErrors { get; private set; }
        public List<ComboBoxItem> RefreshedProductCategoryOptions { get; private set; }

        public void LoadCategories(IEnumerable<CategoryRow> categories) => LoadedCategories = categories.ToList();
        public void AddRow(CategoryRow row) => AddedRows.Add(row);
        public void ReplaceRow(int index, CategoryRow row) => ReplacedRows.Add((index, row));
        public void RemoveRow(int index) => RemovedIndexes.Add(index);
        public void ClearForm() => ClearFormCalled = true;
        public void ShowMessage(string message) => ShownMessages.Add(message);
        public void ShowValidationErrors(IReadOnlyList<string> errors) => ShownValidationErrors = errors.ToList();
        public void RefreshProductCategoryOptions(IEnumerable<ComboBoxItem> options) => RefreshedProductCategoryOptions = options.ToList();
    }
}
