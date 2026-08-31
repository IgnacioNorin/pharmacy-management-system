using System.Collections.Generic;
using PharmacySystem.Model;
using PharmacySystem.Presentation;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakeCreditNoteView : ICreditNoteView
    {
        public string DocumentTypeInput { get; set; } = "Boleta";
        public string DocumentNumberInput { get; set; } = "";
        public string ReasonInput { get; set; } = "";
        public bool ConfirmGenerateResult { get; set; } = true;

        public IReadOnlyList<string> DocumentTypeOptions { get; private set; }
        public SaleLookup ShownSale { get; private set; }
        public IReadOnlyList<SaleCreditDetail> ShownLines { get; private set; }
        public int ClearSaleCount { get; private set; }
        public bool? GenerateEnabled { get; private set; }
        public List<string> ShownMessages { get; } = new List<string>();
        public int CreditNoteCompletedCount { get; private set; }

        // What GetRequestedQuantities() hands back to the presenter; tests set this.
        public List<CreditNoteLineRequest> RequestedQuantities { get; set; } = new List<CreditNoteLineRequest>();

        public bool ConfirmGenerate() => ConfirmGenerateResult;

        public void SetDocumentTypeOptions(IReadOnlyList<string> options) => DocumentTypeOptions = options;
        public void ShowSale(SaleLookup sale) => ShownSale = sale;
        public void ShowCreditableLines(IReadOnlyList<SaleCreditDetail> lines) => ShownLines = lines;
        public IReadOnlyList<CreditNoteLineRequest> GetRequestedQuantities() => RequestedQuantities;
        public void ClearSale() => ClearSaleCount++;
        public void SetGenerateEnabled(bool enabled) => GenerateEnabled = enabled;
        public void ShowMessage(string message) => ShownMessages.Add(message);
        public void CreditNoteCompleted() => CreditNoteCompletedCount++;
    }
}
