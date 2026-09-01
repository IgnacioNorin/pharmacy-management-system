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
        private readonly IPersonService _personService;
        private readonly IPermissionService _permissionService;

        public MainFormPresenter(IMainFormView view, INotificationConfigService notificationService,
            IPersonService personService, IPermissionService permissionService)
        {
            _view = view;
            _notificationService = notificationService;
            _personService = personService;
            _permissionService = permissionService;
        }

        // Re-resolves the signed-in user's role and permission set from the database (DEF-21:
        // otherwise a permission revoked mid-session only takes effect after logout). Returns:
        //  - a fresh CurrentUser when the account is still active,
        //  - null when the account was deleted or deactivated (the caller sends them to login),
        //  - the same session unchanged on a transient data error (do not disrupt the session).
        public CurrentUser? RefreshSession(CurrentUser? current)
        {
            if (current == null) return null;

            try
            {
                Person? fresh = _personService.GetById(current.PersonId);
                if (fresh == null || !fresh.Estado)
                {
                    return null;
                }
                return new CurrentUser(fresh,
                    _permissionService.GetPermissionsForRole(fresh.oPersonType?.idPersonType ?? 0));
            }
            catch (DataUnavailableException)
            {
                return current;
            }
        }

        public void OnLoad(CurrentUser user)
        {
            _view.SetUserName(user.Person.name, user.Person.oPersonType?.description ?? string.Empty);

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
                AuditLog   = user.Can("bitacora.acceso"),
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
