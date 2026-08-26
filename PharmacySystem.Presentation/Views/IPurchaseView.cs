using System;
using System.Collections.Generic;

namespace PharmacySystem.Presentation
{
    public interface IPurchaseView
    {
        int SelectedProductId { get; }
        string SelectedProductCode { get; }
        string SelectedProductName { get; }
        decimal Amount { get; }
        DateTime ExpirationDate { get; }
        string PricePurchaseText { get; }
        string PriceSaleText { get; }

        string DocumentNumber { get; }
        string DocumentType { get; }
        int SelectedSupplierId { get; }

        List<string> ValidateProductEntry();
        void ShowValidationErrors(IReadOnlyList<string> errors);
        void ShowMessage(string message);
        void FocusDocumentNumber();

        void SetSelectedProduct(int id, string code, string name);
        void AddCartLine(PurchaseCartLine line);
        void RemoveCartLineAt(int index);
        void SetTotalText(string formattedTotal);
        void ClearProductEntry();
        void ClearPurchase();
    }
}
