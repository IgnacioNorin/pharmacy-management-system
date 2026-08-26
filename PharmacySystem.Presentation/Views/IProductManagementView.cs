using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    public interface IProductManagementView
    {
        int SelectedIndex { get; }
        int RowCount { get; }
        int ProductId { get; }
        string Code { get; }
        string Name { get; }
        string Description { get; }
        int SelectedCategoryId { get; }
        string SelectedCategoryText { get; }

        List<string> Validate();
        bool ConfirmDelete();

        void LoadCategoryOptions(IEnumerable<ComboBoxItem> options);
        void LoadProducts(IEnumerable<ManagementProductRow> products);
        void AddRow(ManagementProductRow row);
        void ReplaceRow(int index, ManagementProductRow row);
        void RemoveRow(int index);
        void ClearForm();
        void ShowMessage(string message);
        void ShowValidationErrors(IReadOnlyList<string> errors);
    }
}
