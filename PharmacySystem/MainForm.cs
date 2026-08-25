using PharmacySystem.Model;
using PharmacySystem.Presentation;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PharmacySystem
{
    public partial class MainForm : Form, IMainFormView
    {
        public static Person oPerson;

        private readonly MainFormPresenter _presenter;

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
        private void OnInventoryChangedElsewhere()
        {
            notificationDate();
            notificationStock();
        }

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

        public void ShowExpirationWarning(bool visible, string message)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => ShowExpirationWarning(visible, message)));
                return;
            }

            lblnotifyexpireddate.Text = message;
            pictureBoxExpiredDate.Visible = visible;
        }

        public void ShowStockWarning(bool visible, string message)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => ShowStockWarning(visible, message)));
                return;
            }

            lblnotifystock.Text = message;
            pictureBoxStock.Visible = visible;
        }

        // Both queries now hit an indexed, server-filtered SELECT (Fase 1), but they still cross
        // the network - running them off the UI thread keeps the window responsive on the 5-minute
        // timer tick and on every menu navigation, instead of freezing on a slow connection.
        private void notificationDate() => Task.Run(() => _presenter.CheckExpirationWarnings());

        private void notificationStock() => Task.Run(() => _presenter.CheckStockWarnings());

        private void clientsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notificationDate();
            frmClient childForm = new frmClient();

            ShowForm(childForm, sender);
        }

        private void suppliersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notificationDate();
            frmSupplier childForm = new frmSupplier();

            ShowForm(childForm, sender);
        }


        private void managementToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notificationDate();
            frmManagement childForm = new frmManagement();

            ShowForm(childForm, sender);
        }


        private void purchasesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notificationDate();
            frmPurchase childForm = new frmPurchase(oPerson.idPerson);

            ShowForm(childForm, sender);
        }

        private void salesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notificationDate();
            frmSale childForm = new frmSale(oPerson.idPerson);

            ShowForm(childForm, sender);
        }

        private void usersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notificationDate();
            frmUser childForm = new frmUser();

            ShowForm(childForm, sender);
        }


        private void reportsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notificationDate();
            frmReport childForm = new frmReport();

            ShowForm(childForm, sender);
        }

        private void ShowForm(Form form, object senderitem)
        {
            notificationDate();
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
            notificationDate();
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
            notificationDate();
            notificationStock();
        }
    }
}
