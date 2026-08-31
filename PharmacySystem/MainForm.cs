using PharmacySystem.Model;
using PharmacySystem.Presentation;
using PharmacySystem.Wpf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PharmacySystem
{
    public partial class MainForm : Form, IMainFormView
    {
        // The signed-in user and their resolved permission set. Child forms still read oPerson
        // for the person id; anything gating on permissions uses Session.
        public static CurrentUser Session;
        public static Person oPerson;

        private readonly MainFormPresenter _presenter;
        private IReadOnlyList<ProductAlert> _currentAlerts = new List<ProductAlert>();

        // Session-refresh throttle (DEF-21): OnActivated fires on every return of focus.
        private bool _sessionRefreshReady;
        private DateTime _lastSessionRefresh = DateTime.MinValue;

        // Every sidebar item ShowForm can highlight as "active" - Salir is excluded on purpose,
        // it isn't a navigation destination. Order doesn't matter, only membership.
        private Button[] NavButtons => new[]
        {
            btnHome, btnSales, btnPurchases, btnManagement, btnSuppliers,
            btnClients, btnUsers, btnRoles, btnReports, btnCashCount, btnAuditLog, btnAlerts
        };

        public MainForm(CurrentUser session = null)
        {
            InitializeComponent();
            Session = session;
            oPerson = session?.Person;
            _presenter = CompositionRoot.CreateMainFormPresenter(this);
            InventoryChangeNotifier.StockChanged += OnInventoryChangedElsewhere;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (Session != null)
            {
                _presenter.OnLoad(Session);
            }

            // Safety net only - a sale or purchase now triggers an immediate recheck via
            // InventoryChangeNotifier, so this just catches a product crossing its expiration
            // threshold purely because time passed, with nobody having sold or bought anything.
            // Interval is set before Start so the first tick already uses the 5-minute value
            // instead of the designer default (DEF-34).
            timerNotification.Interval = 300000; // 5 minutes
            timerNotification.Start();
            lblAlertBadge.Visible = false;
            AddChangePasswordLink();
            LayoutSidebarItems();
            OpenHome();
            _sessionRefreshReady = true;
        }

        // Re-checks the session whenever focus returns to the main window - e.g. after closing a
        // child form. A permission revoked by an admin in another session then takes effect on
        // the next navigation instead of only after logout (DEF-21). Throttled, and skips the
        // burst of activations during construction.
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            if (!_sessionRefreshReady || Session == null) return;
            if ((DateTime.Now - _lastSessionRefresh).TotalSeconds < 3) return;
            _lastSessionRefresh = DateTime.Now;

            CurrentUser refreshed = _presenter.RefreshSession(Session);
            if (refreshed == null)
            {
                _sessionRefreshReady = false;
                MessageBox.Show(
                    "Su sesión ya no es válida: la cuenta fue desactivada o eliminada.",
                    "Sesión finalizada", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
                return;
            }

            Session = refreshed;
            oPerson = refreshed.Person;
            _presenter.OnLoad(Session); // re-applies the sidebar to the current permission set
        }

        // Built in code (not in the Designer): a small "Cambiar contraseña" link in the sidebar
        // header, always available to any signed-in user regardless of role.
        private void AddChangePasswordLink()
        {
            if (Session == null) return;

            var link = new LinkLabel
            {
                Text = "Cambiar contraseña",
                AutoSize = true,
                Location = new Point(14, 72),
                LinkColor = Color.FromArgb(141, 169, 196),
                ActiveLinkColor = Color.White,
                Font = new Font("Segoe UI", 8F, FontStyle.Regular)
            };
            link.LinkClicked += (s, e) =>
            {
                // First screen ported to WPF. The presenter/service are unchanged - the WPF
                // window implements the same IChangePasswordView. The login-forced path still
                // uses the WinForms ModalChangePassword for now.
                ChangePasswordDialog.Show(
                    Handle,
                    mandatory: false,
                    presenterFactory: view => CompositionRoot.CreateChangePasswordPresenter(view, Session.PersonId));
            };
            pnlSidebarHeader.Controls.Add(link);
            link.BringToFront();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            InventoryChangeNotifier.StockChanged -= OnInventoryChangedElsewhere;
            base.OnFormClosed(e);
        }

        // Raised from SalePresenter/PurchasePresenter after a successful register, on whatever
        // thread called them (normally the UI thread, from a button click) - dispatch through the
        // same non-blocking path as every other alert check instead of assuming the caller's thread.
        private void OnInventoryChangedElsewhere() => checkNotifications();

        public void SetUserName(string name, string role)
        {
            lbluser.Text = name;
            lblUserRole.Text = role;
        }

        public void ApplySidebarPermissions(SidebarPermissions p)
        {
            btnSales.Visible = p.Sales;
            btnPurchases.Visible = p.Purchases;
            btnClients.Visible = p.Clients;
            btnSuppliers.Visible = p.Suppliers;
            btnManagement.Visible = p.Management;
            btnUsers.Visible = p.Users;
            btnRoles.Visible = p.Roles;
            btnReports.Visible = p.Reports;
            btnCashCount.Visible = p.CashCount;
            btnAuditLog.Visible = p.AuditLog;
            btnAlerts.Visible = p.Alerts;
            if (!p.Alerts) lblAlertBadge.Visible = false;

            // Hiding/showing items above changes which sidebar rows should butt up against each
            // other - re-stack everything (and hide a group header whose items are all hidden).
            LayoutSidebarItems();
        }

        // Fase 7 (navigation rework): stacks the visible sidebar items top-to-bottom, skipping
        // hidden ones instead of leaving a gap - the plain-Panel equivalent of what the old
        // MenuStrip's HorizontalStackWithOverflow did automatically. Every role keeps at least one
        // item per group, so group headers are never hidden.
        private void LayoutSidebarItems()
        {
            const int itemGap = 6;
            const int headerGapBefore = 8;
            int y = 14;

            y = PlaceItem(btnHome, y, itemGap);
            y = PlaceGroupHeader(lblGroupOperacion, y, headerGapBefore, itemGap, btnSales, btnPurchases);
            y = PlaceItem(btnSales, y, itemGap);
            y = PlaceItem(btnPurchases, y, itemGap);
            y = PlaceGroupHeader(lblGroupGestion, y, headerGapBefore, itemGap, btnManagement, btnSuppliers, btnClients, btnUsers, btnRoles);
            y = PlaceItem(btnManagement, y, itemGap);
            y = PlaceItem(btnSuppliers, y, itemGap);
            y = PlaceItem(btnClients, y, itemGap);
            y = PlaceItem(btnUsers, y, itemGap);
            y = PlaceItem(btnRoles, y, itemGap);
            y = PlaceGroupHeader(lblGroupConsulta, y, headerGapBefore, itemGap, btnReports, btnCashCount, btnAuditLog, btnAlerts);
            y = PlaceItem(btnReports, y, itemGap);
            y = PlaceItem(btnCashCount, y, itemGap);
            y = PlaceItem(btnAuditLog, y, itemGap);
            PlaceItem(btnAlerts, y, itemGap);
        }

        private static int PlaceItem(Control control, int y, int gap)
        {
            if (!control.Visible) return y;
            control.Top = y;
            return y + control.Height + gap;
        }

        // Hides the group header when every item in the group is hidden (a custom role can now
        // leave a whole group with nothing in it), otherwise places it like before.
        private static int PlaceGroupHeader(Control header, int y, int gapBefore, int gapAfter, params Control[] items)
        {
            bool anyVisible = items.Any(c => c.Visible);
            header.Visible = anyVisible;
            if (!anyVisible) return y;

            header.Top = y + gapBefore;
            return header.Top + header.Height + gapAfter;
        }

        // Fase 6 of the alerts rework: a single bell icon with a badge count instead of two
        // always-on text summaries - the itemized list already lives one click away in
        // ModalAlerts, so the header only needs to say "how many", not "which ones".
        public void ShowAlerts(IReadOnlyList<ProductAlert> alerts)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => ShowAlerts(alerts)));
                return;
            }

            _currentAlerts = alerts;

            // Fase 5 (mute): a muted alert still shows in the notification center (ModalAlerts),
            // it just stops counting toward the badge - that's the whole point of muting it.
            int unmutedCount = alerts.Count(a => a.MutedAt == null);

            lblAlertBadge.Text = unmutedCount > 99 ? "99+" : unmutedCount.ToString();
            // No badge for a user whose role can't open the notification center.
            lblAlertBadge.Visible = btnAlerts.Visible && unmutedCount > 0;
        }

        // The query now hits an indexed, server-filtered SELECT (Fase 1), but it still crosses the
        // network - running it off the UI thread keeps the window responsive on the 5-minute timer
        // tick and on every menu navigation, instead of freezing on a slow connection.
        private void checkNotifications() => Task.Run(() => _presenter.RefreshAlerts());

        // Notification center (Fase 3): shows the itemized alert list computed by the last
        // RefreshAlerts() call, and click-through navigates straight to the flagged product in
        // frmManagement's existing search, instead of leaving the user to find it manually.
        private void OpenAlertsCenter(object sender, EventArgs e)
        {
            if (!CanNavigate("alertas.acceso")) return;

            using (var modal = new ModalAlerts(_currentAlerts, CompositionRoot.NotificationConfigService, oPerson.idPerson))
            {
                // The click-through opens the Producto tab in frmManagement; skip it for a role
                // that can see alerts but not the products section (that tab is not loaded).
                bool canOpenProduct = Session?.Can("productos.acceso") ?? false;
                if (modal.ShowDialog(this) == DialogResult.OK && canOpenProduct && !string.IsNullOrEmpty(modal.SelectedProductCode))
                {
                    frmManagement childForm = new frmManagement();
                    ShowForm(childForm, btnManagement);
                    childForm.ShowProductByCode(modal.SelectedProductCode);
                }
            }

            // Acknowledging or muting inside the center changes what should count toward the
            // badge; re-fetch now instead of waiting for the 5-minute tick (DEF-16).
            checkNotifications();
        }

        private void btnHome_Click(object sender, EventArgs e) => OpenHome();

        // Landing screen: reuses the same GetActiveAlerts() computation the bell badge already
        // ran, plus today's sales. Navigation from its tiles/buttons is handed in as callbacks
        // that just call the existing menu handlers, instead of duplicating their logic.
        private void OpenHome()
        {
            frmHome childForm = new frmHome(
                () => btnSales_Click(btnSales, EventArgs.Empty),
                () => btnPurchases_Click(btnPurchases, EventArgs.Empty),
                () => btnManagement_Click(btnManagement, EventArgs.Empty),
                code =>
                {
                    frmManagement mgmt = new frmManagement();
                    ShowForm(mgmt, btnManagement);
                    mgmt.ShowProductByCode(code);
                });

            ShowForm(childForm, btnHome);
        }

        // Navigation gate. The sidebar already hides what the role cannot reach, but frmHome's
        // quick-access buttons call these same handlers through callbacks - this keeps a hidden
        // destination unreachable even if something triggers its handler. No session -> deny:
        // in the real app there is always a session by this point (DEF-23).
        private static bool CanNavigate(string permission) => Session?.Can(permission) ?? false;

        private void btnClients_Click(object sender, EventArgs e)
        {
            if (!CanNavigate("clientes.acceso")) return;

            frmClient childForm = new frmClient();

            ShowForm(childForm, sender);
        }

        private void btnSuppliers_Click(object sender, EventArgs e)
        {
            if (!CanNavigate("proveedores.acceso")) return;

            frmSupplier childForm = new frmSupplier();

            ShowForm(childForm, sender);
        }

        private void btnManagement_Click(object sender, EventArgs e)
        {
            if (!(CanNavigate("productos.acceso") || CanNavigate("categorias.acceso") || CanNavigate("tienda.acceso")))
            {
                return;
            }

            frmManagement childForm = new frmManagement();

            ShowForm(childForm, sender);
        }

        private void btnPurchases_Click(object sender, EventArgs e)
        {
            if (!CanNavigate("compras.acceso")) return;

            frmPurchase childForm = new frmPurchase(oPerson.idPerson);

            ShowForm(childForm, sender);
        }

        private void btnSales_Click(object sender, EventArgs e)
        {
            if (!CanNavigate("ventas.acceso")) return;

            frmSale childForm = new frmSale(oPerson.idPerson);

            ShowForm(childForm, sender);
        }

        private void btnUsers_Click(object sender, EventArgs e)
        {
            if (!CanNavigate("usuarios.acceso")) return;

            frmUser childForm = new frmUser();

            ShowForm(childForm, sender);
        }

        private void btnRoles_Click(object sender, EventArgs e)
        {
            if (!CanNavigate("roles.gestionar")) return;

            frmRoles childForm = new frmRoles();

            ShowForm(childForm, sender);
        }

        private void btnReports_Click(object sender, EventArgs e)
        {
            if (!CanNavigate("reportes.acceso")) return;

            frmReport childForm = new frmReport();

            ShowForm(childForm, sender);
        }

        private void btnCashCount_Click(object sender, EventArgs e)
        {
            if (!CanNavigate("caja.acceso")) return;

            using (var modal = new ModalCashCount())
            {
                modal.ShowDialog(this);
            }
        }

        private void btnAuditLog_Click(object sender, EventArgs e)
        {
            if (!CanNavigate("bitacora.acceso")) return;

            using (var modal = new ModalSecurityLog())
            {
                modal.ShowDialog(this);
            }
        }

        // ShowForm already calls checkNotifications() for every navigation below - each handler
        // above used to call it again first, which fired two overlapping RefreshAlerts() calls per
        // click and raced inside SyncAlertHistory (see the lock in NotificationConfigService).
        private void ShowForm(Form form, object senderitem)
        {
            checkNotifications();
            foreach (Form frm in this.MdiChildren)
            {
                frm.Close();
            }

            foreach (Button navButton in NavButtons)
            {
                navButton.BackColor = Color.FromArgb(11, 37, 69);
                navButton.ForeColor = Color.White;
            }

            ((Button)senderitem).BackColor = Color.FromArgb(141, 169, 196);

            // A maximized MDI child's minimize/restore/close buttons normally merge into the
            // parent's MainMenuStrip; without one (the sidebar replaced it), Windows draws them as
            // a floating row over the MDI area instead - ControlBox=false on the child doesn't
            // suppress that, since MdiClient draws it, not the child form itself, and combining
            // FormBorderStyle.None with WindowState.Maximized (tried first) breaks native MDI
            // maximize layout instead of fixing it, leaving every child's controls mispositioned.
            // Docking the child to fill the MDI area is a different mechanism entirely - the form
            // is never actually put into the native maximized state, so there is no merge box to
            // draw in the first place.
            form.FormBorderStyle = FormBorderStyle.None;
            form.MdiParent = this;
            form.Dock = DockStyle.Fill;
            form.Show();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Desea Salir?", "Mensaje", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void timerNotification_Tick(object sender, EventArgs e)
        {
            checkNotifications();

        }

        #region Custom title bar (Fase 8)

        // FormBorderStyle.None removes every bit of native chrome - caption, system menu, resize
        // borders, minimize/maximize/close - so pnlTitleBar and everything below replaces it by
        // hand. WM_GETMINMAXINFO stops a maximized borderless window from overhanging the taskbar
        // (a well-known side effect of FormBorderStyle.None with no border to clip it).
        //
        // Dragging is NOT done via WM_NCHITTEST on the Form's own WndProc - pnlTitleBar and
        // lblTitleBarText are themselves real child windows (Panel/Label both have their own
        // HWND), so Windows delivers WM_NCHITTEST/mouse messages to THEM, and MainForm's WndProc
        // never sees it for that area. ReleaseCapture + SendMessage(WM_NCLBUTTONDOWN, HTCAPTION)
        // sidesteps that entirely by asking the OS to treat the current drag as if it had started
        // on a native caption - Aero Snap included, and no per-pixel mouse tracking needed.
        private const int WM_GETMINMAXINFO = 0x0024;
        private const int WM_NCLBUTTONDOWN = 0x00A1;
        private const int HTCAPTION = 2;

        [StructLayout(LayoutKind.Sequential)]
        private struct POINTAPI { public int X; public int Y; }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int Left; public int Top; public int Right; public int Bottom; }

        [StructLayout(LayoutKind.Sequential)]
        private struct MINMAXINFO
        {
            public POINTAPI ptReserved;
            public POINTAPI ptMaxSize;
            public POINTAPI ptMaxPosition;
            public POINTAPI ptMinTrackSize;
            public POINTAPI ptMaxTrackSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public int dwFlags;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_GETMINMAXINFO)
            {
                base.WndProc(ref m);
                ClampMaximizedSizeToWorkArea(ref m);
                return;
            }

            base.WndProc(ref m);
        }

        private void ClampMaximizedSizeToWorkArea(ref Message m)
        {
            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(m.LParam, typeof(MINMAXINFO));

            IntPtr monitor = MonitorFromWindow(Handle, 2 /* MONITOR_DEFAULTTONEAREST */);
            if (monitor != IntPtr.Zero)
            {
                var monitorInfo = new MONITORINFO { cbSize = Marshal.SizeOf(typeof(MONITORINFO)) };
                // A failed call leaves monitorInfo zeroed - falling through to StructureToPtr
                // with that would send ptMaxSize (0,0) and maximize the window into nothing.
                if (!GetMonitorInfo(monitor, ref monitorInfo))
                {
                    Marshal.StructureToPtr(mmi, m.LParam, true);
                    return;
                }

                RECT work = monitorInfo.rcWork;
                RECT bounds = monitorInfo.rcMonitor;

                mmi.ptMaxPosition.X = Math.Abs(work.Left - bounds.Left);
                mmi.ptMaxPosition.Y = Math.Abs(work.Top - bounds.Top);
                mmi.ptMaxSize.X = Math.Abs(work.Right - work.Left);
                mmi.ptMaxSize.Y = Math.Abs(work.Bottom - work.Top);
            }

            Marshal.StructureToPtr(mmi, m.LParam, true);
        }

        private void pnlTitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            // e.Clicks > 1 is the second MouseDown of a double-click: handing that one to the OS
            // drag loop too swallows its matching MouseUp before WinForms can pair the two clicks,
            // so pnlTitleBar_DoubleClick never fires and maximize-by-double-click goes silent.
            if (e.Button != MouseButtons.Left || e.Clicks > 1) return;

            ReleaseCapture();
            SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
        }

        private void pnlTitleBar_DoubleClick(object sender, EventArgs e) => ToggleMaximizeRestore();

        private void btnMinimizeWin_Click(object sender, EventArgs e) => WindowState = FormWindowState.Minimized;

        private void btnMaximizeRestore_Click(object sender, EventArgs e) => ToggleMaximizeRestore();

        private void ToggleMaximizeRestore()
        {
            WindowState = WindowState == FormWindowState.Maximized
                ? FormWindowState.Normal
                : FormWindowState.Maximized;

            //  = restore-down glyph,  = maximize glyph (Segoe MDL2 Assets) - swapped
            // to match whichever action the button performs next, same as the native title bar.
            btnMaximizeRestore.Text = WindowState == FormWindowState.Maximized ? "" : "";
        }

        #endregion
    }
}
