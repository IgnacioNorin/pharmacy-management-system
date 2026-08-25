using System.Linq;
using PharmacySystem.Business;

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
            var clients = _service.List()
                .Where(p => p.oPersonType.idPersonType == 3)
                .Select(ClientRow.From);
            _view.LoadClients(clients);
        }
    }
}
