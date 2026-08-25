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
            RepositionAlertBadge();
            frmSale childForm = new frmSale(oPerson.idPerson);
            ShowForm(childForm, salesToolStripMenuItem);
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
            usersToolStripMenuItem.Visible = visible;
            managementToolStripMenuItem.Visible = visible;
            suppliersToolStripMenuItem.Visible = visible;
            reportsToolStripMenuItem.Visible = visible;
            purchasesToolStripMenuItem.Visible = visible;
            notificationsToolStripMenuItem.Visible = visible;

            // Hiding/showing sibling items reflows msMenu's horizontal stack, which moves
            // alertBellToolStripMenuItem - the alert bell itself stays visible for every user
            // (everyone should see stock/expiration alerts, unlike the admin-only config screens
            // above), so its badge needs to follow wherever the item lands.
            RepositionAlertBadge();
        }

        // The badge is a plain Label floating on top of the Form, not a child of msMenu - a
        // ToolStripItem can't host a nested control, so this is the only way to draw a number in
        // its corner. msMenu isn't anchored (fixed size regardless of window resize), so the only
        // thing that moves alertBellToolStripMenuItem is sibling visibility changing above.
        private void RepositionAlertBadge()
        {
            Rectangle bounds = alertBellToolStripMenuItem.Bounds;
            lblAlertBadge.Location = new Point(bounds.Right - lblAlertBadge.Width - 6, bounds.Top + 4);
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
                    ShowForm(childForm, managementToolStripMenuItem);
                    childForm.ShowProductByCode(modal.SelectedProductCode);
                }
            }
        }

        private void clientsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmClient childForm = new frmClient();

            ShowForm(childForm, sender);
        }

        private void suppliersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmSupplier childForm = new frmSupplier();

            ShowForm(childForm, sender);
        }


        private void managementToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmManagement childForm = new frmManagement();

            ShowForm(childForm, sender);
        }


        private void purchasesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmPurchase childForm = new frmPurchase(oPerson.idPerson);

            ShowForm(childForm, sender);
        }

        private void salesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmSale childForm = new frmSale(oPerson.idPerson);

            ShowForm(childForm, sender);
        }

        private void usersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmUser childForm = new frmUser();

            ShowForm(childForm, sender);
        }


        private void reportsToolStripMenuItem_Click(object sender, EventArgs e)
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

            foreach (ToolStripMenuItem menu in msMenu.Items)
            {
                menu.BackColor = System.Drawing.Color.FromArgb(11, 37, 69);
                menu.ForeColor = Color.White;
            }

           ((ToolStripMenuItem)senderitem).BackColor = System.Drawing.Color.FromArgb(141, 169, 196);

            form.MdiParent = this;
            form.WindowState = FormWindowState.Maximized;
            form.Show();
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            checkNotifications();
            ModalConfignotification frm = new ModalConfignotification();
            frm.ShowDialog();
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
