using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Threading;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using Wpf.Ui.Controls;

namespace PharmacySystem.Ui
{
    // WPF port of MainForm. The application shell: sidebar navigation, the signed-in user header,
    // the alert badge and its 5-minute refresh, and the session re-check on activation. Every
    // screen it opens is a modal WPF dialog (see the XxxDialog helpers); the only persistent
    // content is HomeView. MainFormPresenter / HomePresenter are unchanged.
    public partial class MainWindow : FluentWindow, IMainFormView
    {
        private readonly ShellServices _services;
        private readonly MainFormPresenter _presenter;
        private CurrentUser _session;
        private IReadOnlyList<ProductAlert> _currentAlerts = new List<ProductAlert>();

        private readonly DispatcherTimer _notificationTimer;
        private bool _sessionRefreshReady;
        private DateTime _lastSessionRefresh = DateTime.MinValue;

        public MainWindow(CurrentUser session, ShellServices services)
        {
            InitializeComponent();

            _session = session;
            _services = services;
            AppSession.Set(session);

            _presenter = _services.MainPresenter(this);

            _notificationTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(5) };
            _notificationTimer.Tick += (s, e) => CheckNotifications();

            // A sale / purchase / credit note raises this after moving stock so the badge
            // rechecks immediately instead of waiting for the 5-minute tick.
            InventoryChangeNotifier.StockChanged += CheckNotifications;

            Loaded += MainWindow_Loaded;
            Activated += MainWindow_Activated;
            Closed += (s, e) =>
            {
                _notificationTimer.Stop();
                InventoryChangeNotifier.StockChanged -= CheckNotifications;
            };
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_session != null)
            {
                _presenter.OnLoad(_session);
            }
            _notificationTimer.Start();
            OpenHome();
            _sessionRefreshReady = true;
        }

        // Re-checks the session whenever focus returns to the shell - a permission revoked by an
        // admin in another session then takes effect on the next navigation instead of only after
        // logout (DEF-21). Throttled, and skips the burst during construction.
        private void MainWindow_Activated(object? sender, EventArgs e)
        {
            if (!_sessionRefreshReady || _session == null) return;
            if ((DateTime.Now - _lastSessionRefresh).TotalSeconds < 3) return;
            _lastSessionRefresh = DateTime.Now;

            CurrentUser? refreshed = _presenter.RefreshSession(_session);
            if (refreshed == null)
            {
                _sessionRefreshReady = false;
                System.Windows.MessageBox.Show(this,
                    "Su sesión ya no es válida: la cuenta fue desactivada o eliminada.",
                    "Sesión finalizada",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                Close();
                return;
            }

            _session = refreshed;
            AppSession.Set(refreshed);
            _presenter.OnLoad(_session); // re-applies the sidebar to the current permission set
        }

        private IntPtr OwnerHandle() => new WindowInteropHelper(this).Handle;

        #region IMainFormView

        public void SetUserName(string name, string role)
        {
            lblUser.Text = name;
            lblUserRole.Text = role;
        }

        public void ApplySidebarPermissions(SidebarPermissions p)
        {
            btnSales.Visibility = Vis(p.Sales);
            btnPurchases.Visibility = Vis(p.Purchases);
            btnClients.Visibility = Vis(p.Clients);
            btnSuppliers.Visibility = Vis(p.Suppliers);
            btnManagement.Visibility = Vis(p.Management);
            btnUsers.Visibility = Vis(p.Users);
            btnRoles.Visibility = Vis(p.Roles);
            btnReports.Visibility = Vis(p.Reports);
            btnCashCount.Visibility = Vis(p.CashCount);
            btnAuditLog.Visibility = Vis(p.AuditLog);
            btnAlerts.Visibility = Vis(p.Alerts);
            if (!p.Alerts) alertBadge.Visibility = Visibility.Collapsed;

            // Hide a group header whose items are all hidden (a custom role can leave a whole
            // group empty). The StackPanel re-flows the rest on its own.
            lblGroupOperacion.Visibility = Vis(p.Sales || p.Purchases);
            lblGroupGestion.Visibility = Vis(p.Management || p.Suppliers || p.Clients || p.Users || p.Roles);
            lblGroupConsulta.Visibility = Vis(p.Reports || p.CashCount || p.AuditLog || p.Alerts);
        }

        public void ShowAlerts(IReadOnlyList<ProductAlert> alerts)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(() => ShowAlerts(alerts)));
                return;
            }

            _currentAlerts = alerts;

            // A muted alert still shows in the notification center, it just stops counting toward
            // the badge - that is the whole point of muting it.
            int unmutedCount = alerts.Count(a => a.MutedAt == null);
            lblAlertBadge.Text = unmutedCount > 99 ? "99+" : unmutedCount.ToString();
            alertBadge.Visibility = btnAlerts.Visibility == Visibility.Visible && unmutedCount > 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        #endregion

        private static Visibility Vis(bool visible) => visible ? Visibility.Visible : Visibility.Collapsed;

        private bool CanNavigate(string permission) => _session?.Can(permission) ?? false;

        // Off the UI thread: the query crosses the network and this runs on the 5-minute tick and
        // on every navigation. A brief outage is swallowed by the presenter.
        private void CheckNotifications() => Task.Run(() => _presenter.RefreshAlerts());

        private ManagementPermissions BuildManagementPermissions() => new ManagementPermissions
        {
            Categories = CanNavigate("categorias.acceso"),
            Products = CanNavigate("productos.acceso"),
            ProductPrices = CanNavigate("productos.editar_precios"),
            Store = CanNavigate("tienda.acceso")
        };

        // Swaps the content area and moves the "active" marker to the sidebar button that opened
        // it. Sections still under the modal-dialog model pass navButton = null (no marker).
        // A view holding unsaved work (INavGuard) can veto the switch.
        private void Navigate(System.Windows.Controls.Button? navButton, System.Windows.Controls.Control view)
        {
            if (contentHost.Content is INavGuard guard && !guard.CanNavigateAway())
            {
                return;
            }

            if (_activeNav != null) _activeNav.Tag = null;
            _activeNav = navButton;
            if (navButton != null) navButton.Tag = "active";
            contentHost.Content = view;
        }

        private System.Windows.Controls.Button? _activeNav;

        private void OpenHome()
        {
            Navigate(btnHome, new HomeView(
                _services.HomePresenter,
                () => _session,
                () => btnSales_Click(this, new RoutedEventArgs()),
                () => btnPurchases_Click(this, new RoutedEventArgs()),
                () => btnManagement_Click(this, new RoutedEventArgs()),
                code =>
                {
                    if (!CanNavigate("productos.acceso")) return;
                    Navigate(btnManagement, new ManagementView(_services.ManagementFactories, BuildManagementPermissions(), code));
                }));
        }

        #region Sidebar navigation

        private void btnHome_Click(object sender, RoutedEventArgs e) => OpenHome();

        private void btnClients_Click(object sender, RoutedEventArgs e)
        {
            if (!CanNavigate("clientes.acceso")) return;
            CheckNotifications();
            Navigate(btnClients, new ClientView(CanNavigate("clientes.gestionar"), _services.ClientPresenter));
        }

        private void btnSuppliers_Click(object sender, RoutedEventArgs e)
        {
            if (!CanNavigate("proveedores.acceso")) return;
            Navigate(btnSuppliers, new SupplierView(CanNavigate("proveedores.gestionar"), _services.SupplierPresenter));
        }

        private void btnManagement_Click(object sender, RoutedEventArgs e)
        {
            if (!(CanNavigate("productos.acceso") || CanNavigate("categorias.acceso") || CanNavigate("tienda.acceso"))) return;
            CheckNotifications();
            Navigate(btnManagement, new ManagementView(_services.ManagementFactories, BuildManagementPermissions()));
        }

        private void btnPurchases_Click(object sender, RoutedEventArgs e)
        {
            if (!CanNavigate("compras.acceso")) return;
            CheckNotifications();
            Navigate(btnPurchases, new PurchaseView(
                v => _services.PurchasePresenter(v, _session.PersonId),
                _services.Pickers));
        }

        private void btnSales_Click(object sender, RoutedEventArgs e)
        {
            if (!CanNavigate("ventas.acceso")) return;
            CheckNotifications();

            var hooks = new SaleShellHooks(
                _services.Pickers,
                _services.CreditNotePresenter,
                idSale => PrintSaleDialog.Show(OwnerHandle(), idSale, _services.TicketData),
                CanNavigate("ventas.nota_credito"));

            Navigate(btnSales, new SaleView(
                v => _services.SalePresenter(v, _session.PersonId),
                hooks));
        }

        private void btnUsers_Click(object sender, RoutedEventArgs e)
        {
            if (!CanNavigate("usuarios.acceso")) return;
            Navigate(btnUsers, new UserView(CanNavigate("usuarios.gestionar"), _services.UserPresenter));
        }

        private void btnRoles_Click(object sender, RoutedEventArgs e)
        {
            if (!CanNavigate("roles.gestionar")) return;
            Navigate(btnRoles, new RolesView(_services.RolesPresenter));
        }

        private void btnReports_Click(object sender, RoutedEventArgs e)
        {
            if (!CanNavigate("reportes.acceso")) return;

            var reportPermissions = new ReportPermissions
            {
                Sales = CanNavigate("reportes.ventas"),
                SalesExport = CanNavigate("reportes.ventas.exportar"),
                Purchases = CanNavigate("reportes.compras"),
                PurchasesExport = CanNavigate("reportes.compras.exportar"),
                Products = CanNavigate("reportes.productos"),
                ProductsExport = CanNavigate("reportes.productos.exportar"),
                AlertHistory = CanNavigate("reportes.alertas"),
                AlertHistoryExport = CanNavigate("reportes.alertas.exportar")
            };

            Navigate(btnReports, new ReportView(_services.ReportPresenter, reportPermissions));
        }

        private void btnCashCount_Click(object sender, RoutedEventArgs e)
        {
            if (!CanNavigate("caja.acceso")) return;
            Navigate(btnCashCount, new CashCountView(_services.CashCountPresenter));
        }

        private void btnAuditLog_Click(object sender, RoutedEventArgs e)
        {
            if (!CanNavigate("bitacora.acceso")) return;
            Navigate(btnAuditLog, new SecurityLogView(_services.SecurityLogPresenter));
        }

        private void btnAlerts_Click(object sender, RoutedEventArgs e)
        {
            if (!CanNavigate("alertas.acceso")) return;

            bool canOpenProduct = CanNavigate("productos.acceso");

            string? selectedProductCode = AlertsDialog.Show(
                OwnerHandle(),
                _currentAlerts,
                _services.NotificationConfigService,
                _session.PersonId,
                CanNavigate("alertas.reconocer"),
                CanNavigate("alertas.silenciar"),
                CanNavigate("alertas.configurar"),
                _services.NotificationConfigPresenter);

            if (canOpenProduct && !string.IsNullOrEmpty(selectedProductCode))
            {
                Navigate(btnManagement, new ManagementView(_services.ManagementFactories,
                    BuildManagementPermissions(), selectedProductCode));
            }

            CheckNotifications();
        }

        private void lnkChangePassword_Click(object sender, RoutedEventArgs e)
        {
            if (_session == null) return;
            var dialog = new ChangePasswordWindow(
                mandatory: false,
                presenterFactory: v => _services.ChangePasswordPresenter(v, _session.PersonId))
            {
                Owner = this
            };
            dialog.ShowDialog();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e) => Close();

        #endregion
    }
}
