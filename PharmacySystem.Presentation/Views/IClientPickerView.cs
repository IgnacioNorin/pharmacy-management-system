using System.Collections.Generic;

namespace PharmacySystem.Presentation
{
    // Same shape as ISupplierPickerView: ModalPerson is a read-only picker dialog for clients
    // (person_type_id = PersonType.Cliente), not a CRUD screen. Row selection, filtering and cell
    // painting never touched PersonService in the original and stay in the Form.
    public interface IClientPickerView
    {
        void LoadClients(IEnumerable<ClientRow> clients);
    }
}
