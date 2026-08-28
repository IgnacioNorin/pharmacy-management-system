using System.Linq;
using PharmacySystem.Business;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // Ported from frmClient.cs. Preserves two real quirks from the original rather than fixing
    // them in passing:
    //  - OnSave shows "No se pudo guardar los cambios" on failure for BOTH register and update
    //    (unlike SupplierPresenter, where a failed Update returns silently - frmClient's
    //    btnSave_Click never returns early, it always falls through to the shared result check).
    //  - A newly registered client's row gets Id = 0 in the grid, because
    //    IPersonService.Register returns only a bool, never the new row's id - same as the
    //    original, which reused the still-"0" txtid.Text for the new row.
    //  - OnDelete does nothing at all (no message) when nothing is selected, unlike
    //    SupplierPresenter's explicit "seleccione un proveedor".
    public class ClientPresenter
    {
        private readonly IClientView _view;
        private readonly IPersonService _service;
        private readonly CurrentUser _currentUser;

        public ClientPresenter(IClientView view, IPersonService service, CurrentUser currentUser)
        {
            _view = view;
            _service = service;
            _currentUser = currentUser;
        }

        private bool Can(string permission) => _currentUser?.Can(permission) ?? false;

        public void OnLoad()
        {
            var clients = _service.List()
                .Where(p => p.oPersonType.idPersonType == (int)PersonType.Cliente)
                .Select(ClientRow.From);
            _view.LoadClients(clients);
        }

        public void OnSave()
        {
            if (!Can("clientes.gestionar"))
            {
                _view.ShowMessage("No tiene permiso para crear o editar clientes.");
                return;
            }

            var errors = _view.Validate();
            if (errors.Count > 0)
            {
                _view.ShowValidationErrors(errors);
                return;
            }

            if (_view.IsCompany && (string.IsNullOrWhiteSpace(_view.BusinessName) || string.IsNullOrWhiteSpace(_view.Activity)))
            {
                _view.ShowValidationErrors(new[] { "Para una empresa, la razón social y el giro son obligatorios." });
                return;
            }

            Person person = new Person
            {
                idPerson = _view.PersonId,
                document = _view.Document?.Trim(),
                name = _view.Name?.Trim(),
                address = _view.Address?.Trim(),
                phone = _view.Phone?.Trim(),
                businessName = _view.BusinessName?.Trim(),
                activity = _view.Activity?.Trim(),
                commune = _view.Commune?.Trim(),
                email = _view.Email?.Trim(),
                isCompany = _view.IsCompany,
                password = "",
                oPersonType = new TypePerson { idPersonType = (int)PersonType.Cliente }
            };

            bool result;
            if (person.idPerson == 0)
            {
                result = _service.Register(person);
                if (result)
                {
                    _view.AddRow(ClientRow.From(person));
                }
            }
            else
            {
                result = _service.Update(person);
                if (result)
                {
                    _view.ReplaceRow(_view.SelectedIndex - 1, ClientRow.From(person));
                }
            }

            if (result)
            {
                _view.ClearForm();
            }
            else
            {
                _view.ShowMessage("No se pudo guardar los cambios\nRevise los datos");
            }
        }

        public void OnDelete()
        {
            if (_view.SelectedIndex <= 0)
            {
                return;
            }

            if (!Can("clientes.gestionar"))
            {
                _view.ShowMessage("No tiene permiso para eliminar clientes.");
                return;
            }

            if (!_view.ConfirmDelete())
            {
                return;
            }

            if (_service.Delete(_view.PersonId))
            {
                _view.RemoveRow(_view.SelectedIndex - 1);
                _view.ClearForm();
            }
            else
            {
                _view.ShowMessage("No se pudo eliminar el registro\nRevise los datos");
            }
        }
    }
}
