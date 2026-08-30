using PharmacySystem.Model;
using PharmacySystem.Presentation;
using PharmacySystem.Validators;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PharmacySystem
{
    public partial class frmUser : Form, IUserView
    {
        private readonly UserPresenter _presenter;

        public frmUser()
        {
            InitializeComponent();
            _presenter = CompositionRoot.CreateUserPresenter(this);
        }

        #region IUserView

        public int SelectedIndex => int.Parse(txtindex.Text);
        public int RowCount => dgdata.Rows.Count;
        public int UserId => int.Parse(txtid.Text);
        public string Document => txtdocument.Text;
        string IUserView.Name => txtname.Text;
        public string Password => txtpassword.Text;
        public string ConfirmPassword => txtconfirmpassword.Text;
        public int RoleId => Convert.ToInt32(((ComboBoxItem)cborol.SelectedItem).Value.ToString());
        public string RoleText => ((ComboBoxItem)cborol.SelectedItem).Text;

        List<string> IUserView.Validate()
        {
            var errors = new List<string>();

            foreach (var camp in campWithRules)
            {
                foreach (var ruleName in camp.Value)
                {
                    var rule = Validations.rules[ruleName];
                    if (!rule.Validate(camp.Key.Text))
                    {
                        errors.Add($"{namesMessages[camp.Key.Name]} : {rule.MessageError}");
                    }
                }
            }

            return errors;
        }

        public bool ConfirmDelete() =>
            MessageBox.Show("¿Desea eliminar el usuario?", "Mensaje", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

        public void LoadRoleOptions(IEnumerable<ComboBoxItem> options)
        {
            cborol.Items.Clear();
            foreach (ComboBoxItem option in options)
            {
                cborol.Items.Add(option);
            }
            if (cborol.Items.Count > 0)
            {
                cborol.SelectedIndex = 0;
            }
        }

        public void LoadUsers(IEnumerable<UserRow> users)
        {
            dgdata.Rows.Clear();
            foreach (UserRow row in users)
            {
                AddRow(row);
            }
        }

        public void AddRow(UserRow row)
        {
            int rowId = dgdata.Rows.Add();
            WriteRow(dgdata.Rows[rowId], row);
        }

        public void ReplaceRow(int index, UserRow row) => WriteRow(dgdata.Rows[index], row);

        public void RemoveRow(int index) => dgdata.Rows.RemoveAt(index);

        public void ClearForm() => Clean();

        public void ShowMessage(string message) =>
            MessageBox.Show(message, "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

        public void ShowValidationErrors(IReadOnlyList<string> errors) =>
            MessageBox.Show(string.Join("\n", errors), "Errores de Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        public void ShowPasswordMismatch() =>
            MessageBox.Show("Las contraseñas no coinciden\nRevise nuevamente", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

        private static void WriteRow(DataGridViewRow gridRow, UserRow row)
        {
            gridRow.Cells["Id"].Value = row.Id.ToString();
            gridRow.Cells["NumeroDocumento"].Value = row.Document;
            gridRow.Cells["NombreCompleto"].Value = row.Name;
            gridRow.Cells["Rol"].Value = row.RoleText;
            gridRow.Cells["Estado"].Value = row.StatusText;

            DataGridViewCell estado = gridRow.Cells["Estado"];
            switch (row.StatusText)
            {
                case "Bloqueado":
                    estado.Style.ForeColor = Color.Firebrick;
                    estado.Style.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                    break;
                case "Inactivo":
                    estado.Style.ForeColor = Color.Gray;
                    break;
                default:
                    estado.Style.ForeColor = Color.SeaGreen;
                    break;
            }
        }

        #endregion

        private Dictionary<TextBox, List<string>> campWithRules = new Dictionary<TextBox, List<string>>();
        private Dictionary<string, string> namesMessages = new Dictionary<string, string>
        {
            { "txtdocument", "Numero Documento" },
            { "txtname", "Nombre Completo" },
            { "txtpassword", "Contraseña" },
            { "txtconfirmpassword", "Confirmar Contraseña" },
        };
        private void InitializeValidators()
        {
            campWithRules = new Dictionary<TextBox, List<string>>
            {
                { txtdocument, new List<string>{ "NotEmpty", "ValidateDocument" } },
                { txtname, new List<string>{ "NotEmpty" } },
                // Password fields are not force-validated here: UserPresenter requires a password
                // only for a new user; on an edit a blank field means "keep the current one".
            };
        }


        private void frmUser_Load(object sender, EventArgs e)
        {
            // Initializes validations for the form

            InitializeValidators();

            // The role list comes from person_type now (built-ins + any custom role), loaded by
            // the presenter via LoadRoleOptions.
            cborol.DisplayMember = "Text";
            cborol.ValueMember = "Value";

            DataGridViewButtonColumn Button = new DataGridViewButtonColumn()
            {
                HeaderText = "Seleccionar",
                Width = 80,
                Text = "",
                Name = "btnSeleccionar",
                UseColumnTextForButtonValue = true,
            };


            dgdata.Columns.Add(Button);
            dgdata.Columns.Add("Id", "Id");
            dgdata.Columns.Add("NumeroDocumento", "Numero Documento");
            dgdata.Columns.Add("NombreCompleto", "Nombre Completo");
            dgdata.Columns.Add("Rol", "Rol");
            dgdata.Columns.Add("Estado", "Estado");

            dgdata.Columns["btnSeleccionar"].Width = 80;
            dgdata.Columns["NumeroDocumento"].Width = 150;
            dgdata.Columns["NombreCompleto"].Width = 260;
            dgdata.Columns["Rol"].Width = 240;
            dgdata.Columns["Estado"].Width = 100;
            dgdata.Columns["Id"].Visible = false;

            foreach (DataGridViewColumn cl in dgdata.Columns)
            {
                if (cl.Visible == true && cl.Name != "btnSeleccionar")
                {
                    cbosearch.Items.Add(new ComboBoxItem() { Value = cl.Name, Text = cl.HeaderText });
                }
            }
            cbosearch.DisplayMember = "Text";
            cbosearch.ValueMember = "Value";
            cbosearch.SelectedIndex = 0;

            _presenter.OnLoad();

            bool canManage = MainForm.Session?.Can("usuarios.gestionar") ?? false;
            btnSave.Enabled = canManage;
            btnDelete.Enabled = canManage;
            btnResetPassword.Enabled = canManage;
            btnUnlock.Enabled = canManage;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            _presenter.OnSave();
        }

        private void btnResetPassword_Click(object sender, EventArgs e)
        {
            _presenter.OnResetPassword();
        }

        private void btnUnlock_Click(object sender, EventArgs e)
        {
            _presenter.OnUnlockUser();
        }

        private void dgdata_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex < 0) return;

            dgdata.Cursor = dgdata.Columns[e.ColumnIndex].Name == "btnSleccionar"
                        ? Cursors.Hand
                        : Cursors.Default;


        }

        private void dgdata_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 0) return;


            e.Paint(e.CellBounds, DataGridViewPaintParts.All);

            var w = Properties.Resources.check20.Width;
            var h = Properties.Resources.check20.Height;
            var x = e.CellBounds.Left + (e.CellBounds.Width - w) / 2;
            var y = e.CellBounds.Top + (e.CellBounds.Height - h) / 2;

            e.Graphics.DrawImage(Properties.Resources.check20, new Rectangle(x, y, w, h));
            e.Handled = true;

        }

        private void btnClean_Click(object sender, EventArgs e)
        {
            Clean();
        }

        private void Clean()
        {
            txtindex.Text = "0";
            txtid.Text = "0";
            txtdocument.Text = "";
            txtname.Text = "";
            txtpassword.Text = "";
            txtconfirmpassword.Text = "";
            if(cborol.SelectedValue != null)
            {
                cborol.SelectedIndex = 0;
            }
        }

        private void dgdata_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            int index = e.RowIndex;
            int item_index;
            if (dgdata.Columns[e.ColumnIndex].Name != "btnSeleccionar" || index < 0) return;

            txtindex.Text = (index + 1).ToString();
            txtid.Text = dgdata.Rows[index].Cells["Id"].Value.ToString();
            txtdocument.Text = dgdata.Rows[index].Cells["NumeroDocumento"].Value.ToString();
            txtname.Text = dgdata.Rows[index].Cells["NombreCompleto"].Value.ToString();
            // Leave the password fields blank on select: an edit that does not touch them keeps
            // the user's current password (see UserPresenter / sp_update_person).
            txtpassword.Text = "";
            txtconfirmpassword.Text = "";
            foreach (ComboBoxItem item in cborol.Items)
            {
                if (item.Text == dgdata.Rows[index].Cells["Rol"].Value.ToString())
                {
                    item_index = cborol.Items.IndexOf(item);
                    cborol.SelectedIndex = item_index;
                    break;
                }
            }




        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            _presenter.OnDelete();
        }

        private void btnsearch_Click(object sender, EventArgs e)
        {
            string columnFilter = ((ComboBoxItem)cbosearch.SelectedItem).Value.ToString();
            string value;

            if (dgdata.Rows.Count <= 0) return;

            foreach (DataGridViewRow row in dgdata.Rows)
            {
                value = row.Cells[columnFilter].Value.ToString().Trim();

                if (row.Cells[columnFilter].Value.ToString().Trim().Contains(txtsearch.Text.Trim()))
                    row.Visible = true;
                else
                    row.Visible = false;
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtsearch.Text = "";
            foreach (DataGridViewRow row in dgdata.Rows)
            {
                row.Visible = true;
            }
        }
    }
}
