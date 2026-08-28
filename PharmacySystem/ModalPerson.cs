using PharmacySystem.Presentation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PharmacySystem
{
    public partial class ModalPerson : Form, IClientPickerView
    {
        public string idClient { get; set; }
        public string document { get; set; }
        public string name { get; set; }
        // The full row of the picked client, for the caller that needs the fiscal profile too.
        public ClientRow SelectedClient { get; private set; }

        private readonly List<ClientRow> _rows = new List<ClientRow>();
        private readonly ClientPickerPresenter _presenter;

        public ModalPerson()
        {
            InitializeComponent();
            _presenter = CompositionRoot.CreateClientPickerPresenter(this);
        }

        public void LoadClients(IEnumerable<ClientRow> clients)
        {
            foreach (ClientRow row in clients)
            {
                _rows.Add(row);
                int rowId = dgdata.Rows.Add();
                DataGridViewRow gridRow = dgdata.Rows[rowId];
                gridRow.Cells["Id"].Value = row.Id.ToString();
                gridRow.Cells["NumeroDocumento"].Value = row.Document;
                gridRow.Cells["NombreCompleto"].Value = row.Name;
                gridRow.Cells["RazonSocial"].Value = row.BusinessName;
                gridRow.Cells["EsEmpresa"].Value = row.IsCompany ? "Sí" : "No";
                gridRow.Cells["Direccion"].Value = row.Address;
                gridRow.Cells["Telefono"].Value = row.Phone;
            }
        }

        private void ModalPersona_Load(object sender, EventArgs e)
        {
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
            dgdata.Columns.Add("RazonSocial", "Razón Social");
            dgdata.Columns.Add("EsEmpresa", "Empresa");
            dgdata.Columns.Add("Direccion", "Dirección");
            dgdata.Columns.Add("Telefono", "Telefono");

            dgdata.Columns["btnSeleccionar"].Width = 100;
            dgdata.Columns["NumeroDocumento"].Width = 150;
            dgdata.Columns["NombreCompleto"].Width = 220;
            dgdata.Columns["RazonSocial"].Width = 200;
            dgdata.Columns["EsEmpresa"].Width = 70;
            dgdata.Columns["Direccion"].Width = 240;
            dgdata.Columns["Id"].Visible = false;

            foreach (DataGridViewColumn cl in dgdata.Columns)
            {
                if (cl.Visible == true && cl.Name != "btnSeleccionar")
                {
                    cboSearch.Items.Add(new PharmacySystem.Model.ComboBoxItem() { Value = cl.Name, Text = cl.HeaderText });
                }
            }
            cboSearch.DisplayMember = "Text";
            cboSearch.ValueMember = "Value";
            cboSearch.SelectedIndex = 0;

            _presenter.OnLoad();
        }

        private void dgdata_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex >= 0)
            {
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
            if (e.RowIndex < 0)
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

        private void dgdata_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgdata.Columns[e.ColumnIndex].Name == "btnSeleccionar")
            {
                int index = e.RowIndex;
                if (index >= 0)
                {
                    SelectedClient = index < _rows.Count ? _rows[index] : null;
                    idClient = dgdata.Rows[index].Cells["Id"].Value.ToString();
                    document = dgdata.Rows[index].Cells["NumeroDocumento"].Value.ToString();
                    name = dgdata.Rows[index].Cells["NombreCompleto"].Value.ToString();
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
        }

        private void btnsearch_Click(object sender, EventArgs e)
        {
            string columnFilter = ((PharmacySystem.Model.ComboBoxItem)cboSearch.SelectedItem).Value.ToString();

            if (dgdata.Rows.Count > 0)
            {
                foreach (DataGridViewRow row in dgdata.Rows)
                {
                    string cellValue = (row.Cells[columnFilter].Value?.ToString() ?? "").Trim();
                    row.Visible = cellValue.Contains(txtSearch.Text.Trim());
                }
            }
        }

        private void btnclear_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            foreach (DataGridViewRow row in dgdata.Rows)
            {
                row.Visible = true;
            }
        }
    }
}
