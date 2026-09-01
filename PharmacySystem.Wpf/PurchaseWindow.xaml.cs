using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using PharmacySystem.Helpers;
using PharmacySystem.Presentation;
using PharmacySystem.Validators;

namespace PharmacySystem.Ui
{
    // WPF port of frmPurchase. Implements the same IPurchaseView; PurchasePresenter owns the cart
    // and is unchanged. Supplier / product lookups go through the WPF pickers.
    public partial class PurchaseWindow : Wpf.Ui.Controls.FluentWindow, IPurchaseView
    {
        public class PurchaseLineVm
        {
            public string Code { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string QuantityText { get; set; } = string.Empty;
            public string ExpiryText { get; set; } = string.Empty;
            public string PriceText { get; set; } = string.Empty;
            public string SubTotalText { get; set; } = string.Empty;
        }

        private readonly PurchasePresenter _presenter;
        private readonly PickerFactories _pickers;
        private readonly ObservableCollection<PurchaseLineVm> _lines = new ObservableCollection<PurchaseLineVm>();

        private int _selectedProductId;
        private int _supplierId;

        public PurchaseWindow(Func<IPurchaseView, PurchasePresenter> presenterFactory, PickerFactories pickers)
        {
            InitializeComponent();

            _pickers = pickers;

            cboDocType.ItemsSource = new[] { "Factura" };
            cboDocType.SelectedIndex = 0;
            dpExpiry.SelectedDate = DateTime.Today;
            dpExpiry.DisplayDateStart = DateTime.Today;
            dgCart.ItemsSource = _lines;

            _presenter = presenterFactory(this);
        }

        #region IPurchaseView

        public int SelectedProductId => _selectedProductId;
        public string SelectedProductCode => txtCode.Text.Trim();
        public string SelectedProductName => txtProductName.Text.Trim();
        public decimal Amount =>
            decimal.TryParse(txtAmount.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal v)
            || decimal.TryParse(txtAmount.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out v)
                ? v : 0m;
        public DateTime ExpirationDate => dpExpiry.SelectedDate ?? DateTime.Today;
        public string PricePurchaseText => txtPrice.Text;

        public string DocumentNumber => txtDocNumber.Text.Trim();
        public string DocumentType => cboDocType.SelectedItem?.ToString() ?? "Factura";
        public int SelectedSupplierId => _supplierId;

        // Mirrors frmPurchase's ValidateForm() rule set (Validations.cs).
        public List<string> ValidateProductEntry()
        {
            var errors = new List<string>();

            NotEmptyOrDocument(errors, "Número Documento", txtDocNumber.Text, checkDocument: true);
            NotEmptyOrDocument(errors, "Documento Proveedor", txtSupplierDoc.Text, checkDocument: true);
            NotEmpty(errors, "Razón Social Proveedor", txtSupplierName.Text);
            NotEmpty(errors, "Código Producto", txtCode.Text);
            NotEmpty(errors, "Nombre Producto", txtProductName.Text);
            NotEmpty(errors, "Cantidad", txtAmount.Text);
            NotEmpty(errors, "Precio Compra", txtPrice.Text);

            return errors;
        }

        private static void NotEmpty(List<string> errors, string label, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                errors.Add($"{label} : Este campo no puede estar vacío");
        }

        private static void NotEmptyOrDocument(List<string> errors, string label, string value, bool checkDocument)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                errors.Add($"{label} : Este campo no puede estar vacío");
            }
            else if (checkDocument && !DocumentValidator.IsValid(value))
            {
                errors.Add($"{label} : Documento inválido: use entre 3 y 20 letras, números, punto o guion");
            }
        }

        public void ShowValidationErrors(IReadOnlyList<string> errors) =>
            MessageBox.Show(this, string.Join("\n", errors), "Errores de validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);

        public void ShowMessage(string message) =>
            MessageBox.Show(this, message, "Mensaje", MessageBoxButton.OK, MessageBoxImage.Exclamation);

        public void FocusDocumentNumber() => txtDocNumber.Focus();

        public void SetSelectedProduct(int id, string code, string name)
        {
            _selectedProductId = id;
            txtCode.Text = code;
            txtProductName.Text = name;
        }

        public void AddCartLine(PurchaseCartLine line) => _lines.Add(new PurchaseLineVm
        {
            Code = line.Code,
            Name = line.Name,
            QuantityText = line.Quantity.ToString("0.##", CultureInfo.CurrentCulture),
            ExpiryText = line.ExpirationDate.ToShortDateString(),
            PriceText = CultureInfoHelper.FormatAsCurrency(line.PurchasePrice),
            SubTotalText = CultureInfoHelper.FormatAsCurrency(line.SubTotal)
        });

        public void RemoveCartLineAt(int index)
        {
            if (index >= 0 && index < _lines.Count)
                _lines.RemoveAt(index);
        }

        public void SetTotalText(string formattedTotal) => lblTotal.Text = "Total: " + formattedTotal;

        public void SetVatBreakdown(decimal net, decimal tax, decimal exempt)
        {
            string text = $"Neto: {CultureInfoHelper.FormatAsCurrency(net)}   IVA: {CultureInfoHelper.FormatAsCurrency(tax)}";
            if (exempt > 0)
                text += $"   Exento: {CultureInfoHelper.FormatAsCurrency(exempt)}";
            lblVat.Text = text;
        }

        public void ClearProductEntry()
        {
            _selectedProductId = 0;
            txtCode.Clear();
            txtProductName.Clear();
            txtAmount.Text = "1";
            txtPrice.Clear();
        }

        public void ClearPurchase()
        {
            ClearProductEntry();
            cboDocType.SelectedIndex = 0;
            txtDocNumber.Clear();
            txtSupplierDoc.Clear();
            txtSupplierName.Clear();
            _supplierId = 0;
            _lines.Clear();
            lblTotal.Text = "Total: 0";
            lblVat.Text = "";
        }

        #endregion

        private void btnPickSupplier_Click(object sender, RoutedEventArgs e)
        {
            SupplierRow? picked = SupplierPickerDialog.Show(OwnerHandle(), _pickers.Supplier);
            if (picked != null)
            {
                txtSupplierName.Text = picked.CompanyName;
                txtSupplierDoc.Text = picked.Document;
                _supplierId = picked.Id;
            }
        }

        private void btnPickProduct_Click(object sender, RoutedEventArgs e)
        {
            ProductPickerRow? picked = ProductPickerDialog.Show(OwnerHandle(),
                v => _pickers.Product(v, "frmPurchase"));
            if (picked != null)
            {
                SetSelectedProduct(picked.Id, picked.Code, picked.Name);
            }
        }

        private void txtCode_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
                _presenter.OnProductCodeEntered(txtCode.Text.Trim());
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e) => _presenter.OnAddProduct();

        private void btnRemoveLine_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is PurchaseLineVm vm)
            {
                int index = _lines.IndexOf(vm);
                if (index >= 0) _presenter.OnRemoveProduct(index);
            }
        }

        private void btnFinish_Click(object sender, RoutedEventArgs e) => _presenter.OnFinishPurchase();

        private IntPtr OwnerHandle() => new System.Windows.Interop.WindowInteropHelper(this).Handle;
    }
}
