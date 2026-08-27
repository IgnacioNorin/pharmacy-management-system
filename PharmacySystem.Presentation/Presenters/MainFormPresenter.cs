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

        public void OnLoad(CurrentUser user)
        {
            Store store = _storeService.ListStore();
            CultureInfoHelper.SetCurrency(store?.currencyCulture);

            _view.SetUserName(user.Person.name, user.Person.oPersonType?.description);

            // Sidebar visibility is driven by permissions now, not by the role. Gestión is a
            // container for three tabs, so its button shows if the user can reach any of them.
            _view.ApplySidebarPermissions(new SidebarPermissions
            {
                Sales      = user.Can("ventas.acceso"),
                Purchases  = user.Can("compras.acceso"),
                Clients    = user.Can("clientes.acceso"),
                Suppliers  = user.Can("proveedores.acceso"),
                Management = user.Can("productos.acceso") || user.Can("categorias.acceso") || user.Can("tienda.acceso"),
                Users      = user.Can("usuarios.acceso"),
                Reports    = user.Can("reportes.acceso"),
                Alerts     = user.Can("alertas.acceso")
            });
        }

        public void RefreshAlerts()
        {
            _view.ShowAlerts(_notificationService.GetActiveAlerts());
        }
    }
}
