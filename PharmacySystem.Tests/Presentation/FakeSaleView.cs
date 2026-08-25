using System.Collections.Generic;
using PharmacySystem.Presentation;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakeSaleView : ISaleView
    {
        public int SelectedProductId { get; set; }
        public string SelectedProductName { get; set; }
        public int Stock { get; set; }
        public decimal Amount { get; set; } = 1;
        public string PriceSaleText { get; set; }

        public string DocumentClient { get; set; } = "";
        public string NameClient { get; set; } = "";
        public string PayWithText { get; set; } = "0";
        public string TotalPayText { get; set; } = "0";
        public string ChangeText { get; set; } = "0";
        public string DocumentType { get; set; } = "Factura";

        // What the presenter rendered via AddCartLine/RemoveCartLineAt - the presenter owns cart
        // state now, this list is just what the View was told to display, for assertions.
        public List<SaleCartLine> RenderedCartLines { get; } = new List<SaleCartLine>();

        public List<string> ShownMessages { get; } = new List<string>();
        public void ShowMessage(string message) => ShownMessages.Add(message);

        public (int Id, string Code, string Name, int Stock, string PriceSaleFormatted)? SelectedProductSetTo { get; private set; }
        public void SetSelectedProduct(int id, string code, string name, int stock, string priceSaleFormatted) =>
            SelectedProductSetTo = (id, code, name, stock, priceSaleFormatted);

        public void AddCartLine(SaleCartLine line) => RenderedCartLines.Add(line);
        public void RemoveCartLineAt(int index) => RenderedCartLines.RemoveAt(index);

        public string TotalText { get; private set; }
        public void SetTotalText(string formattedTotal) => TotalText = formattedTotal;

        public string ChangeTextSet { get; private set; }
        public void SetChangeText(string formattedChange) => ChangeTextSet = formattedChange;

        public bool ProductEntryCleared { get; private set; }
        public void ClearProductEntry() => ProductEntryCleared = true;

        public bool SaleCleared { get; private set; }
        public void ClearSale() => SaleCleared = true;

        public int? RegisteredSaleId { get; private set; }
        public void SaleRegistered(int idSale) => RegisteredSaleId = idSale;
    }
}
