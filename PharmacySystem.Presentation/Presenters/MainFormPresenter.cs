using PharmacySystem.Business;
using PharmacySystem.Helpers;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // Ported from MainForm.cs's notificationDate()/notificationStock(). Fase 3 of the alerts
    // rework replaced the two separate "is anything wrong" checks with a single itemized,
    // severity-ranked list (NotificationConfigService.GetActiveAlerts()) - the View decides how to
    // render it (notification center), this presenter just fetches and forwards it.
    public class MainFormPresenter
    {
        private readonly IMainFormView _view;
        private readonly IStoreService _storeService;
        private readonly INotificationConfigService _notificationService;

        public MainFormPresenter(IMainFormView view, IStoreService storeService, INotificationConfigService notificationService)
        {
            _view = view;
            _storeService = storeService;
            _notificationService = notificationService;
        }

        public void OnLoad(Person person)
        {
            Store store = _storeService.ListStore();
            CultureInfoHelper.SetCurrency(store?.currencyCulture);

            _view.SetUserName(person.name, person.oPersonType.description);
            // Anyone who isn't Empleado (both admin tiers) sees the admin sections.
            _view.SetAdministrativeMenusVisible(person.oPersonType.idPersonType != (int)PersonType.Empleado);
        }

        public void RefreshAlerts()
        {
            _view.ShowAlerts(_notificationService.GetActiveAlerts());
        }
    }
}
