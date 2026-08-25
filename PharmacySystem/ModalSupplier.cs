using PharmacySystem.Presentation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PharmacySystem
{
    public partial class ModalSupplier : Form, ISupplierPickerView
    {
        public int idSupplier { get; set; }
        public string document { get; set; }
        public string companyName { get; set; }

        private readonly SupplierPickerPresenter _presenter;

        public ModalSupplier()
        {
            InitializeComponent();
            _presenter = CompositionRoot.CreateSupplierPickerPresenter(this);
        }

        public void LoadSuppliers(IEnumerable<SupplierRow> suppliers)
        {
            foreach (SupplierRow row in suppliers)
            {
                int rowId = dgdata.Rows.Add();
                DataGridViewRow gridRow = dgdata.Rows[rowId];
                gridRow.Cells["Id"].Value = row.Id.ToString();
                gridRow.Cells["NumeroDocumento"].Value = row.Document;
                gridRow.Cells["RazonSocial"].Value = row.CompanyName;
                gridRow.Cells["Correo"].Value = row.Email;
                gridRow.Cells["Telefono"].Value = row.Phone;
            }
        }

        private void ModalSupplier_Load(object sender, EventArgs e)
        {
            //AGREGAR BOTON ELIMINAR
            DataGridViewButtonColumn Button = new DataGridViewButtonColumn();

            Button.HeaderText = "Seleccionar";
            Button.Width = 80;
            Button.Text = "";
            Button.Name = "btnSeleccionar";
            Button.UseColumnTextForButtonValue = true;

            //AGREGAMOS LOS BOTONES
            dgdata.Columns.Add(Button);
            dgdata.Columns.Add("Id", "Id");
            dgdata.Columns.Add("NumeroDocumento", "Numero Documento");
            dgdata.Columns.Add("RazonSocial", "Razon Social");
            dgdata.Columns.Add("Correo", "Correo");
            dgdata.Columns.Add("Telefono", "Telefono");

            dgdata.Columns["btnSeleccionar"].Width = 80;
            dgdata.Columns["NumeroDocumento"].Width = 130;
            dgdata.Columns["RazonSocial"].Width = 160;
            dgdata.Columns["Correo"].Width = 150;
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
                    idSupplier = Convert.ToInt32( dgdata.Rows[index].Cells["Id"].Value.ToString());
                    document = dgdata.Rows[index].Cells["NumeroDocumento"].Value.ToString();
                    companyName = dgdata.Rows[index].Cells["RazonSocial"].Value.ToString();

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
                    string value = row.Cells[columnFilter].Value.ToString().Trim();

                    if (row.Cells[columnFilter].Value.ToString().Trim().Contains(txtSearch.Text.Trim()))
                        row.Visible = true;
                    else
                        row.Visible = false;
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
