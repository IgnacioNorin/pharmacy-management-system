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
        public const int MinDays = 1;
        public const int MaxDays = 3650;          // ~10 years
        public const int MaxCriticalStock = 100000;

        private readonly INotificationConfigView _view;
        private readonly INotificationConfigService _service;
        private readonly CurrentUser _currentUser;
        private readonly ISecurityAudit _audit;

        public NotificationConfigPresenter(INotificationConfigView view, INotificationConfigService service, CurrentUser currentUser, ISecurityAudit audit)
        {
            _view = view;
            _service = service;
            _currentUser = currentUser;
            _audit = audit;
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

            int days = int.Parse(daysText.Trim());
            int criticalStock = int.Parse(stockText.Trim());

            // Bounds (DEF-19): a negative or huge threshold silently breaks the alert - a
            // negative one never fires, a huge one flags the whole catalogue. Days: 1 to 10 years.
            if (days < MinDays || days > MaxDays || criticalStock < 0 || criticalStock > MaxCriticalStock)
            {
                _view.ShowInvalidValueError();
                return;
            }

            NotificationConfig config = new NotificationConfig
            {
                days = days,
                criticalStock = criticalStock
            };

            if (_service.ConfigUpdate(config))
            {
                _audit.Record(_currentUser?.PersonId ?? 0, "alert_config.update", "notification_settings", 1,
                    $"vencimiento {days} día(s), stock crítico {criticalStock}");
                _view.ShowSaveSucceeded();
            }
            else
            {
                _view.ShowSaveFailed();
            }
        }
    }
}
