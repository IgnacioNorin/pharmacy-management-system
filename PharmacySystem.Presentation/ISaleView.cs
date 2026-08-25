using System.Collections.Generic;

namespace PharmacySystem.Presentation
{
    public interface ISaleView
    {
        int SelectedProductId { get; }
        string SelectedProductName { get; }
        int Stock { get; }
        decimal Amount { get; }
        string PriceSaleText { get; }

        string DocumentClient { get; }
        string NameClient { get; }
        string PayWithText { get; }
        string TotalPayText { get; }
        string ChangeText { get; }
        string DocumentType { get; }

        IReadOnlyList<SaleCartLine> CartLines { get; }

        void ShowMessage(string message);

        void SetSelectedProduct(int id, string code, string name, int stock, string priceSaleFormatted);
        void AddCartLine(SaleCartLine line);
        void RemoveCartLineAt(int index);
        void SetTotalText(string formattedTotal);
        void SetChangeText(string formattedChange);
        void ClearProductEntry();
        void ClearSale();
        void SaleRegistered(int idSale);
    }
}
