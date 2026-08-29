using PharmacySystem.Helpers;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using PharmacySystem.Validators;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PharmacySystem
{
    public partial class frmPurchase : Form, IPurchaseView
    {
        private readonly PurchasePresenter _presenter;

        public frmPurchase(int IdPerson = 0)
        {
            InitializeComponent();
            _presenter = CompositionRoot.CreatePurchasePresenter(this, IdPerson);
        }

        private Dictionary<TextBox, List<string>> campWithRules = new Dictionary<TextBox, List<string>>();
        private Dictionary<string, string> namesMessages = new Dictionary<string, string>
        {
            { "txtnumberdocument", "Número Documento" },
            { "txtdocumentsupplier", "Documento Proveedor" },
            { "txtnamesupplier", "Razón Social Proveedor" },
            { "txtcodeproduct", "Código Producto" },
            { "txtnameproduct", "Nombre Producto" },
            { "txtamount", "Cantidad" },
            { "txtpricepurchase", "Precio Compra" },
        };
        private void InitializeValidators()
        {
            var txtAmountInternal = txtamount.Controls[1] as TextBox;
            txtAmountInternal.Name = "txtcantidad";
            campWithRules = new Dictionary<TextBox, List<string>>
            {
                { txtnumberdocument, new List<string>{ "NotEmpty", "ValidateDocument" } },
                { txtdocumentsupplier, new List<string>{ "NotEmpty", "ValidateDocument" } },
                { txtnamesupplier, new List<string>{ "NotEmpty" } },
                { txtcodeproduct, new List<string>{ "NotEmpty" } },
                { txtnameproduct, new List<string>{ "NotEmpty" } },
                { txtAmountInternal, new List<string>{ "NotEmpty" } },
                { txtpricepurchase, new List<string>{ "NotEmpty" } },
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
            dgdata.Columns.Add("SubTotal", "SubTotal");

            dgdata.Columns["IdProducto"].Visible = false;

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

        private void btnAdd_Click(object sender, EventArgs e) => _presenter.OnAddProduct();

        public void CleanProduct() {
            txtidproduct.Text = "0";
            txtcodeproduct.Text = "";
            txtnameproduct.Text = "";
            txtamount.Value = 1;
            txtpricepurchase.Text = "";
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
                    _presenter.OnRemoveProduct(index);
                }
            }
        }

        private void btnFinishPurchase_Click(object sender, EventArgs e) => _presenter.OnFinishPurchase();

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
                _presenter.OnProductCodeEntered(txtcodeproduct.Text.Trim());
            }
        }

        private List<string> ValidateForm()
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
            return errors;
        }

        #region IPurchaseView

        int IPurchaseView.SelectedProductId => int.Parse(txtidproduct.Text);
        string IPurchaseView.SelectedProductCode => txtcodeproduct.Text.Trim();
        string IPurchaseView.SelectedProductName => txtnameproduct.Text.Trim();
        decimal IPurchaseView.Amount => txtamount.Value;
        DateTime IPurchaseView.ExpirationDate => DTPexpireddate.Value;
        string IPurchaseView.PricePurchaseText => txtpricepurchase.Text;

        string IPurchaseView.DocumentNumber => txtnumberdocument.Text.Trim();
        string IPurchaseView.DocumentType => ((ComboBoxItem)cbotypedocument.SelectedItem).Value.ToString();
        int IPurchaseView.SelectedSupplierId => int.Parse(txtidsupplier.Text.Trim());

        List<string> IPurchaseView.ValidateProductEntry() => ValidateForm();

        void IPurchaseView.ShowValidationErrors(IReadOnlyList<string> errors) =>
            MessageBox.Show(string.Join("\n", errors), "Errores de Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        public void ShowMessage(string message) =>
            MessageBox.Show(message, "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

        public void FocusDocumentNumber() => txtnumberdocument.Focus();

        public void SetSelectedProduct(int id, string code, string name)
        {
            txtidproduct.Text = id.ToString();
            txtcodeproduct.Text = code;
            txtnameproduct.Text = name;
        }

        public void AddCartLine(PurchaseCartLine line)
        {
            int rowId = dgdata.Rows.Add();
            DataGridViewRow row = dgdata.Rows[rowId];

            row.Cells["IdProducto"].Value = line.ProductId.ToString();
            row.Cells["Codigo"].Value = line.Code;
            row.Cells["NombreProducto"].Value = line.Name;
            row.Cells["Cantidad"].Value = line.Quantity.ToString();
            row.Cells["FechaVencimiento"].Value = line.ExpirationDate.ToShortDateString();
            row.Cells["PrecioCompra"].Value = CultureInfoHelper.FormatAsCurrency(line.PurchasePrice);
            row.Cells["SubTotal"].Value = CultureInfoHelper.FormatAsCurrency(line.SubTotal);
        }

        public void RemoveCartLineAt(int index) => dgdata.Rows.RemoveAt(index);

        public void SetTotalText(string formattedTotal) => lbltotalamount.Text = formattedTotal;

        public void ClearProductEntry() => CleanProduct();

        public void ClearPurchase() => Clean();

        #endregion
    }
}
