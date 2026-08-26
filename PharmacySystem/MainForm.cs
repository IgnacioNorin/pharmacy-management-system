using PharmacySystem.Model;
using PharmacySystem.Presentation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PharmacySystem
{
    public partial class MainForm : Form, IMainFormView
    {
        public static Person oPerson;

        private readonly MainFormPresenter _presenter;
        private IReadOnlyList<ProductAlert> _currentAlerts = new List<ProductAlert>();

        // Every sidebar item ShowForm can highlight as "active" - Salir is excluded on purpose,
        // it isn't a navigation destination. Order doesn't matter, only membership.
        private Button[] NavButtons => new[]
        {
            btnHome, btnSales, btnPurchases, btnManagement, btnSuppliers,
            btnClients, btnUsers, btnReports, btnAlerts
        };

        public MainForm(Person obj = null)
        {
            InitializeComponent();
            oPerson = obj;
            _presenter = CompositionRoot.CreateMainFormPresenter(this);
            InventoryChangeNotifier.StockChanged += OnInventoryChangedElsewhere;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _presenter.OnLoad(oPerson);

            timerNotification.Start();
            // Safety net only - a sale or purchase now triggers an immediate recheck via
            // InventoryChangeNotifier, so this just catches a product crossing its expiration
            // threshold purely because time passed, with nobody having sold or bought anything.
            timerNotification.Interval = 300000; // 5 minutes
            lblAlertBadge.Visible = false;
            LayoutSidebarItems();
            OpenHome();
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

        public void SetUserName(string name) => lbluser.Text = name;

        public void SetAdministrativeMenusVisible(bool visible)
        {
            btnUsers.Visible = visible;
            btnManagement.Visible = visible;
            btnSuppliers.Visible = visible;
            btnReports.Visible = visible;
            btnPurchases.Visible = visible;

            // Hiding/showing items above changes which sidebar rows should butt up against each
            // other - re-stack everything below the highest hidden row.
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
            y = PlaceHeader(lblGroupOperacion, y, headerGapBefore, itemGap);
            y = PlaceItem(btnSales, y, itemGap);
            y = PlaceItem(btnPurchases, y, itemGap);
            y = PlaceHeader(lblGroupGestion, y, headerGapBefore, itemGap);
            y = PlaceItem(btnManagement, y, itemGap);
            y = PlaceItem(btnSuppliers, y, itemGap);
            y = PlaceItem(btnClients, y, itemGap);
            y = PlaceItem(btnUsers, y, itemGap);
            y = PlaceHeader(lblGroupConsulta, y, headerGapBefore, itemGap);
            y = PlaceItem(btnReports, y, itemGap);
            PlaceItem(btnAlerts, y, itemGap);
        }

        private static int PlaceItem(Control control, int y, int gap)
        {
            if (!control.Visible) return y;
            control.Top = y;
            return y + control.Height + gap;
        }

        private static int PlaceHeader(Control header, int y, int gapBefore, int gapAfter)
        {
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
            lblAlertBadge.Visible = unmutedCount > 0;
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
            using (var modal = new ModalAlerts(_currentAlerts, CompositionRoot.NotificationConfigService, oPerson.idPerson))
            {
                if (modal.ShowDialog(this) == DialogResult.OK && !string.IsNullOrEmpty(modal.SelectedProductCode))
                {
                    frmManagement childForm = new frmManagement();
                    ShowForm(childForm, btnManagement);
                    childForm.ShowProductByCode(modal.SelectedProductCode);
                }
            }
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

        private void btnClients_Click(object sender, EventArgs e)
        {
            frmClient childForm = new frmClient();

            ShowForm(childForm, sender);
        }

        private void btnSuppliers_Click(object sender, EventArgs e)
        {
            frmSupplier childForm = new frmSupplier();

            ShowForm(childForm, sender);
        }

        private void btnManagement_Click(object sender, EventArgs e)
        {
            frmManagement childForm = new frmManagement();

            ShowForm(childForm, sender);
        }

        private void btnPurchases_Click(object sender, EventArgs e)
        {
            frmPurchase childForm = new frmPurchase(oPerson.idPerson);

            ShowForm(childForm, sender);
        }

        private void btnSales_Click(object sender, EventArgs e)
        {
            frmSale childForm = new frmSale(oPerson.idPerson);

            ShowForm(childForm, sender);
        }

        private void btnUsers_Click(object sender, EventArgs e)
        {
            frmUser childForm = new frmUser();

            ShowForm(childForm, sender);
        }

        private void btnReports_Click(object sender, EventArgs e)
        {
            frmReport childForm = new frmReport();

            ShowForm(childForm, sender);
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
    }
}
