using PharmacySystem.Business;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // Ported from ModalConfignotification.cs's Load/btnSaveConfig_Click. The original nests
    // three if/else levels with no early return; this flattens them into early returns over the
    // same three mutually-exclusive checks, in the same order, so the decision tree - and every
    // message it can show - is unchanged.
    public class NotificationConfigPresenter
    {
        private readonly INotificationConfigView _view;
        private readonly INotificationConfigService _service;
        private readonly CurrentUser _currentUser;

        public NotificationConfigPresenter(INotificationConfigView view, INotificationConfigService service, CurrentUser currentUser)
        {
            _view = view;
            _service = service;
            _currentUser = currentUser;
        }

        private bool Can(string permission) => _currentUser?.Can(permission) ?? false;

        public void OnLoad()
        {
            _view.SetDays(_service.ConfigDay().ToString());
            _view.SetStock(_service.ConfigStock().ToString());
        }

        public void OnSave()
        {
            if (!Can("alertas.configurar"))
            {
                _view.ShowMessage("No tiene permiso para cambiar la configuracion de alertas.");
                return;
            }

            string daysText = _view.DaysText;
            string stockText = _view.StockText;

            bool daysInvalid = !string.IsNullOrEmpty(daysText) && !int.TryParse(daysText, out _);
            if (daysInvalid)
            {
                _view.ShowInvalidValueError();
                return;
            }

            bool stockInvalid = !string.IsNullOrEmpty(stockText) && !int.TryParse(stockText, out _);
            if (stockInvalid)
            {
                _view.ShowInvalidValueError();
                return;
            }

            if (string.IsNullOrEmpty(daysText) || string.IsNullOrEmpty(stockText))
            {
                _view.ShowEmptyFieldsError();
                return;
            }

            NotificationConfig config = new NotificationConfig
            {
                days = int.Parse(daysText.Trim()),
                criticalStock = int.Parse(stockText.Trim())
            };

            if (_service.ConfigUpdate(config))
            {
                _view.ShowSaveSucceeded();
            }
            else
            {
                _view.ShowSaveFailed();
            }
        }
    }
}
