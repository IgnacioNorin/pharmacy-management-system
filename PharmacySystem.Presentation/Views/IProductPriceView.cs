using System.Collections.Generic;

namespace PharmacySystem.Presentation
{
    // The Prices tab of frmManagement: release a product for sale, re-price it, or withdraw it.
    public interface IProductPriceView
    {
        // The product the user is acting on (selected in either grid).
        int SelectedProductId { get; }
        // The new sale price typed in the form.
        string NewPriceText { get; }
        // Free-text reason for the change (optional).
        string Reason { get; }

        // Products in stock that have not been released for sale yet.
        void LoadReleasable(IEnumerable<ProductPriceRow> rows);
        // Products currently released for sale.
        void LoadCommercialized(IEnumerable<ProductPriceRow> rows);
        // Price timeline of the selected product, newest first.
        void LoadHistory(IEnumerable<ProductPriceHistoryRow> entries);

        void ClearEntry();
        void ShowMessage(string message);
        void ShowValidationErrors(IReadOnlyList<string> errors);
    }
}
