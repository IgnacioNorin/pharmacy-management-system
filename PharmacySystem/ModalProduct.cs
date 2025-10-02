using PharmacySystem.Logical;
using PharmacySystem.Model;
using PharmacySystem.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PharmacySystem
{
    public partial class ModalProduct : Form
    {

        public int idProduct { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string priceSale { get; set; }
        public string stock { get; set; }

        private string formOrigin;
        public ModalProduct(string origin)
        {
            InitializeComponent();
            formOrigin = origin;
        }
        private void ModalProducto_Load(object sender, EventArgs e)
        {

            //AGREGAR BOTON ELIMINAR
            DataGridViewButtonColumn Button = new DataGridViewButtonColumn();
            Button.HeaderText = "Seleccionar";
            Button.Width = 80;
            Button.Text = "";
            Button.Name = "btnSeleccionar";
            Button.UseColumnTextForButtonValue = true;

            //AGREGAMOS LOS BOTONES
            dgdataproduct.Columns.Add(Button);
            dgdataproduct.Columns.Add("Id", "Id");
            dgdataproduct.Columns.Add("Codigo", "Código");
            dgdataproduct.Columns.Add("Nombre", "Nombre");
            dgdataproduct.Columns.Add("Descripcion", "Descripción");
            dgdataproduct.Columns.Add("Categoria", "Categoria");
            dgdataproduct.Columns.Add("Stock", "Stock");
            dgdataproduct.Columns.Add("PrecioVenta", "PrecioVenta");

            dgdataproduct.Columns["btnSeleccionar"].Width = 90;
            dgdataproduct.Columns["Codigo"].Width = 100;
            dgdataproduct.Columns["Nombre"].Width = 200;
            dgdataproduct.Columns["Descripcion"].Width = 210;
            dgdataproduct.Columns["Categoria"].Width = 150;

            dgdataproduct.Columns["Id"].Visible = false;
            dgdataproduct.Columns["PrecioVenta"].Visible = false;

            foreach (DataGridViewColumn cl in dgdataproduct.Columns)
            {
                if (cl.Visible == true && cl.Name != "btnSeleccionar")
                {
                    cbosearchproduct.Items.Add(new ComboBoxItem() { Value = cl.Name, Text = cl.HeaderText });
                }
            }
            cbosearchproduct.DisplayMember = "Text";
            cbosearchproduct.ValueMember = "Value";
            cbosearchproduct.SelectedIndex = 0;

            fillProductList();
        }
        
        private void fillProductList()
        {
            switch (formOrigin)
            {
                case "frmPurchase":
                    foreach (Product p in ProductService.Instance.ListProduct())
                    {

                        if (p != null)
                        {

                            int rowId = dgdataproduct.Rows.Add();
                            DataGridViewRow row = dgdataproduct.Rows[rowId];
                            row.Cells["Id"].Value = p.idProduct.ToString();
                            row.Cells["Codigo"].Value = p.code;
                            row.Cells["Nombre"].Value = p.name;
                            row.Cells["Descripcion"].Value = p.description;
                            row.Cells["Categoria"].Value = p.oCategory.description;
                            row.Cells["Stock"].Value = p.stock;
                            row.Cells["PrecioVenta"].Value = p.salePrice;
                        }

                    }
                    break;
                case "frmSale":
                    foreach (Product p in ProductService.Instance.ListProduct())
                    {

                        if (p != null && p.expirationDate != null && p.stock > 0)
                        {

                            int rowId = dgdataproduct.Rows.Add();
                            DataGridViewRow row = dgdataproduct.Rows[rowId];
                            row.Cells["Id"].Value = p.idProduct.ToString();
                            row.Cells["Codigo"].Value = p.code;
                            row.Cells["Nombre"].Value = p.name;
                            row.Cells["Descripcion"].Value = p.description;
                            row.Cells["Categoria"].Value = p.oCategory.description;
                            row.Cells["Stock"].Value = p.stock;
                            row.Cells["PrecioVenta"].Value = p.salePrice;
                        }

                    }

                    break;
            }
        }

        private void dgdataproduct_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex >= 0)
            {
                string colname = dgdataproduct.Columns[e.ColumnIndex].Name;
                if (colname != "btnSeleccionar")
                {
                    dgdataproduct.Cursor = Cursors.Default;
                }
                else
                {
                    dgdataproduct.Cursor = Cursors.Hand;
                }
            }
        }

        private void dgdataproduct_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
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

        private void dgdataproduct_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgdataproduct.Columns[e.ColumnIndex].Name == "btnSeleccionar")
            {
                int index = e.RowIndex;
                if (index >= 0)
                {
                    idProduct = int.Parse(dgdataproduct.Rows[index].Cells["Id"].Value.ToString());
                    code = dgdataproduct.Rows[index].Cells["Codigo"].Value.ToString();
                    name = dgdataproduct.Rows[index].Cells["Nombre"].Value.ToString();
                    priceSale = dgdataproduct.Rows[index].Cells["PrecioVenta"].Value.ToString();
                    stock = dgdataproduct.Rows[index].Cells["Stock"].Value.ToString();
                    
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
        }

        private void btnsearch_Click(object sender, EventArgs e)
        {
            string columnFilter = ((ComboBoxItem)cbosearchproduct.SelectedItem).Value.ToString();

            if (dgdataproduct.Rows.Count > 0)
            {
                foreach (DataGridViewRow row in dgdataproduct.Rows)
                {
                    string value = row.Cells[columnFilter].Value.ToString().Trim();

                    if (value.Contains(txtseachproduct.Text.Trim()))
                        row.Visible = true;
                    else
                        row.Visible = false;
                }
            }

        }

        private void btnclear_Click(object sender, EventArgs e)
        {
            txtseachproduct.Text = "";
            foreach (DataGridViewRow row in dgdataproduct.Rows)
            {
                row.Visible = true;
            }
        }
    }
}
