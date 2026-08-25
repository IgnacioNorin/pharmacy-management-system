using System;
using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Presentation;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakePurchaseView : IPurchaseView
    {
        public int SelectedProductId { get; set; }
        public string SelectedProductCode { get; set; }
        public string SelectedProductName { get; set; }
        public decimal Amount { get; set; } = 1;
        public DateTime ExpirationDate { get; set; } = DateTime.Today;
        public string PricePurchaseText { get; set; }
        public string PriceSaleText { get; set; }

        public string DocumentNumber { get; set; }
        public string DocumentType { get; set; } = "Factura";
        public int SelectedSupplierId { get; set; }

        // What the presenter rendered via AddCartLine/RemoveCartLineAt - the presenter owns cart
        // state now, this list is just what the View was told to display, for assertions.
        public List<PurchaseCartLine> RenderedCartLines { get; } = new List<PurchaseCartLine>();

        public List<string> ValidationErrors { get; set; } = new List<string>();

        List<string> IPurchaseView.ValidateProductEntry() => ValidationErrors;

        public List<string> ShownValidationErrors { get; private set; }
        public List<string> ShownMessages { get; } = new List<string>();
        public bool DocumentNumberFocused { get; private set; }

        void IPurchaseView.ShowValidationErrors(IReadOnlyList<string> errors) => ShownValidationErrors = errors.ToList();
        public void ShowMessage(string message) => ShownMessages.Add(message);
        public void FocusDocumentNumber() => DocumentNumberFocused = true;

        public (int Id, string Code, string Name)? SelectedProductSetTo { get; private set; }
        public void SetSelectedProduct(int id, string code, string name) => SelectedProductSetTo = (id, code, name);

        public void AddCartLine(PurchaseCartLine line) => RenderedCartLines.Add(line);
        public void RemoveCartLineAt(int index) => RenderedCartLines.RemoveAt(index);

        public string TotalText { get; private set; }
        public void SetTotalText(string formattedTotal) => TotalText = formattedTotal;

        public bool ProductEntryCleared { get; private set; }
        public void ClearProductEntry() => ProductEntryCleared = true;

        public bool PurchaseCleared { get; private set; }
        public void ClearPurchase() => PurchaseCleared = true;
    }
}
