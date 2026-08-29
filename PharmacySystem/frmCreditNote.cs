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
        private readonly CreditNotePresenter _presenter;

        public frmCreditNote()
        {
            InitializeComponent();
            _presenter = CompositionRoot.CreateCreditNotePresenter(this);
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
            lblDetail.Text =
                $"{sale.DocumentType} N° {sale.DocumentNumber}\r\n" +
                $"Fecha: {sale.Date:dd/MM/yyyy HH:mm}\r\n" +
                $"Cliente: {sale.ClientName}\r\n" +
                $"Total: {CultureInfoHelper.FormatAsCurrency(sale.TotalAmount)}\r\n" +
                $"Estado: {(sale.IsCreditNote ? "Nota de crédito" : sale.AlreadyCreditNoted ? "Ya anulada" : "Vigente")}";
        }

        public void ClearSale() => lblDetail.Text = "Sin comprobante seleccionado.";

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
