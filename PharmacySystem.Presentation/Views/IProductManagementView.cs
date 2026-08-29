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

        // Raw text of the two price fields. Only read when price editing is enabled for the role
        // (productos.editar_precios); otherwise the fields are disabled and their content ignored.
        string PurchasePriceText { get; }
        string SalePriceText { get; }

        List<string> Validate();
        bool ConfirmDelete();

        // Enables or disables the two price fields depending on whether the role may edit prices.
        void SetPriceEditingEnabled(bool enabled);

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
