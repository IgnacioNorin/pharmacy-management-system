using PharmacySystem.Presentation;
using PharmacySystem.Validators;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PharmacySystem
{
    public partial class frmClient : Form, IClientView
    {
        private readonly ClientPresenter _presenter;

        public frmClient()
        {
            InitializeComponent();
            _presenter = CompositionRoot.CreateClientPresenter(this);
        }

        #region IClientView

        public int SelectedIndex => int.Parse(txtindex.Text);
        public int PersonId => int.Parse(txtid.Text);
        public string Document => txtdocument.Text;
        string IClientView.Name => txtname.Text;
        public string Address => txtaddress.Text;
        public string Phone => txtphone.Text;

        List<string> IClientView.Validate()
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
            MessageBox.Show("¿Desea eliminar el cliente?", "Mensaje", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

        public void LoadClients(IEnumerable<ClientRow> clients)
        {
            foreach (ClientRow row in clients)
            {
                AddRow(row);
            }
        }

        public void AddRow(ClientRow row)
        {
            int rowId = dgdata.Rows.Add();
            WriteRow(dgdata.Rows[rowId], row);
        }

        public void ReplaceRow(int index, ClientRow row) => WriteRow(dgdata.Rows[index], row);

        public void RemoveRow(int index) => dgdata.Rows.RemoveAt(index);

        public void ClearForm() => Clean();

        public void ShowMessage(string message) =>
            MessageBox.Show(message, "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

        public void ShowValidationErrors(IReadOnlyList<string> errors) =>
            MessageBox.Show(string.Join("\n", errors), "Errores de Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        private static void WriteRow(DataGridViewRow gridRow, ClientRow row)
        {
            gridRow.Cells["Id"].Value = row.Id.ToString();
            gridRow.Cells["NumeroDocumento"].Value = row.Document;
            gridRow.Cells["NombreCompleto"].Value = row.Name;
            gridRow.Cells["Direccion"].Value = row.Address;
            gridRow.Cells["Telefono"].Value = row.Phone;
        }

        #endregion

        private Dictionary<TextBox, List<string>> campWithRules = new Dictionary<TextBox, List<string>>();
        private Dictionary<string, string> namesMessages = new Dictionary<string, string>
        {
            { "txtdocument", "Numero Documento" },
            { "txtname", "Nombre Completo" },
            { "txtaddress", "Dirección" },
            { "txtphone", "Teléfono" },
        };
        private void InitializeValidators()
        {
            campWithRules = new Dictionary<TextBox, List<string>>
            {
                { txtdocument, new List<string>{ "NotEmpty", "ValidatorRUC/CI" } },
                { txtname, new List<string>{ "NotEmpty" } },
                { txtaddress, new List<string>{ "NotEmpty" } },
                { txtphone, new List<string>{ "NotEmpty" } },
            };
        }

        private void frmClient_Load(object sender, EventArgs e)
        {
            InitializeValidators();

            DataGridViewButtonColumn Button = new DataGridViewButtonColumn();
            Button.HeaderText = "Seleccionar";
            Button.Width = 80;
            Button.Text = "";
            Button.Name = "btnSeleccionar";
            Button.UseColumnTextForButtonValue = true;

            dgdata.Columns.Add(Button);
            dgdata.Columns.Add("Id", "Id");
            dgdata.Columns.Add("NumeroDocumento", "Numero Documento");
            dgdata.Columns.Add("NombreCompleto", "Nombre Completo");
            dgdata.Columns.Add("Direccion", "Dirección");
            dgdata.Columns.Add("Telefono", "Telefono");

            dgdata.Columns["Id"].Visible = false;

            foreach (DataGridViewColumn cl in dgdata.Columns)
            {
                if (cl.Visible == true && cl.Name != "btnSeleccionar")
                {
                    cbosearch.Items.Add(new PharmacySystem.Model.ComboBoxItem() { Value = cl.Name, Text = cl.HeaderText });
                }
            }
            cbosearch.DisplayMember = "Text";
            cbosearch.ValueMember = "Value";
            cbosearch.SelectedIndex = 0;

            _presenter.OnLoad();

            // Read-only for a role without clientes.gestionar - the presenter also rejects the
            // action, this just greys out the buttons.
            bool canManage = MainForm.Session?.Can("clientes.gestionar") ?? false;
            btnSave.Enabled = canManage;
            btnDelete.Enabled = canManage;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            _presenter.OnSave();
        }

        private void dgdata_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex >= 0){
                string colname = dgdata.Columns[e.ColumnIndex].Name;
                if (colname != "btnSeleccionar")
                {
                    dgdata.Cursor = Cursors.Default;
                }
                else
                {
                    dgdata.Cursor = Cursors.Hand;
                }
            }
        }

        private void dgdata_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if(e.RowIndex < 0)
                return;

            if (e.ColumnIndex == 0)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All);

                var w = Properties.Resources.check20.Width;
                var h = Properties.Resources.check20.Height;
                var x = e.CellBounds.Left + (e.CellBounds.Width - w) / 2;
                var y = e.CellBounds.Top + (e.CellBounds.Height - h) / 2;

                e.Graphics.DrawImage(Properties.Resources.check20, new Rectangle(x, y, w, h));
                e.Handled = true;
            }
        }

        private void btnClean_Click(object sender, EventArgs e)
        {
            Clean();
        }
        private void Clean() {
            txtindex.Text = "0";
            txtid.Text = "0";
            txtdocument.Text = "";
            txtname.Text = "";
            txtaddress.Text = "";
            txtphone.Text = "";
        }

        private void dgdata_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgdata.Columns[e.ColumnIndex].Name == "btnSeleccionar")
            {
                int index = e.RowIndex;
                if (index >= 0)
                {
                    txtindex.Text = (index + 1).ToString();
                    txtid.Text = dgdata.Rows[index].Cells["Id"].Value.ToString();
                    txtdocument.Text = dgdata.Rows[index].Cells["NumeroDocumento"].Value.ToString();
                    txtname.Text = dgdata.Rows[index].Cells["NombreCompleto"].Value.ToString();
                    txtaddress.Text = dgdata.Rows[index].Cells["Direccion"].Value.ToString();
                    txtphone.Text = dgdata.Rows[index].Cells["Telefono"].Value.ToString();
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            _presenter.OnDelete();
        }

        private void btnsearch_Click(object sender, EventArgs e)
        {
            string columnFilter = ((PharmacySystem.Model.ComboBoxItem)cbosearch.SelectedItem).Value.ToString();

            if (dgdata.Rows.Count > 0) {
                foreach (DataGridViewRow row in dgdata.Rows)
                {
                    string valor = row.Cells[columnFilter].Value.ToString().Trim();

                    if (row.Cells[columnFilter].Value.ToString().Trim().Contains(txtsearch.Text.Trim()))
                        row.Visible = true;
                    else
                        row.Visible = false;
                }
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
