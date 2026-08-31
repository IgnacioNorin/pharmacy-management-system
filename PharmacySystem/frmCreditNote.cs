using PharmacySystem.Helpers;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PharmacySystem
{
    // Hand-authored Designer (like frmRoles). Issues a Nota de Credito that reverses a sale.
    public partial class frmCreditNote : Form, ICreditNoteView
    {
        private const string ColSource = "SourceDetailId";
        private const string ColProduct = "Producto";
        private const string ColPrice = "PrecioUnit";
        private const string ColSold = "Vendido";
        private const string ColCredited = "Acreditado";
        private const string ColToCredit = "AAcreditar";

        private readonly CreditNotePresenter _presenter;

        public frmCreditNote()
        {
            InitializeComponent();
            BuildLineGrid();
            _presenter = CompositionRoot.CreateCreditNotePresenter(this);
        }

        private void BuildLineGrid()
        {
            dgvLines.Columns.Clear();
            dgvLines.Columns.Add(new DataGridViewTextBoxColumn { Name = ColSource, Visible = false });
            dgvLines.Columns.Add(new DataGridViewTextBoxColumn { Name = ColProduct, HeaderText = "Producto", ReadOnly = true, FillWeight = 34 });
            dgvLines.Columns.Add(new DataGridViewTextBoxColumn { Name = ColPrice, HeaderText = "Precio unit.", ReadOnly = true, FillWeight = 16 });
            dgvLines.Columns.Add(new DataGridViewTextBoxColumn { Name = ColSold, HeaderText = "Vendido", ReadOnly = true, FillWeight = 14 });
            dgvLines.Columns.Add(new DataGridViewTextBoxColumn { Name = ColCredited, HeaderText = "Acreditado", ReadOnly = true, FillWeight = 16 });
            dgvLines.Columns.Add(new DataGridViewTextBoxColumn { Name = ColToCredit, HeaderText = "A acreditar", FillWeight = 20 });
        }

        private void frmCreditNote_Load(object sender, EventArgs e) => _presenter.OnLoad();

        #region ICreditNoteView

        public string DocumentTypeInput => cboType.SelectedItem?.ToString() ?? "";
        public string DocumentNumberInput => txtNumber.Text;
        public string ReasonInput => txtReason.Text;

        public bool ConfirmGenerate() =>
            MessageBox.Show("¿Emitir la nota de crédito para este comprobante?", "Confirmar",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

        public void SetDocumentTypeOptions(IReadOnlyList<string> options)
        {
            cboType.Items.Clear();
            foreach (string option in options)
            {
                cboType.Items.Add(option);
            }
            if (cboType.Items.Count > 0)
            {
                cboType.SelectedIndex = 0;
            }
        }

        public void ShowSale(SaleLookup sale)
        {
            string state = sale.IsCreditNote ? "Nota de crédito"
                : sale.FullyCreditNoted ? "Acreditada por completo"
                : sale.AlreadyCreditNoted ? "Acreditada en parte"
                : "Vigente";

            lblDetail.Text =
                $"{sale.DocumentType} N° {sale.DocumentNumber}\r\n" +
                $"Fecha: {sale.Date:dd/MM/yyyy HH:mm}\r\n" +
                $"Cliente: {sale.ClientName}\r\n" +
                $"Total: {CultureInfoHelper.FormatAsCurrency(sale.TotalAmount)}\r\n" +
                $"Estado: {state}";
        }

        public void ShowCreditableLines(IReadOnlyList<SaleCreditDetail> lines)
        {
            dgvLines.Rows.Clear();
            foreach (SaleCreditDetail line in lines)
            {
                int index = dgvLines.Rows.Add(
                    line.SourceDetailId,
                    line.ProductName,
                    CultureInfoHelper.FormatAsCurrency(line.UnitPrice),
                    line.SoldQuantity,
                    line.CreditedQuantity,
                    line.RemainingQuantity);

                DataGridViewRow row = dgvLines.Rows[index];
                // A fully-credited line has nothing left to credit - lock it at 0.
                if (line.RemainingQuantity <= 0)
                {
                    row.Cells[ColToCredit].Value = 0;
                    row.Cells[ColToCredit].ReadOnly = true;
                    row.DefaultCellStyle.ForeColor = System.Drawing.Color.Gray;
                }
            }
        }

        public IReadOnlyList<CreditNoteLineRequest> GetRequestedQuantities()
        {
            var requests = new List<CreditNoteLineRequest>();
            foreach (DataGridViewRow row in dgvLines.Rows)
            {
                if (row.IsNewRow) continue;
                int quantity = ViewParse.Int(row.Cells[ColToCredit].Value?.ToString());
                requests.Add(new CreditNoteLineRequest
                {
                    SourceDetailId = ViewParse.Int(row.Cells[ColSource].Value?.ToString()),
                    Quantity = quantity < 0 ? 0 : quantity
                });
            }
            return requests;
        }

        public void ClearSale()
        {
            lblDetail.Text = "Sin comprobante seleccionado.";
            dgvLines.Rows.Clear();
        }

        public void SetGenerateEnabled(bool enabled) => btnGenerate.Enabled = enabled;

        public void ShowMessage(string message) =>
            MessageBox.Show(message, "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Information);

        public void CreditNoteCompleted()
        {
            ClearSale();
            txtReason.Clear();
            txtNumber.Clear();
            btnGenerate.Enabled = false;
        }

        #endregion

        private void btnSearch_Click(object sender, EventArgs e) => _presenter.OnSearch();
        private void btnGenerate_Click(object sender, EventArgs e) => _presenter.OnGenerate();
        private void btnClose_Click(object sender, EventArgs e) => Close();
    }
}
