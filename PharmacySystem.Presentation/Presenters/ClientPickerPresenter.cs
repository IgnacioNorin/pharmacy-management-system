using System.Linq;
using PharmacySystem.Business;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    public class ClientPickerPresenter
    {
        private readonly IClientPickerView _view;
        private readonly IPersonService _service;

        public ClientPickerPresenter(IClientPickerView view, IPersonService service)
        {
            _view = view;
            _service = service;
        }

        public void OnLoad()
        {
            var clients = _service.ListClients().Select(ClientRow.From);
            _view.LoadClients(clients);
        }
    }
}
