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
        void ClearSale();
        void SetGenerateEnabled(bool enabled);
        void ShowMessage(string message);
        void CreditNoteCompleted();
    }
}
