using PharmacySystem.Presentation;
using PharmacySystem.Validators;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PharmacySystem
{
    // Pilot migration to MVP: this Form only reads/writes its own controls and forwards user
    // intent to SupplierPresenter, which owns every decision (which message, which row changes,
    // whether the form clears). Grid painting and the free-text search filter stay here - they
    // never touched SupplierService even before this migration, so they aren't presenter concerns.
    public partial class frmSupplier : Form, ISupplierView
    {
        private readonly SupplierPresenter _presenter;

        public frmSupplier()
        {
            InitializeComponent();
            _presenter = CompositionRoot.CreateSupplierPresenter(this);
        }

        #region ISupplierView

        public int SelectedIndex => int.Parse(txtindex.Text);
        public int RowCount => dgdata.Rows.Count;
        public int SupplierId => int.Parse(txtid.Text);
        public string Document => txtdocument.Text;
        // Explicit interface implementation: Control.CompanyName and ContainerControl.Validate()
        // (below) are both inherited members this Form already has; qualifying them explicitly
        // avoids silently shadowing those instead of implementing the interface.
        string ISupplierView.CompanyName => txtcompanyname.Text;
        public string Email => txtemail.Text;
        public string Phone => txtphone.Text;

        List<string> ISupplierView.Validate()
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

        public bool ConfirmDelete()
        {
            return MessageBox.Show("¿Desea eliminar el proveedor?", "Mensaje", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        public void LoadSuppliers(IEnumerable<SupplierRow> suppliers)
        {
            foreach (SupplierRow row in suppliers)
            {
                AddRow(row);
            }
        }

        public void AddRow(SupplierRow row)
        {
            int rowId = dgdata.Rows.Add();
            WriteRow(dgdata.Rows[rowId], row);
        }

        public void ReplaceRow(int index, SupplierRow row)
        {
            WriteRow(dgdata.Rows[index], row);
        }

        public void RemoveRow(int index)
        {
            dgdata.Rows.RemoveAt(index);
        }

        public void ClearForm()
        {
            Clean();
        }

        public void ShowMessage(string message)
        {
            MessageBox.Show(message, "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        public void ShowValidationErrors(IReadOnlyList<string> errors)
        {
            MessageBox.Show(string.Join("\n", errors), "Errores de Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private static void WriteRow(DataGridViewRow gridRow, SupplierRow row)
        {
            gridRow.Cells["Id"].Value = row.Id.ToString();
            gridRow.Cells["NumeroDocumento"].Value = row.Document;
            gridRow.Cells["RazonSocial"].Value = row.CompanyName;
            gridRow.Cells["Correo"].Value = row.Email;
            gridRow.Cells["Telefono"].Value = row.Phone;
        }

        #endregion

        private Dictionary<TextBox, List<string>> campWithRules = new Dictionary<TextBox, List<string>>();
        private Dictionary<string, string> namesMessages = new Dictionary<string, string>
        {
            { "txtdocument", "RUC/Número Documento" },
            { "txtcompanyname", "Razón Social" },
            { "txtemail", "Correo" },
            { "txtphone", "Teléfono" },
        };
        private void InitializeValidators()
        {
            campWithRules = new Dictionary<TextBox, List<string>>
            {
                { txtdocument, new List<string>{ "NotEmpty", "ValidatorRUC/CI" } },
                { txtcompanyname, new List<string>{ "NotEmpty" } },
                { txtemail, new List<string>{ "NotEmpty", "ValidateEmail" } },
                { txtphone, new List<string>{ "NotEmpty", "OnlyNumbers" } },
            };
        }

        private void frmSupplier_Load(object sender, EventArgs e)
        {
            InitializeValidators();

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
            dgdata.Columns.Add("RazonSocial", "Razon Social");
            dgdata.Columns.Add("Correo", "Correo");
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

            bool canManage = MainForm.Session?.Can("proveedores.gestionar") ?? false;
            btnSave.Enabled = canManage;
            btnDelete.Enabled = canManage;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            _presenter.OnSave();
        }

        private void dgdata_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex < 0) return;

            dgdata.Cursor = dgdata.Columns[e.ColumnIndex].Name == "btnSeleccionar"
                            ? Cursors.Hand
                            : Cursors.Default;
        }

        private void dgdata_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 0)
                return;

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
            txtcompanyname.Text = "";
            txtemail.Text = "";
            txtphone.Text = "";
        }

        private void dgdata_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            int index = e.RowIndex;
            if (dgdata.Columns[e.ColumnIndex].Name != "btnSeleccionar" || index < 0) return;

            txtindex.Text = (index + 1).ToString();
            txtid.Text = dgdata.Rows[index].Cells["Id"].Value.ToString();
            txtdocument.Text = dgdata.Rows[index].Cells["NumeroDocumento"].Value.ToString();
            txtcompanyname.Text = dgdata.Rows[index].Cells["RazonSocial"].Value.ToString();
            txtemail.Text = dgdata.Rows[index].Cells["Correo"].Value.ToString();
            txtphone.Text = dgdata.Rows[index].Cells["Telefono"].Value.ToString();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            _presenter.OnDelete();
        }

        private void btnsearch_Click(object sender, EventArgs e)
        {
            if (dgdata.Rows.Count <= 0) return;

            string columnFilter = ((PharmacySystem.Model.ComboBoxItem)cbosearch.SelectedItem).Value.ToString();
            string value;

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
