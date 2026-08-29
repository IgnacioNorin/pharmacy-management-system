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
        string PaymentMethod { get; }

        // Recipient fiscal data, only read when DocumentType is a Factura.
        string RecipientTaxId { get; }
        string RecipientBusinessName { get; }
        string RecipientActivity { get; }
        string RecipientAddress { get; }
        string RecipientCommune { get; }

        void ShowMessage(string message);

        void SetDocumentTypeOptions(IReadOnlyList<string> options, string selected);
        void SetPaymentMethodOptions(IReadOnlyList<string> options, string selected);
        void SetFacturaFieldsVisible(bool visible);
        void SetClient(string document, string name);
        void SetRecipient(string taxId, string businessName, string activity, string address, string commune);
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
