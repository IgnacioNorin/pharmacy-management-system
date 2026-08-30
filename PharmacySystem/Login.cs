using PharmacySystem.Model;
using PharmacySystem.Presentation;
using System;
using System.Windows.Forms;

namespace PharmacySystem
{
    public partial class Login : Form, ILoginView
    {
        private readonly LoginPresenter _presenter;

        public Login()
        {
            InitializeComponent();
            _presenter = CompositionRoot.CreateLoginPresenter(this);
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
            _presenter.OnLogin();
        }

        string ILoginView.Document => txtdocument.Text;
        string ILoginView.Password => txtpassword.Text;

        public void LoginSucceeded(Person person)
        {
            // Resolve the permission set from the user's role once, here, and hand it to MainForm.
            CurrentUser session = CompositionRoot.CreateCurrentUser(person);
            MainForm frm = new MainForm(session);
            frm.Show();
            this.Hide();
            frm.FormClosing += Frm_Closing;
        }

        public void RequirePasswordChange(Person person)
        {
            using (var modal = new ModalChangePassword(person.idPerson, mandatory: true))
            {
                if (modal.ShowDialog(this) == DialogResult.OK)
                {
                    LoginSucceeded(person);
                }
                else
                {
                    txtpassword.Text = "";
                    txtpassword.Focus();
                }
            }
        }

        public void ShowError(string message) =>
            MessageBox.Show(message, "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

        private void Frm_Closing(object sender, FormClosingEventArgs e)
        {
            txtdocument.Text = "";
            txtpassword.Text = "";
            txtdocument.Focus();
            this.Show();
        }
    }
}
