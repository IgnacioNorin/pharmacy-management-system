using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using PharmacySystem.Helpers;
using PharmacySystem.Presentation;

namespace PharmacySystem.Wpf
{
    // WPF port of frmSale. Implements the same ISaleView; SalePresenter owns the cart and the
    // payment split and is unchanged. Client / product lookups, the "pago mixto" dialog, the
    // credit-note screen and the ticket print all go through the shell hooks / WPF dialogs.
    public partial class SaleWindow : Window, ISaleView
    {
        public class SaleLineVm
        {
            public string Name { get; set; }
            public string QuantityText { get; set; }
            public string PriceText { get; set; }
            public string SubTotalText { get; set; }
        }

        private readonly SalePresenter _presenter;
        private readonly SaleShellHooks _hooks;
        private readonly ObservableCollection<SaleLineVm> _lines = new ObservableCollection<SaleLineVm>();

        private int _selectedProductId;
        private int _selectedStock;

        public SaleWindow(Func<ISaleView, SalePresenter> presenterFactory, SaleShellHooks hooks)
        {
            InitializeComponent();

            _hooks = hooks;
            dgCart.ItemsSource = _lines;
            btnCreditNote.IsEnabled = hooks.CanCreditNote;

            _presenter = presenterFactory(this);
            Loaded += (s, e) => _presenter.OnLoad();
        }

        #region ISaleView

        public int SelectedProductId => _selectedProductId;
        public string SelectedProductName => txtProductName.Text.Trim();
        public int Stock => _selectedStock;
        public decimal Amount =>
            decimal.TryParse(txtAmount.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal v)
            || decimal.TryParse(txtAmount.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out v)
                ? v : 0m;
        public string PriceSaleText => txtPrice.Text;

        public string DocumentClient => txtClientDoc.Text;
        public string NameClient => txtClientName.Text;
        public string PayWithText => txtPayWith.Text;
        public string TotalPayText => txtTotal.Text;
        public string ChangeText => txtChange.Text;
        public string DocumentType => cboDocType.SelectedItem?.ToString() ?? "";
        public string PaymentMethod => cboPaymentMethod.SelectedItem?.ToString() ?? "";

        public string RecipientTaxId => txtRecTaxId.Text;
        public string RecipientBusinessName => txtRecName.Text;
        public string RecipientActivity => txtRecActivity.Text;
        public string RecipientAddress => txtRecAddress.Text;
        public string RecipientCommune => txtRecCommune.Text;

        public void ShowMessage(string message) =>
            MessageBox.Show(this, message, "Mensaje", MessageBoxButton.OK, MessageBoxImage.Exclamation);

        public void SetDocumentTypeOptions(IReadOnlyList<string> options, string selected) =>
            FillCombo(cboDocType, options, selected);

        public void SetPaymentMethodOptions(IReadOnlyList<string> options, string selected) =>
            FillCombo(cboPaymentMethod, options, selected);

        public IReadOnlyList<SalePaymentEntry> PromptPaymentSplit(decimal total,
            IReadOnlyList<SalePaymentEntry> current, IReadOnlyList<string> methods)
        {
            var window = new SalePaymentsWindow(total, current, methods) { Owner = this };
            return window.ShowDialog() == true ? window.Result : null;
        }

        public void ShowPaymentSplit(IReadOnlyList<SalePaymentEntry> split)
        {
            bool mixed = split != null && split.Count > 1;
            cboPaymentMethod.IsEnabled = !mixed;
            btnMixedPayment.Content = mixed ? "Editar pago mixto…" : "Pago mixto…";
            lblMixedPayment.Text = mixed
                ? "Pago mixto: " + string.Join("  +  ",
                      SelectMethods(split))
                : "";
        }

        private static IEnumerable<string> SelectMethods(IReadOnlyList<SalePaymentEntry> split)
        {
            foreach (SalePaymentEntry entry in split)
                yield return entry.Method + " " + CultureInfoHelper.FormatAsCurrency(entry.Amount);
        }

        public void SetFacturaFieldsVisible(bool visible) =>
            grpFactura.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;

        public void SetClient(string document, string name)
        {
            txtClientDoc.Text = document ?? "";
            txtClientName.Text = name ?? "";
        }

        public void SetRecipient(string taxId, string businessName, string activity, string address, string commune)
        {
            txtRecTaxId.Text = taxId ?? "";
            txtRecName.Text = businessName ?? "";
            txtRecActivity.Text = activity ?? "";
            txtRecAddress.Text = address ?? "";
            txtRecCommune.Text = commune ?? "";
        }

        public void SetSelectedProduct(int id, string code, string name, int stock, string priceSaleFormatted)
        {
            _selectedProductId = id;
            _selectedStock = stock;
            txtCode.Text = code;
            txtProductName.Text = name;
            txtPrice.Text = priceSaleFormatted;
        }

        public void AddCartLine(SaleCartLine line) => _lines.Add(new SaleLineVm
        {
            Name = line.Name,
            QuantityText = line.Quantity.ToString("0.##", CultureInfo.CurrentCulture),
            PriceText = CultureInfoHelper.FormatAsCurrency(line.SalePrice),
            SubTotalText = CultureInfoHelper.FormatAsCurrency(line.SubTotal)
        });

        public void RemoveCartLineAt(int index)
        {
            if (index >= 0 && index < _lines.Count)
                _lines.RemoveAt(index);
        }

        public void SetTotalText(string formattedTotal) => txtTotal.Text = formattedTotal;
        public void SetChangeText(string formattedChange) => txtChange.Text = formattedChange;

        public void ClearProductEntry()
        {
            _selectedProductId = 0;
            _selectedStock = 0;
            txtCode.Clear();
            txtProductName.Clear();
            txtAmount.Text = "1";
            txtPrice.Clear();
        }

        public void ClearSale()
        {
            txtClientDoc.Clear();
            txtClientName.Clear();
            SetRecipient("", "", "", "", "");
            txtTotal.Text = "0";
            txtPayWith.Text = "0";
            txtChange.Text = "0";
            _lines.Clear();

            if (cboDocType.Items.Count > 0 && cboDocType.SelectedIndex != 0)
                cboDocType.SelectedIndex = 0; // fires OnDocumentTypeChanged -> hides the panel
            else
                grpFactura.Visibility = Visibility.Collapsed;
        }

        public void SaleRegistered(int idSale)
        {
            if (MessageBox.Show(this, "La venta fue registrada\n¿Desea imprimir el ticket ahora?", "Mensaje",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _hooks.PrintTicket?.Invoke(idSale);
            }
        }

        #endregion

        private static void FillCombo(ComboBox combo, IReadOnlyList<string> options, string selected)
        {
            combo.ItemsSource = options;
            int index = 0;
            for (int i = 0; i < options.Count; i++)
            {
                if (string.Equals(options[i], selected, StringComparison.OrdinalIgnoreCase)) { index = i; break; }
            }
            if (options.Count > 0) combo.SelectedIndex = index;
        }

        private void cboDocType_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            _presenter?.OnDocumentTypeChanged();

        private void btnPickClient_Click(object sender, RoutedEventArgs e)
        {
            ClientRow picked = ClientPickerDialog.Show(OwnerHandle(), _hooks.Pickers.Client);
            if (picked != null)
                _presenter.OnClientSelected(picked);
        }

        private void btnPickProduct_Click(object sender, RoutedEventArgs e)
        {
            ProductPickerRow picked = ProductPickerDialog.Show(OwnerHandle(),
                v => _hooks.Pickers.Product(v, "frmSale"));
            if (picked != null)
                SetSelectedProduct(picked.Id, picked.Code, picked.Name, picked.Stock,
                    CultureInfoHelper.FormatAsCurrency(picked.SalePrice));
        }

        private void txtCode_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
                _presenter.OnProductCodeEntered(txtCode.Text.Trim());
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e) => _presenter.OnAddProduct();

        private void btnRemoveLine_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is SaleLineVm vm)
            {
                int index = _lines.IndexOf(vm);
                if (index >= 0) _presenter.OnRemoveProduct(index);
            }
        }

        private void btnCalc_Click(object sender, RoutedEventArgs e) => _presenter.OnCalculateChangeRequested();
        private void btnMixedPayment_Click(object sender, RoutedEventArgs e) => _presenter.OnSplitPaymentRequested();
        private void btnFinish_Click(object sender, RoutedEventArgs e) => _presenter.OnFinishSale();

        private void btnCreditNote_Click(object sender, RoutedEventArgs e) =>
            CreditNoteDialog.Show(OwnerHandle(), _hooks.CreditNoteFactory);

        private IntPtr OwnerHandle() => new System.Windows.Interop.WindowInteropHelper(this).Handle;
    }
}
