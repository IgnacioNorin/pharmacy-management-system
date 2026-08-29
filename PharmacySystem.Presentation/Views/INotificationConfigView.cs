namespace PharmacySystem.Presentation
{
    // Passive View, same pattern as ISupplierView. Three distinct message methods instead of one
    // generic ShowMessage because the original ModalConfignotification.cs uses a different title
    // and icon for each of the three outcomes (Error/"Error", Information/"Exito",
    // Warning/"Fallido") - the Form maps each to its own MessageBoxIcon, Presentation stays free
    // of System.Windows.Forms.
    public interface INotificationConfigView
    {
        string DaysText { get; }
        string StockText { get; }

        void SetDays(string value);
        void SetStock(string value);

        void ShowInvalidValueError();
        void ShowEmptyFieldsError();
        void ShowSaveSucceeded();
        void ShowSaveFailed();

        // Generic notice (used for the "no tiene permiso" message).
        void ShowMessage(string message);
    }
}
