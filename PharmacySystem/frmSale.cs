using PharmacySystem.Helpers;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using PharmacySystem.Wpf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PharmacySystem
{
    public partial class frmSale : Form, ISaleView
    {
        private readonly SalePresenter _presenter;

        // Built in code so the large frmSale Designer stays untouched. Sits between the "Cambio"
        // field and the "Terminar Venta" button.
        private System.Windows.Forms.ComboBox cbopaymentmethod;
        private System.Windows.Forms.Button btnMixedPayment;
        private System.Windows.Forms.Label lblMixedPayment;

        public frmSale(int idperson = 0)
        {
            InitializeComponent();
            BuildPaymentMethodCombo();
            _presenter = CompositionRoot.CreateSalePresenter(this, idperson);
        }

        private void BuildPaymentMethodCombo()
        {
            var lbl = new System.Windows.Forms.Label
            {
                Text = "Forma de pago:",
                Location = new System.Drawing.Point(795, 496),
                AutoSize = true
            };
            cbopaymentmethod = new System.Windows.Forms.ComboBox
            {
                Location = new System.Drawing.Point(795, 513),
                Size = new System.Drawing.Size(150, 21),
                DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList,
                DisplayMember = "Text",
                ValueMember = "Value"
            };

            btnMixedPayment = new System.Windows.Forms.Button
            {
                Text = "Pago mixto…",
                Location = new System.Drawing.Point(795, 538),
                Size = new System.Drawing.Size(150, 25),
                FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                BackColor = System.Drawing.Color.WhiteSmoke,
                Cursor = System.Windows.Forms.Cursors.Hand
            };
            btnMixedPayment.Click += (s, e) => _presenter.OnSplitPaymentRequested();

            lblMixedPayment = new System.Windows.Forms.Label
            {
                Location = new System.Drawing.Point(795, 566),
                Size = new System.Drawing.Size(220, 32),
                ForeColor = System.Drawing.Color.FromArgb(11, 37, 69),
                Visible = false
            };

            Controls.Add(lbl);
            Controls.Add(cbopaymentmethod);
            Controls.Add(btnMixedPayment);
            Controls.Add(lblMixedPayment);
        }

        private void frmSale_Load(object sender, EventArgs e)
        {
            cbodocumenttype.DisplayMember = "Text";
            cbodocumenttype.ValueMember = "Value";
            _presenter.OnLoad();
            btnCreditNote.Enabled = MainForm.Session?.Can("ventas.nota_credito") ?? false;
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
                if (form.ShowDialog() == DialogResult.OK)
                {
                    _presenter.OnClientSelected(form.SelectedClient);
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

        private void btnCreditNote_Click(object sender, EventArgs e)
        {
            // Ported to WPF (step 4). Same ICreditNoteView; CreditNotePresenter unchanged.
            CreditNoteDialog.Show(Handle, CompositionRoot.CreateCreditNotePresenter);
        }

        private void Clean()
        {
            txtdocumentclient.Text = "";
            txtnameclient.Text = "";
            txtrectaxid.Text = "";
            txtrecname.Text = "";
            txtrecactivity.Text = "";
            txtrecaddress.Text = "";
            txtreccommune.Text = "";
            txttotalpay.Text = "0";
            txtpaywith.Text = "0";
            txtchange.Text = "0";
            dgdata.Rows.Clear();

            // Back to Boleta and hide the recipient panel: otherwise the next sale silently
            // starts in Factura mode with the previous receptor still on screen (DEF-31).
            if (cbodocumenttype.Items.Count > 0 && cbodocumenttype.SelectedIndex != 0)
            {
                cbodocumenttype.SelectedIndex = 0; // fires OnDocumentTypeChanged -> hides pnlFactura
            }
            else
            {
                pnlFactura.Visible = false;
            }
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

        int ISaleView.SelectedProductId => ViewParse.Int(txtidproduct.Text);
        string ISaleView.SelectedProductName => txtnameproduct.Text.Trim();
        int ISaleView.Stock => ViewParse.Int(txtstock.Text);
        decimal ISaleView.Amount => txtamount.Value;
        string ISaleView.PriceSaleText => txtpricesale.Text;

        string ISaleView.DocumentClient => txtdocumentclient.Text;
        string ISaleView.NameClient => txtnameclient.Text;
        string ISaleView.PayWithText => txtpaywith.Text;
        string ISaleView.TotalPayText => txttotalpay.Text;
        string ISaleView.ChangeText => txtchange.Text;
        string ISaleView.DocumentType => ((ComboBoxItem)cbodocumenttype.SelectedItem)?.Value.ToString() ?? "";
        string ISaleView.PaymentMethod => ((ComboBoxItem)cbopaymentmethod.SelectedItem)?.Value.ToString() ?? "";

        public string RecipientTaxId => txtrectaxid.Text;
        public string RecipientBusinessName => txtrecname.Text;
        public string RecipientActivity => txtrecactivity.Text;
        public string RecipientAddress => txtrecaddress.Text;
        public string RecipientCommune => txtreccommune.Text;

        public void SetFacturaFieldsVisible(bool visible) => pnlFactura.Visible = visible;

        public void SetClient(string document, string name)
        {
            txtdocumentclient.Text = document ?? "";
            txtnameclient.Text = name ?? "";
        }

        public void SetRecipient(string taxId, string businessName, string activity, string address, string commune)
        {
            txtrectaxid.Text = taxId ?? "";
            txtrecname.Text = businessName ?? "";
            txtrecactivity.Text = activity ?? "";
            txtrecaddress.Text = address ?? "";
            txtreccommune.Text = commune ?? "";
        }

        private void cbodocumenttype_SelectedIndexChanged(object sender, EventArgs e) => _presenter.OnDocumentTypeChanged();

        public void ShowMessage(string message) =>
            MessageBox.Show(message, "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

        public void SetDocumentTypeOptions(IReadOnlyList<string> options, string selected) =>
            FillOptionCombo(cbodocumenttype, options, selected);

        public void SetPaymentMethodOptions(IReadOnlyList<string> options, string selected) =>
            FillOptionCombo(cbopaymentmethod, options, selected);

        IReadOnlyList<SalePaymentEntry> ISaleView.PromptPaymentSplit(decimal total, IReadOnlyList<SalePaymentEntry> current, IReadOnlyList<string> methods)
        {
            using (var modal = new ModalSalePayments(total, current, methods))
            {
                return modal.ShowDialog(this) == DialogResult.OK ? modal.Result : null;
            }
        }

        public void ShowPaymentSplit(IReadOnlyList<SalePaymentEntry> split)
        {
            bool mixed = split != null && split.Count > 1;
            cbopaymentmethod.Enabled = !mixed;
            btnMixedPayment.Text = mixed ? "Editar pago mixto…" : "Pago mixto…";
            lblMixedPayment.Visible = mixed;
            lblMixedPayment.Text = mixed
                ? "Pago mixto: " + string.Join("  +  ",
                      split.Select(s => s.Method + " " + CultureInfoHelper.FormatAsCurrency(s.Amount)))
                : "";
        }

        private static void FillOptionCombo(ComboBox combo, IReadOnlyList<string> options, string selected)
        {
            combo.Items.Clear();
            foreach (string option in options)
            {
                combo.Items.Add(new ComboBoxItem { Value = option, Text = option });
            }

            int index = 0;
            for (int i = 0; i < options.Count; i++)
            {
                if (string.Equals(options[i], selected, StringComparison.OrdinalIgnoreCase))
                {
                    index = i;
                    break;
                }
            }
            if (combo.Items.Count > 0)
            {
                combo.SelectedIndex = index;
            }
        }

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
