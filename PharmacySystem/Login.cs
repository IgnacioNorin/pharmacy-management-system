using PharmacySystem.Logical;
using PharmacySystem.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PharmacySystem
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        private void Login_Load(object sender, EventArgs e)
        {
            txtdocument.Focus();
        }

        private void btnEnter_Click(object sender, EventArgs e)
        {
            Person oPerson = PersonService.Instance.ListPerson().Where(p => p.document == txtdocument.Text && p.password == txtpassword.Text && p.oPersonType.idPersonType != 3).FirstOrDefault();
            if (oPerson != null)
            {
                MainForm frm = new MainForm(oPerson);
                frm.Show();
                this.Hide();
                frm.FormClosing += Frm_Closing;
                
            }
            else
            {
                MessageBox.Show("No se econtraron coincidencias del usuario", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void Frm_Closing(object sender, FormClosingEventArgs e)
        {
            txtdocument.Text = "";
            txtpassword.Text = "";
            txtdocument.Focus();
            this.Show();
        }
    }
}
