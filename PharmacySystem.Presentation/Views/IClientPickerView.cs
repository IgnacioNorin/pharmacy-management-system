using System.Collections.Generic;

namespace PharmacySystem.Presentation
{
    // Same shape as ISupplierPickerView: ModalPerson is a read-only picker dialog for clients,
    // not a CRUD screen. Row selection, filtering and cell painting never touched the service in
    // the original and stay in the Form.
    public interface IClientPickerView
    {
        void LoadClients(IEnumerable<ClientRow> clients);
    }
}
