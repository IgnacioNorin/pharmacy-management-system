using PharmacySystem.Helpers;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PharmacySystem
{
    public partial class frmSale : Form, ISaleView
    {
        private readonly SalePresenter _presenter;

        public frmSale(int idperson = 0)
        {
            InitializeComponent();
            _presenter = CompositionRoot.CreateSalePresenter(this, idperson);
        }

        private void frmSale_Load(object sender, EventArgs e)
        {
            cbodocumenttype.Items.Add(new ComboBoxItem() { Value = "Factura", Text = "Factura" });
            cbodocumenttype.DisplayMember = "Text";
            cbodocumenttype.ValueMember = "Value";
            cbodocumenttype.SelectedIndex = 0;
            txtstock.Visible = false;

            DataGridViewButtonColumn Button = new DataGridViewButtonColumn();

            Button.HeaderText = "Eliminar";
            Button.Width = 100;
            Button.Text = "";
            Button.Name = "btnEliminar";
            Button.UseColumnTextForButtonValue = true;

            dgdata.Columns.Add(Button);
            dgdata.Columns.Add("IdProducto", "IdProducto");
            dgdata.Columns.Add("NombreProducto", "Nombre Producto");
            dgdata.Columns.Add("Cantidad", "Cantidad");
            dgdata.Columns.Add("PrecioVenta", "Precio Venta");
            dgdata.Columns.Add("SubTotal", "SubTotal");

            dgdata.Columns["IdProducto"].Visible = false;
            dgdata.Columns["PrecioVenta"].DefaultCellStyle.FormatProvider = CultureInfoHelper.CustomCultureInfo();
            dgdata.Columns["SubTotal"].DefaultCellStyle.FormatProvider = CultureInfoHelper.CustomCultureInfo();
        }

        private void btnSearchClient_Click(object sender, EventArgs e)
        {
            using (var form = new ModalPerson())
            {
                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    txtdocumentclient.Text = form.document;
                    txtnameclient.Text = form.name;
                    txtidclient.Text = form.idClient;
                }
            }
        }

        private void btnSearchProduct_Click(object sender, EventArgs e)
        {
            using (var form = new ModalProduct("frmSale"))
            {
                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    txtcodeproduct.Text = form.code;
                    txtstock.Text = form.stock;
                    txtnameproduct.Text = form.name;
                    txtidproduct.Text = form.idProduct.ToString();
                    txtpricesale.Text = CultureInfoHelper.FormatAsCurrency(Convert.ToDecimal(form.priceSale.ToLower()));

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

                e.Graphics.DrawImage(Properties.Resources.delete32, new Rectangle(x, y, w, h));
                e.Handled = true;
            }
        }

        private void dgdata_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex >= 0)
            {
                string colname = dgdata.Columns[e.ColumnIndex].Name;
                if (colname != "btnEliminar")
                {
                    dgdata.Cursor = Cursors.Default;
                }
                else
                {
                    dgdata.Cursor = Cursors.Hand;
                }
            }
        }

        private void dgdata_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgdata.Columns[e.ColumnIndex].Name == "btnEliminar")
            {
                int index = e.RowIndex;
                if (index >= 0)
                {
                    _presenter.OnRemoveProduct(index);
                }
            }
        }

        private void btnAddProduct_Click(object sender, EventArgs e) => _presenter.OnAddProduct();

        public void CleanProduct()
        {
            txtidproduct.Text = "0";
            txtstock.Text = "0";
            txtcodeproduct.Text = "";
            txtnameproduct.Text = "";
            txtamount.Value = 1;
            txtpricesale.Text = "";
        }

        private void btnFinishSale_Click(object sender, EventArgs e) => _presenter.OnFinishSale();

        private void Clean()
        {
            txtdocumentclient.Text = "";
            txtnameclient.Text = "";
            txttotalpay.Text = "0";
            txtpaywith.Text = "0";
            txtchange.Text = "0";
            dgdata.Rows.Clear();
        }

        private void txtPayWith_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Char.IsDigit(e.KeyChar))
            {
                e.Handled = false;
            }
            else
            {
                if (txtpaywith.Text.Trim().Length == 0 && e.KeyChar.ToString() == ".")
                {
                    e.Handled = true;
                }
                else
                {
                    if (Char.IsControl(e.KeyChar) || e.KeyChar.ToString() == ".")
                    {
                        e.Handled = false;
                    }
                    else
                    {
                        e.Handled = true;
                    }
                }

            }

        }

        private void btnCalculate_Click(object sender, EventArgs e) => _presenter.OnCalculateChangeRequested();

        private void txtCodeProduct_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                _presenter.OnProductCodeEntered(txtcodeproduct.Text.Trim());
            }
        }

        #region ISaleView

        int ISaleView.SelectedProductId => int.Parse(txtidproduct.Text);
        string ISaleView.SelectedProductName => txtnameproduct.Text.Trim();
        int ISaleView.Stock => int.Parse(txtstock.Text);
        decimal ISaleView.Amount => txtamount.Value;
        string ISaleView.PriceSaleText => txtpricesale.Text;

        string ISaleView.DocumentClient => txtdocumentclient.Text;
        string ISaleView.NameClient => txtnameclient.Text;
        string ISaleView.PayWithText => txtpaywith.Text;
        string ISaleView.TotalPayText => txttotalpay.Text;
        string ISaleView.ChangeText => txtchange.Text;
        string ISaleView.DocumentType => ((ComboBoxItem)cbodocumenttype.SelectedItem).Value.ToString();

        public void ShowMessage(string message) =>
            MessageBox.Show(message, "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

        public void SetSelectedProduct(int id, string code, string name, int stock, string priceSaleFormatted)
        {
            txtcodeproduct.Text = code;
            txtstock.Text = stock.ToString();
            txtnameproduct.Text = name;
            txtidproduct.Text = id.ToString();
            txtpricesale.Text = priceSaleFormatted;
        }

        public void AddCartLine(SaleCartLine line)
        {
            int rowId = dgdata.Rows.Add();
            DataGridViewRow row = dgdata.Rows[rowId];

            row.Cells["IdProducto"].Value = line.ProductId.ToString();
            row.Cells["NombreProducto"].Value = line.Name;
            row.Cells["Cantidad"].Value = line.Quantity.ToString();
            row.Cells["PrecioVenta"].Value = CultureInfoHelper.FormatAsCurrency(line.SalePrice);
            row.Cells["SubTotal"].Value = CultureInfoHelper.FormatAsCurrency(line.SubTotal);
        }

        public void RemoveCartLineAt(int index) => dgdata.Rows.RemoveAt(index);

        public void SetTotalText(string formattedTotal) => txttotalpay.Text = formattedTotal;

        public void SetChangeText(string formattedChange) => txtchange.Text = formattedChange;

        public void ClearProductEntry() => CleanProduct();

        public void ClearSale() => Clean();

        public void SaleRegistered(int idSale)
        {
            if (MessageBox.Show("La venta fue registrada\n¿Desea imprimir el ticket ahora?", "Mensaje", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                PrintSale imp = new PrintSale(idSale);
                imp.ShowDialog();
            }
        }

        #endregion
    }
}
