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

        public List<SaleCartLine> CartLinesList { get; set; } = new List<SaleCartLine>();
        public IReadOnlyList<SaleCartLine> CartLines => CartLinesList;

        public List<string> ShownMessages { get; } = new List<string>();
        public void ShowMessage(string message) => ShownMessages.Add(message);

        public (int Id, string Code, string Name, int Stock, string PriceSaleFormatted)? SelectedProductSetTo { get; private set; }
        public void SetSelectedProduct(int id, string code, string name, int stock, string priceSaleFormatted) =>
            SelectedProductSetTo = (id, code, name, stock, priceSaleFormatted);

        public void AddCartLine(SaleCartLine line) => CartLinesList.Add(line);
        public void RemoveCartLineAt(int index) => CartLinesList.RemoveAt(index);

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
