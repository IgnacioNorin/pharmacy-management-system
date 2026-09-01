using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using PharmacySystem.Helpers;
using PharmacySystem.Model;
using PharmacySystem.Presentation;

namespace PharmacySystem.Ui
{
    // WPF port of frmCreditNote. Implements the same ICreditNoteView; CreditNotePresenter is
    // unchanged. Search a sale, choose how many units of each line to credit, generate.
    public partial class CreditNoteWindow : Window, ICreditNoteView
    {
        // Grid row model. ToCredit is edited in place; the presenter reads it back on generate.
        public class CreditLineVm
        {
            public int SourceDetailId { get; set; }
            public string Product { get; set; } = string.Empty;
            public string PriceText { get; set; } = string.Empty;
            public int Sold { get; set; }
            public int Credited { get; set; }
            public int Remaining { get; set; }
            public int ToCredit { get; set; }
            public bool Editable { get; set; }
        }

        private readonly CreditNotePresenter _presenter;

        public CreditNoteWindow(Func<ICreditNoteView, CreditNotePresenter> presenterFactory)
        {
            InitializeComponent();
            _presenter = presenterFactory(this);
            Loaded += (s, e) => _presenter.OnLoad();
        }

        #region ICreditNoteView

        public string DocumentTypeInput => cboType.SelectedItem?.ToString() ?? "";
        public string DocumentNumberInput => txtNumber.Text;
        public string ReasonInput => txtReason.Text;

        public bool ConfirmGenerate() =>
            MessageBox.Show(this, "¿Emitir la nota de crédito para este comprobante?", "Confirmar",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

        public void SetDocumentTypeOptions(IReadOnlyList<string> options)
        {
            cboType.ItemsSource = options;
            if (options.Count > 0)
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
                $"{sale.DocumentType} N° {sale.DocumentNumber}\n" +
                $"Fecha: {sale.Date:dd/MM/yyyy HH:mm}\n" +
                $"Cliente: {sale.ClientName}\n" +
                $"Total: {CultureInfoHelper.FormatAsCurrency(sale.TotalAmount)}\n" +
                $"Estado: {state}";
        }

        public void ShowCreditableLines(IReadOnlyList<SaleCreditDetail> lines)
        {
            dgLines.ItemsSource = lines.Select(l => new CreditLineVm
            {
                SourceDetailId = l.SourceDetailId,
                Product = l.ProductName,
                PriceText = CultureInfoHelper.FormatAsCurrency(l.UnitPrice),
                Sold = l.SoldQuantity,
                Credited = l.CreditedQuantity,
                Remaining = l.RemainingQuantity,
                ToCredit = 0,
                Editable = l.RemainingQuantity > 0
            }).ToList();
        }

        public IReadOnlyList<CreditNoteLineRequest> GetRequestedQuantities()
        {
            dgLines.CommitEdit(System.Windows.Controls.DataGridEditingUnit.Row, true);

            var rows = dgLines.ItemsSource as IEnumerable<CreditLineVm>;
            if (rows == null)
            {
                return new List<CreditNoteLineRequest>();
            }

            return rows.Select(r => new CreditNoteLineRequest
            {
                SourceDetailId = r.SourceDetailId,
                Quantity = r.ToCredit < 0 ? 0 : r.ToCredit
            }).ToList();
        }

        public void ClearSale()
        {
            lblDetail.Text = "Sin comprobante seleccionado.";
            dgLines.ItemsSource = null;
        }

        public void SetGenerateEnabled(bool enabled) => btnGenerate.IsEnabled = enabled;

        public void ShowMessage(string message) =>
            MessageBox.Show(this, message, "Mensaje", MessageBoxButton.OK, MessageBoxImage.Information);

        public void CreditNoteCompleted()
        {
            ClearSale();
            txtReason.Clear();
            txtNumber.Clear();
            btnGenerate.IsEnabled = false;
        }

        #endregion

        private void btnSearch_Click(object sender, RoutedEventArgs e) => _presenter.OnSearch();
        private void btnGenerate_Click(object sender, RoutedEventArgs e) => _presenter.OnGenerate();
        private void btnClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}
