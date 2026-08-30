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
        public string DocumentType { get; set; } = "Boleta";
        public string PaymentMethod { get; set; } = "Efectivo";

        public string RecipientTaxId { get; set; } = "";
        public string RecipientBusinessName { get; set; } = "";
        public string RecipientActivity { get; set; } = "";
        public string RecipientAddress { get; set; } = "";
        public string RecipientCommune { get; set; } = "";

        public IReadOnlyList<string> DocumentTypeOptions { get; private set; }
        public string SelectedDocumentTypeOption { get; private set; }
        public void SetDocumentTypeOptions(IReadOnlyList<string> options, string selected)
        {
            DocumentTypeOptions = options;
            SelectedDocumentTypeOption = selected;
            DocumentType = selected; // mirrors the real combo landing on the selected item
        }

        public IReadOnlyList<string> PaymentMethodOptions { get; private set; }
        public string SelectedPaymentMethodOption { get; private set; }
        public void SetPaymentMethodOptions(IReadOnlyList<string> options, string selected)
        {
            PaymentMethodOptions = options;
            SelectedPaymentMethodOption = selected;
            PaymentMethod = selected;
        }

        // Set this to the split the "pago mixto" dialog should return; null = the cashier cancels.
        public IReadOnlyList<SalePaymentEntry> PaymentSplitToReturn { get; set; }
        public (decimal Total, IReadOnlyList<SalePaymentEntry> Current)? PromptPaymentSplitArgs { get; private set; }
        public IReadOnlyList<SalePaymentEntry> ShownPaymentSplit { get; private set; }
        public int ShowPaymentSplitCallCount { get; private set; }

        public IReadOnlyList<SalePaymentEntry> PromptPaymentSplit(decimal total, IReadOnlyList<SalePaymentEntry> current, IReadOnlyList<string> methods)
        {
            PromptPaymentSplitArgs = (total, current);
            return PaymentSplitToReturn;
        }

        public void ShowPaymentSplit(IReadOnlyList<SalePaymentEntry> split)
        {
            ShownPaymentSplit = split;
            ShowPaymentSplitCallCount++;
        }

        public bool? FacturaFieldsVisible { get; private set; }
        public void SetFacturaFieldsVisible(bool visible) => FacturaFieldsVisible = visible;

        public (string Document, string Name)? ClientSetTo { get; private set; }
        public void SetClient(string document, string name)
        {
            ClientSetTo = (document, name);
            DocumentClient = document ?? "";
            NameClient = name ?? "";
        }

        public (string TaxId, string BusinessName, string Activity, string Address, string Commune)? RecipientSetTo { get; private set; }
        public void SetRecipient(string taxId, string businessName, string activity, string address, string commune)
        {
            RecipientSetTo = (taxId, businessName, activity, address, commune);
            RecipientTaxId = taxId ?? "";
            RecipientBusinessName = businessName ?? "";
            RecipientActivity = activity ?? "";
            RecipientAddress = address ?? "";
            RecipientCommune = commune ?? "";
        }

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
