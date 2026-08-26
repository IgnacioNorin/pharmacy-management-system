using System.Collections.Generic;

namespace PharmacySystem.Presentation
{
    // ModalSupplier is a read-only picker dialog (grid + "select" button that closes the
    // dialog), not a CRUD screen, so it needs a much smaller surface than ISupplierView: just
    // the initial load. Row selection, filtering and cell painting never touched
    // SupplierService in the original and stay in the Form as pure UI/dialog-result concerns.
    public interface ISupplierPickerView
    {
        void LoadSuppliers(IEnumerable<SupplierRow> suppliers);
    }
}
