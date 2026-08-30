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
        bool TaxAffected { get; }

        // The free-text term for the server-side search (matches code / name / description).
        string SearchText { get; }

        List<string> Validate();
        bool ConfirmDelete();

        void LoadCategoryOptions(IEnumerable<ComboBoxItem> options);
        // Replaces the whole grid with one page of rows.
        void LoadProducts(IEnumerable<ManagementProductRow> products);
        // Updates the pager: current page, total pages and total row count.
        void SetPageInfo(int currentPage, int totalPages, int totalCount);
        void ClearForm();
        void ShowMessage(string message);
        void ShowValidationErrors(IReadOnlyList<string> errors);
    }
}
