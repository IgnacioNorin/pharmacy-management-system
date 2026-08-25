using PharmacySystem.Helpers;
using PharmacySystem.Logical;
using PharmacySystem.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PharmacySystem
{
    public partial class frmSale : Form
    {
        private static int _IdPerson;
        public frmSale(int idperson = 0)
        {
            InitializeComponent();
            _IdPerson = idperson;
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
                    //int idProduct = int.Parse(dgdata.Rows[index].Cells["IdProducto"].Value.ToString());
                    //int stock = int.Parse(dgdata.Rows[index].Cells["Cantidad"].Value.ToString());
                    //bool result = SaleService.Instance.ControlStock(idProduct, stock, false);

                    //if (result) {
                    dgdata.Rows.RemoveAt(index);
                    CalculateTotal();
                    //}
                }
            }
        }

        private void btnAddProduct_Click(object sender, EventArgs e)
        {
            decimal priceSale = 0;
            decimal subTotal;
            bool productExists = false;




            if (int.Parse(txtidproduct.Text) == 0)
            {
                MessageBox.Show("Debe seleccionar un producto primero", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (int.Parse(txtstock.Text) < int.Parse(txtamount.Text))
            {
                MessageBox.Show("No hay suficiente stock del producto", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            bool errorSale = false;
            try
            {
                priceSale = CultureInfoHelper.CultureInfoConverterStringToDecimal(txtpricesale.Text);
            }
            catch
            {
                errorSale = true;
            }

            if (errorSale)
            {
                MessageBox.Show("Error al convertir el tipo de moneda - Precio Venta\nEjemplo Formato ##.##", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            foreach (DataGridViewRow row in dgdata.Rows)
            {
                if (row.Cells["IdProducto"].Value.ToString() == txtidproduct.Text)
                {
                    productExists = true;
                    break;
                }
            }

            if (!productExists) {

                
                int rowId = dgdata.Rows.Add();
                DataGridViewRow row = dgdata.Rows[rowId];
                subTotal = Convert.ToDecimal(txtamount.Text.Trim()) * priceSale;

                row.Cells["IdProducto"].Value = txtidproduct.Text;
                row.Cells["NombreProducto"].Value = txtnameproduct.Text.Trim();
                row.Cells["Cantidad"].Value = txtamount.Text.Trim();
                row.Cells["PrecioVenta"].Value = CultureInfoHelper.FormatAsCurrency(priceSale);
                row.Cells["SubTotal"].Value = CultureInfoHelper.FormatAsCurrency(subTotal);
                CalculateTotal();
                CleanProduct();


            }
            else
            {
                MessageBox.Show("El producto ya fue agregado\nElimínelo e ingrese el nuevo si quiere cambiar la cantidad.", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            
        }

        public void CleanProduct()
        {
            txtidproduct.Text = "0";
            txtstock.Text = "0";
            txtcodeproduct.Text = "";
            txtnameproduct.Text = "";
            txtamount.Value = 1;
            txtpricesale.Text = "";
        }

        private void CalculateTotal()
        {

            decimal total = 0;
            if (dgdata.Rows.Count > 0)
            {
                foreach (DataGridViewRow row in dgdata.Rows)
                {
                    total += CultureInfoHelper.CultureInfoConverterStringToDecimal(row.Cells["SubTotal"].Value.ToString());
                }
            }

            txttotalpay.Text = CultureInfoHelper.FormatAsCurrency(total);

        }

        private void btnFinishSale_Click(object sender, EventArgs e)
        {

            if (txtdocumentclient.Text.Trim() == "" || txtnameclient.Text.Trim() == "")
            {
                MessageBox.Show("Debe ingresar todos los datos del cliente", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (dgdata.Rows.Count < 1)
            {
                MessageBox.Show("Debe ingresar un producto como minimo\npara registrar una venta", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (txtpaywith.Text.Trim() == "0")
            {
                MessageBox.Show("Debe ingresar con cuanto paga el cliente", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (!CalculateChange())
            {
                MessageBox.Show("Error al convertir el tipo de moneda - Paga con\nEjemplo Formato ##.##", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            var moneyToPay = CultureInfoHelper.CultureInfoConverterStringToDecimal(txtpaywith.Text);
            var totalToPay = CultureInfoHelper.CultureInfoConverterStringToDecimal(txttotalpay.Text);
            var changeMoney = CultureInfoHelper.CultureInfoConverterStringToDecimal(txtchange.Text);

            if (totalToPay > moneyToPay)
            {
                MessageBox.Show("Falta dinero para pagar", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            bool result = SaleService.Instance.ControlStock(int.Parse(txtidproduct.Text), int.Parse(txtamount.Text.Trim()), true);
            bool existsProduct = false;
            bool subtractStock = false;
            if (result)
            {
                List<SaleDetail> olist = new List<SaleDetail>();
                

                if (dgdata.Rows.Count > 0)
                {
                    foreach (DataGridViewRow row in dgdata.Rows)
                    {
                        int idProduct = int.Parse(row.Cells["IdProducto"].Value.ToString());
                        int amount = int.Parse(row.Cells["Cantidad"].Value.ToString());
                        decimal salePrice = CultureInfoHelper.CultureInfoConverterStringToDecimal(row.Cells["PrecioVenta"].Value.ToString());
                        decimal subtotal = CultureInfoHelper.CultureInfoConverterStringToDecimal(row.Cells["SubTotal"].Value.ToString());
                        existsProduct = ProductService.Instance.VerifyProduct(idProduct);

                        if (existsProduct)
                        {
                            olist.Add(new SaleDetail()
                            {
                                oProduct = new Product() { idProduct = idProduct},
                                amount = amount,
                                salePrice = salePrice,
                                subtotal = subtotal
                            });
                            subtractStock = SaleService.Instance.ControlStock(idProduct, amount, true);
                            if (!subtractStock)
                            {
                                olist.Clear();
                                MessageBox.Show("No se pudo registrar la venta\n Problema con Stock", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                return;
                            }

                        }
                        else
                        {
                            MessageBox.Show("No se pudo registrar la venta\n Problema con producto", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            return;
                        }

                    }
                }

                Sale oSale = new Sale()
                {
                    typeDocument = ((ComboBoxItem)cbodocumenttype.SelectedItem).Value.ToString(),
                    oPerson = new Person() { idPerson = _IdPerson },
                    documentClient = txtdocumentclient.Text.Trim(),
                    nameClient = txtnameclient.Text.Trim(),
                    totalPay = totalToPay,
                    payWith = moneyToPay,
                    change = changeMoney
                };

                oSale.oSaleDetail = olist;

                int idsale = SaleService.Instance.RegisterSale(oSale);

                //bool resultSt = SaleService.Instance.ControlStock(idProduct, stock, false);
                if (idsale != 0)
                {
                    Clean();
                    if (MessageBox.Show("La venta fue registrada\n¿Desea imprimir el ticket ahora?", "Mensaje", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        PrintSale imp = new PrintSale(idsale);
                        imp.ShowDialog();
                    }
                }
                else
                {
                    MessageBox.Show("No se pudo registrar la venta", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

            }



        }
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

        

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            if (!CalculateChange()) {
                MessageBox.Show("Error al convertir el tipo de moneda - Paga con\nEjemplo Formato ##.##", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }


        private bool CalculateChange() {

            bool result = true;
            decimal moneyToPay = Convert.ToDecimal(txtpaywith.Text);
            decimal totalPay = CultureInfoHelper.CultureInfoConverterStringToDecimal(txttotalpay.Text);

            if (moneyToPay < totalPay)
            {
                txtchange.Text = CultureInfoHelper.FormatAsCurrency(0);
            }
            else
            {
                decimal change = moneyToPay - totalPay;
                txtchange.Text = CultureInfoHelper.FormatAsCurrency(change);
            }

            return result;
        }

        private void txtCodeProduct_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {

                Product pr = ProductService.Instance.ListProduct().Where(p => p.code == txtcodeproduct.Text.Trim()).FirstOrDefault();
                if (pr != null) {
                    txtcodeproduct.Text = pr.code;
                    txtstock.Text = pr.stock.ToString();
                    txtnameproduct.Text = pr.name;
                    txtidproduct.Text = pr.idProduct.ToString();
                    txtpricesale.Text = CultureInfoHelper.FormatAsCurrency(pr.salePrice);
                }
                
            }
        }
    }
}
