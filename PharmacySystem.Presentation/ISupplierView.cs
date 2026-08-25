using System.Collections.Generic;

namespace PharmacySystem.Presentation
{
    // Passive View: the Form exposes plain data and forwards user intent by calling the
    // presenter's OnXxx methods; every decision (which message, whether to clear the form,
    // which row changes) is made by SupplierPresenter, not by frmSupplier itself. No type here
    // comes from System.Windows.Forms, which is what lets SupplierPresenter run in a plain unit
    // test.
    public interface ISupplierView
    {
        // Form field state, read by the presenter when handling an intent.
        int SelectedIndex { get; }   // 1-based row currently loaded into the form; 0 = none/new
        int RowCount { get; }
        int SupplierId { get; }
        string Document { get; }
        string CompanyName { get; }
        string Email { get; }
        string Phone { get; }

        // Field-level validation stays view-side for this pilot: it already runs against the
        // same Validations.rules table used everywhere else in the app, keyed by control. Moving
        // that keying to plain strings is a separate, larger change and isn't needed to prove
        // the MVP split - the presenter only needs the resulting errors, not how they were found.
        List<string> Validate();

        bool ConfirmDelete();

        // Outputs the presenter drives.
        void LoadSuppliers(IEnumerable<SupplierRow> suppliers);
        void AddRow(SupplierRow row);
        void ReplaceRow(int index, SupplierRow row);
        void RemoveRow(int index);
        void ClearForm();
        void ShowMessage(string message);

        // Kept separate from ShowMessage because the original dialog for validation failures
        // uses a different title and icon ("Errores de Validación" / Warning) than every other
        // message in this screen ("Mensaje" / Exclamation).
        void ShowValidationErrors(IReadOnlyList<string> errors);
    }
}
