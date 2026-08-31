using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    public interface ICreditNoteView
    {
        string DocumentTypeInput { get; }
        string DocumentNumberInput { get; }
        string ReasonInput { get; }

        bool ConfirmGenerate();

        void SetDocumentTypeOptions(IReadOnlyList<string> options);
        void ShowSale(SaleLookup sale);
        // The lines of the found sale with their still-creditable quantity, for the operator to
        // choose how many units of each to credit.
        void ShowCreditableLines(IReadOnlyList<SaleCreditDetail> lines);
        // How many units of each original line the operator asked to credit.
        IReadOnlyList<CreditNoteLineRequest> GetRequestedQuantities();
        void ClearSale();
        void SetGenerateEnabled(bool enabled);
        void ShowMessage(string message);
        void CreditNoteCompleted();
    }
}
