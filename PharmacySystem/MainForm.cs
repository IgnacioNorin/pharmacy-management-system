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
            pictureBoxStock.Visible = false;
            pictureBoxExpiredDate.Visible = false;
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
        }

        // Fase 3 of the alerts rework: one itemized, severity-ranked list instead of two generic
        // sentences. Still rendered through the same two labels/icons for now - a dedicated
        // notification-center panel (with per-product click-through) is the next step, not yet
        // wired up here.
        public void ShowAlerts(IReadOnlyList<ProductAlert> alerts)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => ShowAlerts(alerts)));
                return;
            }

            _currentAlerts = alerts;

            List<ProductAlert> stockAlerts = alerts
                .Where(a => a.Severity == AlertSeverity.Critical || a.Severity == AlertSeverity.Low)
                .ToList();
            List<ProductAlert> expirationAlerts = alerts
                .Where(a => a.Severity == AlertSeverity.Expired || a.Severity == AlertSeverity.ExpiringSoon)
                .ToList();

            pictureBoxStock.Visible = stockAlerts.Count > 0;
            lblnotifystock.Text = FormatAlertSummary("Stock: ", stockAlerts);

            pictureBoxExpiredDate.Visible = expirationAlerts.Count > 0;
            lblnotifyexpireddate.Text = FormatAlertSummary("Vencimientos: ", expirationAlerts);
        }

        private static string FormatAlertSummary(string prefix, List<ProductAlert> alerts)
        {
            if (alerts.Count == 0) return "";

            const int maxNamed = 3;
            string names = string.Join(", ", alerts.Take(maxNamed).Select(a => a.Name));
            string overflow = alerts.Count > maxNamed ? $" (+{alerts.Count - maxNamed})" : "";
            return prefix + names + overflow;
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
            checkNotifications();
            frmClient childForm = new frmClient();

            ShowForm(childForm, sender);
        }

        private void suppliersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            checkNotifications();
            frmSupplier childForm = new frmSupplier();

            ShowForm(childForm, sender);
        }


        private void managementToolStripMenuItem_Click(object sender, EventArgs e)
        {
            checkNotifications();
            frmManagement childForm = new frmManagement();

            ShowForm(childForm, sender);
        }


        private void purchasesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            checkNotifications();
            frmPurchase childForm = new frmPurchase(oPerson.idPerson);

            ShowForm(childForm, sender);
        }

        private void salesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            checkNotifications();
            frmSale childForm = new frmSale(oPerson.idPerson);

            ShowForm(childForm, sender);
        }

        private void usersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            checkNotifications();
            frmUser childForm = new frmUser();

            ShowForm(childForm, sender);
        }


        private void reportsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            checkNotifications();
            frmReport childForm = new frmReport();

            ShowForm(childForm, sender);
        }

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
