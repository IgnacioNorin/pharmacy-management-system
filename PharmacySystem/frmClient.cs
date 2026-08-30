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
            BuildClientPager();
            _presenter = CompositionRoot.CreateClientPresenter(this);
        }

        #region IClientView

        public int SelectedIndex => int.Parse(txtindex.Text);
        public int PersonId => int.Parse(txtid.Text);
        public string Document => txtdocument.Text;
        string IClientView.Name => txtname.Text;
        public string Address => txtaddress.Text;
        public string Phone => txtphone.Text;
        public string BusinessName => txtbusinessname.Text;
        public string Activity => txtactivity.Text;
        public string Commune => txtcommune.Text;
        public string Email => txtemail.Text;
        public bool IsCompany => chkiscompany.Checked;

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
            dgdata.Rows.Clear();
            foreach (ClientRow row in clients)
            {
                int rowId = dgdata.Rows.Add();
                WriteRow(dgdata.Rows[rowId], row);
            }
        }

        public void SetPageInfo(int currentPage, int totalPages, int totalCount)
        {
            lblClientPage.Text = totalCount == 0
                ? "Sin resultados"
                : $"Página {currentPage} de {totalPages}  ·  {totalCount} cliente(s)";

            btnClientFirst.Enabled = btnClientPrev.Enabled = currentPage > 1;
            btnClientNext.Enabled = btnClientLast.Enabled = currentPage < totalPages;
        }

        public string SearchText => txtsearch.Text;

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
            gridRow.Cells["RazonSocial"].Value = row.BusinessName;
            gridRow.Cells["Giro"].Value = row.Activity;
            gridRow.Cells["Comuna"].Value = row.Commune;
            gridRow.Cells["Email"].Value = row.Email;
            gridRow.Cells["EsEmpresa"].Value = row.IsCompany ? "Sí" : "No";
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
                { txtdocument, new List<string>{ "NotEmpty", "ValidateDocument" } },
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
            dgdata.Columns.Add("RazonSocial", "Razón Social");
            dgdata.Columns.Add("Giro", "Giro");
            dgdata.Columns.Add("Comuna", "Comuna");
            dgdata.Columns.Add("Email", "Email");
            dgdata.Columns.Add("EsEmpresa", "Empresa");

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
            txtbusinessname.Text = "";
            txtactivity.Text = "";
            txtcommune.Text = "";
            txtemail.Text = "";
            chkiscompany.Checked = false;
        }

        // Fiscal columns can be null (a boleta-only client), so unlike the base fields these
        // are read null-safe.
        private string CellText(int rowIndex, string column)
        {
            object value = dgdata.Rows[rowIndex].Cells[column].Value;
            return value?.ToString() ?? "";
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
                    txtbusinessname.Text = CellText(index, "RazonSocial");
                    txtactivity.Text = CellText(index, "Giro");
                    txtcommune.Text = CellText(index, "Comuna");
                    txtemail.Text = CellText(index, "Email");
                    chkiscompany.Checked = CellText(index, "EsEmpresa") == "Sí";
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            _presenter.OnDelete();
        }

        private void btnsearch_Click(object sender, EventArgs e) => _presenter.OnSearch();

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtsearch.Text = "";
            _presenter.OnSearch();
        }

        // The Clientes grid is server-paged (ClientPresenter). Built in code to leave the
        // Designer untouched; the search box now runs a server-side query. The "buscar por"
        // column selector no longer applies - the term matches name, document, business name
        // and email at once.
        private Button btnClientFirst;
        private Button btnClientPrev;
        private Button btnClientNext;
        private Button btnClientLast;
        private Label lblClientPage;

        private void BuildClientPager()
        {
            int top = dgdata.Bottom + 8;
            int left = dgdata.Left;

            btnClientFirst = MakePagerButton("|<", left, top);
            btnClientPrev = MakePagerButton("<", left + 44, top);
            btnClientNext = MakePagerButton(">", left + 88, top);
            btnClientLast = MakePagerButton(">|", left + 132, top);

            lblClientPage = new Label
            {
                AutoSize = true,
                Location = new Point(left + 188, top + 6),
                Text = string.Empty
            };

            btnClientFirst.Click += (s, e) => _presenter.OnFirstPage();
            btnClientPrev.Click += (s, e) => _presenter.OnPreviousPage();
            btnClientNext.Click += (s, e) => _presenter.OnNextPage();
            btnClientLast.Click += (s, e) => _presenter.OnLastPage();

            Control host = dgdata.Parent ?? this;
            host.Controls.Add(btnClientFirst);
            host.Controls.Add(btnClientPrev);
            host.Controls.Add(btnClientNext);
            host.Controls.Add(btnClientLast);
            host.Controls.Add(lblClientPage);
        }

        private static Button MakePagerButton(string text, int x, int y) => new Button
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(40, 25),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.WhiteSmoke,
            Cursor = Cursors.Hand
        };
    }
}
