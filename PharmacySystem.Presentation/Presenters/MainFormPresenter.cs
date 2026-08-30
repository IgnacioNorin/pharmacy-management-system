using PharmacySystem.Business;
using PharmacySystem.Infrastructure;
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
        private readonly INotificationConfigService _notificationService;

        public MainFormPresenter(IMainFormView view, INotificationConfigService notificationService)
        {
            _view = view;
            _notificationService = notificationService;
        }

        public void OnLoad(CurrentUser user)
        {
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
                Roles      = user.Can("roles.gestionar"),
                Reports    = user.Can("reportes.acceso"),
                CashCount  = user.Can("caja.acceso"),
                Alerts     = user.Can("alertas.acceso")
            });
        }

        public void RefreshAlerts()
        {
            try
            {
                _view.ShowAlerts(_notificationService.GetActiveAlerts());
            }
            catch (DataUnavailableException)
            {
                // Runs on the 5-minute timer and on every navigation, off the UI thread. A brief
                // database outage must not pop a dialog on a background tick or bubble up as an
                // unobserved task exception - the badge keeps its last value and the next tick
                // retries. The repository already logged the underlying error.
            }
        }
    }
}
