using DocumentFormat.OpenXml.Wordprocessing;
using PharmacySystem.Helpers;
using PharmacySystem.Logical;
using PharmacySystem.Model;
using PharmacySystem.Validators;
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
    public partial class frmPurchase : Form
    {
        private static int _IdPerson = 0;
        public frmPurchase(int IdPerson = 0)
        {
            InitializeComponent();
            _IdPerson = IdPerson;
        }

        private Dictionary<TextBox, List<string>> campWithRules = new Dictionary<TextBox, List<string>>();
        private Dictionary<string, string> namesMessages = new Dictionary<string, string>
        {
            { "txtnumberdocument", "Número Documento" },
            { "txtdocumentsupplier", "RUC/ Documento Proveedor" },
            { "txtnamesupplier", "Razón Social Proveedor" },
            { "txtcodeproduct", "Código Producto" },
            { "txtnameproduct", "Nombre Producto" },
            { "txtamount", "Cantidad" },
            { "txtpricepurchase", "Precio Compra" },
            { "txtpricesale", "Precio Venta" },
        };
        private void InitializeValidators()
        {
            var txtAmountInternal = txtamount.Controls[1] as TextBox;
            txtAmountInternal.Name = "txtcantidad";
            campWithRules = new Dictionary<TextBox, List<string>>
            {
                { txtnumberdocument, new List<string>{ "NotEmpty", "ValidatorRUC/CI" } },
                { txtdocumentsupplier, new List<string>{ "NotEmpty", "ValidatorRUC/CI" } },
                { txtnamesupplier, new List<string>{ "NotEmpty" } },
                { txtcodeproduct, new List<string>{ "NotEmpty" } },
                { txtnameproduct, new List<string>{ "NotEmpty" } },
                { txtAmountInternal, new List<string>{ "NotEmpty" } },
                { txtpricepurchase, new List<string>{ "NotEmpty" } },
                { txtpricesale, new List<string>{ "NotEmpty" } },
            };
        }

        private void frmPurchase_Load(object sender, EventArgs e)
        {
            InitializeValidators();
  
            DataGridViewButtonColumn Button = new DataGridViewButtonColumn();

            Button.HeaderText = "Eliminar";
            Button.Width = 100;
            Button.Text = "";
            Button.Name = "btnEliminar";
            Button.UseColumnTextForButtonValue = true;

   
            dgdata.Columns.Add(Button);
            dgdata.Columns.Add("IdProducto", "IdProducto");
            dgdata.Columns.Add("Codigo", "Codigo");
            dgdata.Columns.Add("NombreProducto", "Nombre Producto");
            dgdata.Columns.Add("Cantidad", "Cantidad");
            dgdata.Columns.Add("FechaVencimiento", "FechaVencimiento");
            dgdata.Columns.Add("PrecioCompra", "Precio Compra");
            dgdata.Columns.Add("PrecioVenta", "Precio Venta");
            dgdata.Columns.Add("SubTotal", "SubTotal");

            dgdata.Columns["IdProducto"].Visible = false;
            dgdata.Columns["PrecioVenta"].Visible = false;



            //cbotipodocumento.Items.Add(new ComboBoxItem() { Value = "Boleta", Text = "Boleta" });
            cbotypedocument.Items.Add(new ComboBoxItem() { Value = "Factura", Text = "Factura" });
            cbotypedocument.DisplayMember = "Text";
            cbotypedocument.ValueMember = "Value";
            cbotypedocument.SelectedIndex = 0;
            LockDateBack();
        }

        public void LockDateBack()
        {
            DTPexpireddate.MinDate = DateTime.Today;
        }

        private void btnSearchSupplier_Click(object sender, EventArgs e)
        {
            using (var form = new ModalSupplier())
            {
                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    txtnamesupplier.Text = form.companyName;
                    txtdocumentsupplier.Text = form.document;
                    txtidsupplier.Text = form.idSupplier.ToString();
                }
            }
        }

        private void btnSearchProduct_Click(object sender, EventArgs e)
        {
            using (var form = new ModalProduct("frmPurchase"))
            {
                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    txtcodeproduct.Text = form.code;
                    txtnameproduct.Text = form.name;
                    txtidproduct.Text = form.idProduct.ToString();
                }
            }
        }

        private void txtPricePurchase_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Char.IsDigit(e.KeyChar))
            {
                e.Handled = false;
            }
            else {
                if (txtpricepurchase.Text.Trim().Length == 0 && e.KeyChar.ToString() == ".")
                {
                    e.Handled = true;
                }
                else {
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

        private void txtPriceSale_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Char.IsDigit(e.KeyChar))
            {
                e.Handled = false;
            }
            else
            {
                if (txtpricesale.Text.Trim().Length == 0 && e.KeyChar.ToString() == ".")
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

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateForm()) return;
            decimal pricePurchase = 0;
            decimal priceSale = 0;
            decimal subTotal;
            bool product_exists = false;

            if (int.Parse(txtidproduct.Text) == 0) {
                MessageBox.Show("Debe seleccionar un producto primero", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            bool errorPurchase = false;
            bool errorSale = false;
            try
            {
                pricePurchase = CultureInfoHelper.CultureInfoConverterStringToDecimal(txtpricepurchase.Text);
            }
            catch {
                errorPurchase = true;
            }

            try
            {
                Console.WriteLine(txtpricesale.Text);
                priceSale = CultureInfoHelper.CultureInfoConverterStringToDecimal(txtpricesale.Text);
            }
            catch
            {
                errorSale = true;
            }

            if (errorPurchase) {
                MessageBox.Show("Error al convertir el tipo de moneda - Precio Compra\nEjemplo Formato ##.##", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            if (errorSale)
            {
                MessageBox.Show("Error al convertir el tipo de moneda - Precio Venta\nEjemplo Formato ##.##", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }


            foreach (DataGridViewRow row in dgdata.Rows)
            {
                if (row.Cells["IdProducto"].Value.ToString() == txtidproduct.Text) {
                    product_exists = true;
                    break;
                }
            }
            
            if (!product_exists) {
                int rowId = dgdata.Rows.Add();
                DataGridViewRow row = dgdata.Rows[rowId];
                subTotal = Convert.ToDecimal(txtamount.Text.Trim()) * pricePurchase;

                row.Cells["IdProducto"].Value = txtidproduct.Text;
                row.Cells["Codigo"].Value = txtcodeproduct.Text.Trim();
                row.Cells["NombreProducto"].Value = txtnameproduct.Text.Trim();
                row.Cells["Cantidad"].Value = txtamount.Text.Trim();
                row.Cells["FechaVencimiento"].Value = DTPexpireddate.Value.ToShortDateString();
                row.Cells["PrecioCompra"].Value = CultureInfoHelper.FormatAsEcuadorCurrency(pricePurchase);
                row.Cells["PrecioVenta"].Value = CultureInfoHelper.FormatAsEcuadorCurrency(priceSale);
                row.Cells["SubTotal"].Value = CultureInfoHelper.FormatAsEcuadorCurrency(subTotal);

                CleanProduct();
                CalculateTotal();
            }

           

        }

        public void CleanProduct() {
            txtidproduct.Text = "0";
            txtcodeproduct.Text = "";
            txtnameproduct.Text = "";
            txtamount.Value = 1;
            txtpricepurchase.Text = "";
            txtpricesale.Text = "";
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

        private void dgdata_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgdata.Columns[e.ColumnIndex].Name == "btnEliminar")
            {
                int index = e.RowIndex;
                if (index >= 0)
                {
                    dgdata.Rows.RemoveAt(index);
                    CalculateTotal();
                }
            }
        }

        private void btnFinishPurchase_Click(object sender, EventArgs e)
        {
            if (txtnumberdocument.Text.Trim() == "")
            {
                MessageBox.Show("Debe ingresar el numero de documento\npara registrar una compra", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtnumberdocument.Focus();
                return;
            }

            if (int.Parse(txtidsupplier.Text.Trim()) == 0) {
                MessageBox.Show("Debe seleccionar un proveedor\npara registrar una compra", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (dgdata.Rows.Count < 1)
            {
                MessageBox.Show("Debe ingresar un producto como minimo\npara registrar una compra", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            var lblTotalAmount = CultureInfoHelper.CultureInfoConverterStringToDecimal(lbltotalamount.Text);

            Purchase oPurchase = new Purchase() {
                oPerson = new Person() { idPerson = _IdPerson },
                oSupplier = new Supplier() { idSupplier = int.Parse(txtidsupplier.Text.Trim()) },
                totalAmount = lblTotalAmount,
                documentType = ((ComboBoxItem)cbotypedocument.SelectedItem).Value.ToString(),
                documentNumber = txtnumberdocument.Text.Trim()
                
            };

            List<PurchaseDetail> olist = new List<PurchaseDetail>();
            if (dgdata.Rows.Count > 0)
            {
                foreach (DataGridViewRow row in dgdata.Rows)
                {
                    olist.Add(new PurchaseDetail() {
                        oProduct = new Product() { idProduct = int.Parse(row.Cells["IdProducto"].Value.ToString()) },
                        quantity = int.Parse(row.Cells["Cantidad"].Value.ToString()),
                        expirationDate = Convert.ToDateTime(row.Cells["FechaVencimiento"].Value),
                        purchasePrice = CultureInfoHelper.CultureInfoConverterStringToDecimal(row.Cells["PrecioCompra"].Value.ToString()),
                        salePrice = CultureInfoHelper.CultureInfoConverterStringToDecimal(row.Cells["PrecioVenta"].Value.ToString()),
                        total = CultureInfoHelper.CultureInfoConverterStringToDecimal(row.Cells["SubTotal"].Value.ToString())
                    });
                }
            }
            oPurchase.oPurchaseDetail = olist;


            if (PurchaseService.Instance.RegisterPurchase(oPurchase))
            {
                Clean();
                MessageBox.Show("La compra fue registrada", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else {
                MessageBox.Show("No se pudo registrar la compra", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

        }

        private void CalculateTotal() {

            decimal total = 0;
            if (dgdata.Rows.Count > 0)
            {
                foreach (DataGridViewRow row in dgdata.Rows)
                {
                    total +=  CultureInfoHelper.CultureInfoConverterStringToDecimal(row.Cells["SubTotal"].Value.ToString());
                }
            }

            lbltotalamount.Text = CultureInfoHelper.FormatAsEcuadorCurrency(total);

        }

        private void Clean() {
            CleanProduct();
            cbotypedocument.SelectedIndex = 0;
            txtnumberdocument.Text = "";
            txtdocumentsupplier.Text = "";
            txtidsupplier.Text = "0";
            txtnamesupplier.Text = "";
            dgdata.Rows.Clear();
            lbltotalamount.Text = "0";
        }

        private void txtCodeProduct_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                Product pr = ProductService.Instance.ListProduct().Where(p => p.code == txtcodeproduct.Text.Trim()).FirstOrDefault();
                if (pr != null)
                {
                    txtcodeproduct.Text = pr.code;
                    txtnameproduct.Text = pr.name;
                    txtidproduct.Text = pr.idProduct.ToString();
                }

            }
        }

        private bool ValidateForm()
        {
            var errors = new List<string>();

            foreach (var camp in campWithRules)
            {
                foreach (var rulePassword in camp.Value)
                {
                    var rule = Validations.rules[rulePassword];
                    if (!rule.Validate(camp.Key.Text))
                    {
                        errors.Add($"{namesMessages[camp.Key.Name]} : {rule.MessageError}");
                    }
                }

            }
            if (errors.Count > 0)
            {
                MessageBox.Show(string.Join("\n", errors), "Errores de Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }
    }
}
