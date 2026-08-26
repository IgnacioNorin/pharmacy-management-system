using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // Passive View for the "Categoria" tab of frmManagement. Category CRUD here has a real
    // cross-tab side effect in the original: a successful save/delete also rebuilds the
    // "Categoria" combo on the Product tab from scratch. RefreshProductCategoryOptions is that
    // side effect, kept explicit on this interface rather than hidden inside the Form.
    public interface ICategoryManagementView
    {
        int SelectedIndex { get; }
        int RowCount { get; }
        int CategoryId { get; }
        string Description { get; }

        List<string> Validate();
        bool ConfirmDelete();

        void LoadCategories(IEnumerable<CategoryRow> categories);
        void AddRow(CategoryRow row);
        void ReplaceRow(int index, CategoryRow row);
        void RemoveRow(int index);
        void ClearForm();
        void ShowMessage(string message);
        void ShowValidationErrors(IReadOnlyList<string> errors);

        void RefreshProductCategoryOptions(IEnumerable<ComboBoxItem> options);
    }
}
