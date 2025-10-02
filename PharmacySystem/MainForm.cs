using PharmacySystem.Logical;
using PharmacySystem.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PharmacySystem
{
    public partial class MainForm : Form
    {
        public static Person oPerson;

        public MainForm(Person obj = null)
        {
            InitializeComponent();
            oPerson = obj;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            lbluser.Text = oPerson.name;
            if (oPerson.oPersonType.idPersonType == 2)
            {

                usersToolStripMenuItem.Visible = false;
                managementToolStripMenuItem.Visible = false;
                suppliersToolStripMenuItem.Visible = false;
                reportsToolStripMenuItem.Visible = false;
                purchasesToolStripMenuItem.Visible = false;
                notificationsToolStripMenuItem.Visible = false;
            }
            timerNotification.Start();
            timerNotification.Interval = 3000;
            pictureBoxStock.Visible = false;
            pictureBoxExpiredDate.Visible = false;
            frmSale childForm = new frmSale(oPerson.idPerson);
            ShowForm(childForm, salesToolStripMenuItem);
        }


        public void notificationDate()
        {
            NotificationConfigService confignotify = new NotificationConfigService();
            List<DateTime> expiredDates = new List<DateTime>();
            DateTime expirationDate;
            int days = 0;
            days = confignotify.ConfigDay();
            lblnotifyexpireddate.Text = "";
            pictureBoxExpiredDate.Visible = false;
            foreach (var item in confignotify.ListExpirationDate().ToList())
            {
                expirationDate = item.expirationDate.AddDays(-days);

                if (DateTime.Today >= expirationDate)
                {
                    expiredDates.Add(expirationDate);
                    if (expiredDates.Count() >= 1)
                    {
                        lblnotifyexpireddate.Text = "Hay productos con Fechas Vencidas Revise";
                        expiredDates.Clear();
                        pictureBoxExpiredDate.Visible = true;
                    }
                    else
                    {
                        lblnotifyexpireddate.Text = "";
                        expiredDates.Clear();
                    }

                }

            }


        }
        public void notificationStock()
        {
            NotifyIcon notify;
            List<int>  criticalStock = new List<int>();
            NotificationConfigService confignotify = new NotificationConfigService();
            int criticstock = confignotify.ConfigStock();
            int Stock = 0;
            lblnotifystock.Text = "";
            pictureBoxStock.Visible = false;
            foreach (var item in confignotify.ListStock().ToList())
            {
                Stock = item.stock;
                if (Stock <= criticstock)
                {
                    criticalStock.Add(Stock);
                    if (criticalStock.Count() >= 1)
                    {
                        lblnotifystock.Text = "Revise si hay productos con Stock Crítico";
                        criticalStock.Clear();
                        pictureBoxStock.Visible = true;
                    }
                    else
                    {
                        lblnotifystock.Text = "";
                        criticalStock.Clear();
                    }
                }
            }
        }


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
