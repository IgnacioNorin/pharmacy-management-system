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
        // The single payment method combo - used when the sale is not split ("pago mixto").
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

        // Opens the "pago mixto" dialog for the given total, seeded with the current split (may
        // be null). Returns the entered split - which must sum to total - or null if cancelled.
        IReadOnlyList<SalePaymentEntry> PromptPaymentSplit(decimal total, IReadOnlyList<SalePaymentEntry> current, IReadOnlyList<string> methods);
        // Reflects the current split: null / empty re-enables the single method combo; a list
        // shows the sale is paid with several methods ("Mixto").
        void ShowPaymentSplit(IReadOnlyList<SalePaymentEntry> split);
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
