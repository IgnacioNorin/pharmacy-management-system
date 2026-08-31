using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PharmacySystem.Helpers;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using ComboBoxItem = PharmacySystem.Model.ComboBoxItem;

namespace PharmacySystem.Wpf
{
    // WPF port of frmManagement. One window, four tabs, each a passive view for its own presenter:
    // Productos (IProductManagementView), Categorías (ICategoryManagementView), Precios
    // (IProductPriceView) and Tienda (IStoreManagementView). The presenter/service/repository
    // layers are unchanged; a tab the role can't use is removed on load.
    public partial class ManagementWindow : Window,
        ICategoryManagementView, IProductManagementView, IProductPriceView, IStoreManagementView
    {
        private readonly ManagementPresenterFactories _factories;
        private readonly ManagementPermissions _permissions;

        private readonly CategoryManagementPresenter _categoryPresenter;
        private readonly ProductManagementPresenter _productPresenter;
        private readonly ProductPricePresenter _productPricePresenter;
        private readonly StoreManagementPresenter _storePresenter;

        private readonly ObservableCollection<CategoryRowVm> _categoryRows = new ObservableCollection<CategoryRowVm>();
        private readonly ObservableCollection<ProductRowVm> _productRows = new ObservableCollection<ProductRowVm>();
        private readonly ObservableCollection<PriceRowVm> _releasableRows = new ObservableCollection<PriceRowVm>();
        private readonly ObservableCollection<PriceRowVm> _commercializedRows = new ObservableCollection<PriceRowVm>();
        private readonly ObservableCollection<PriceHistoryRowVm> _priceHistoryRows = new ObservableCollection<PriceHistoryRowVm>();

        private int _categoryId;
        private int _categorySelectedIndex;
        private int _productId;
        private int _productSelectedIndex;
        private int _priceSelectedId;

        private bool _suppressCategorySelection;
        private bool _suppressProductSelection;
        private bool _suppressPriceSelection;

        // Set by the alerts click-through: jump to the Productos tab filtered to this code once
        // the presenters have loaded.
        private string _pendingProductCode;

        public ManagementWindow(ManagementPresenterFactories factories, ManagementPermissions permissions, string pendingProductCode = null)
        {
            InitializeComponent();

            _factories = factories;
            _permissions = permissions ?? new ManagementPermissions();
            _pendingProductCode = string.IsNullOrEmpty(pendingProductCode) ? null : pendingProductCode;

            dgCategory.ItemsSource = _categoryRows;
            dgProduct.ItemsSource = _productRows;
            dgReleasable.ItemsSource = _releasableRows;
            dgCommercialized.ItemsSource = _commercializedRows;
            dgPriceHistory.ItemsSource = _priceHistoryRows;

            _categoryPresenter = _factories.Category(this);
            _productPresenter = _factories.Product(this);
            _productPricePresenter = _factories.ProductPrice(this);
            _storePresenter = _factories.Store(this);

            Loaded += ManagementWindow_Loaded;
        }

        private void ManagementWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // The category list also backs the product form's category combo, so load it whenever
            // either tab is going to be shown.
            if (_permissions.Categories || _permissions.Products)
            {
                _categoryPresenter.OnLoad();
            }
            if (!_permissions.Categories)
            {
                tabControl.Items.Remove(tabCategory);
            }

            if (_permissions.Products)
            {
                _productPresenter.OnLoad();
            }
            else
            {
                tabControl.Items.Remove(tabProduct);
            }

            if (_permissions.ProductPrices)
            {
                _productPricePresenter.OnLoad();
            }
            else
            {
                tabControl.Items.Remove(tabPrices);
            }

            if (_permissions.Store)
            {
                _storePresenter.OnLoad();
            }
            else
            {
                tabControl.Items.Remove(tabStore);
            }

            if (_pendingProductCode != null && _permissions.Products)
            {
                tabControl.SelectedItem = tabProduct;
                txtSearchProduct.Text = _pendingProductCode;
                _productPresenter.OnSearch();
            }
            _pendingProductCode = null;
        }

        // Entry point for the notification center's click-through (Fase 3 of the alerts rework):
        // jumps straight to the Productos tab filtered to this code.
        public void ShowProductByCode(string code)
        {
            if (string.IsNullOrEmpty(code) || !_permissions.Products) return;
            tabControl.SelectedItem = tabProduct;
            txtSearchProduct.Text = code;
            _productPresenter.OnSearch();
        }

        private static int ComboInt(ComboBox combo)
        {
            object value = (combo.SelectedItem as ComboBoxItem)?.Value;
            return value != null && int.TryParse(value.ToString(), out int id) ? id : 0;
        }

        private static string ComboText(ComboBox combo) => (combo.SelectedItem as ComboBoxItem)?.Text ?? "";

        private static void SelectComboByValue(ComboBox combo, int value)
        {
            foreach (ComboBoxItem item in combo.Items.OfType<ComboBoxItem>())
            {
                if (item.Value != null && int.TryParse(item.Value.ToString(), out int id) && id == value)
                {
                    combo.SelectedItem = item;
                    return;
                }
            }
        }

        private static void SelectComboByText(ComboBox combo, string text)
        {
            foreach (ComboBoxItem item in combo.Items.OfType<ComboBoxItem>())
            {
                if (item.Text == text)
                {
                    combo.SelectedItem = item;
                    return;
                }
            }
        }

        private void FillCategoryCombo(IEnumerable<ComboBoxItem> options)
        {
            cboCategory.Items.Clear();
            foreach (ComboBoxItem item in options) cboCategory.Items.Add(item);
            if (cboCategory.Items.Count > 0) cboCategory.SelectedIndex = 0;
        }

        // Shared by three of the four view interfaces.
        public void ShowMessage(string message) =>
            MessageBox.Show(this, message, "Mensaje", MessageBoxButton.OK, MessageBoxImage.Exclamation);

        public void ShowValidationErrors(IReadOnlyList<string> errors) =>
            MessageBox.Show(this, string.Join("\n", errors), "Errores de Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);

        #region ICategoryManagementView

        int ICategoryManagementView.SelectedIndex => _categorySelectedIndex;
        int ICategoryManagementView.RowCount => _categoryRows.Count;
        public int CategoryId => _categoryId;
        string ICategoryManagementView.Description => txtDescriptionCategory.Text;

        List<string> ICategoryManagementView.Validate()
        {
            var errors = new List<string>();
            FieldRules.Check(errors, "Descripcion", txtDescriptionCategory.Text.Trim(), "NotEmpty", "ValidateMaxLength");
            return errors;
        }

        bool ICategoryManagementView.ConfirmDelete() =>
            MessageBox.Show(this, "¿Desea eliminar la categoria?", "Mensaje",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

        public void LoadCategories(IEnumerable<CategoryRow> categories)
        {
            _suppressCategorySelection = true;
            _categoryRows.Clear();
            foreach (CategoryRow row in categories) _categoryRows.Add(new CategoryRowVm(row));
            _suppressCategorySelection = false;
        }

        void ICategoryManagementView.AddRow(CategoryRow row) => _categoryRows.Add(new CategoryRowVm(row));

        void ICategoryManagementView.ReplaceRow(int index, CategoryRow row)
        {
            if (index >= 0 && index < _categoryRows.Count) _categoryRows[index] = new CategoryRowVm(row);
        }

        void ICategoryManagementView.RemoveRow(int index)
        {
            if (index >= 0 && index < _categoryRows.Count) _categoryRows.RemoveAt(index);
        }

        void ICategoryManagementView.ClearForm()
        {
            _categoryId = 0;
            _categorySelectedIndex = 0;
            txtDescriptionCategory.Clear();
            _suppressCategorySelection = true;
            dgCategory.SelectedItem = null;
            _suppressCategorySelection = false;
        }

        public void RefreshProductCategoryOptions(IEnumerable<ComboBoxItem> options) => FillCategoryCombo(options);

        private void dgCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_suppressCategorySelection) return;
            if (!(dgCategory.SelectedItem is CategoryRowVm row)) return;

            _categorySelectedIndex = dgCategory.SelectedIndex + 1;
            _categoryId = row.Id;
            txtDescriptionCategory.Text = row.Description;
        }

        private void btnSaveCategory_Click(object sender, RoutedEventArgs e) => _categoryPresenter.OnSave();
        private void btnDeleteCategory_Click(object sender, RoutedEventArgs e) => _categoryPresenter.OnDelete();
        private void btnCleanCategory_Click(object sender, RoutedEventArgs e) => ((ICategoryManagementView)this).ClearForm();

        #endregion

        #region IProductManagementView

        int IProductManagementView.SelectedIndex => _productSelectedIndex;
        int IProductManagementView.RowCount => _productRows.Count;
        public string SearchText => txtSearchProduct.Text;
        public int ProductId => _productId;
        public string Code => txtCodeProduct.Text;
        string IProductManagementView.Name => txtNameProduct.Text;
        string IProductManagementView.Description => txtDescriptionProduct.Text;
        public int SelectedCategoryId => ComboInt(cboCategory);
        public string SelectedCategoryText => ComboText(cboCategory);
        public bool TaxAffected => chkTaxAffected.IsChecked == true;

        List<string> IProductManagementView.Validate()
        {
            var errors = new List<string>();
            FieldRules.Check(errors, "Codigo", txtCodeProduct.Text.Trim(), "NotEmpty", "ValidateMaxLength");
            FieldRules.Check(errors, "Nombre", txtNameProduct.Text.Trim(), "NotEmpty", "ValidateMaxLength");
            FieldRules.Check(errors, "Descripcion", txtDescriptionProduct.Text.Trim(), "NotEmpty", "ValidateMaxLength");
            FieldRules.Check(errors, "Categoria", (SelectedCategoryId == 0 ? "" : SelectedCategoryId.ToString()), "ComboNotEmpty");
            return errors;
        }

        bool IProductManagementView.ConfirmDelete() =>
            MessageBox.Show(this, "¿Desea eliminar el producto?", "Mensaje",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

        public void LoadCategoryOptions(IEnumerable<ComboBoxItem> options) => FillCategoryCombo(options);

        public void LoadProducts(IEnumerable<ManagementProductRow> products)
        {
            _suppressProductSelection = true;
            _productRows.Clear();
            foreach (ManagementProductRow row in products) _productRows.Add(new ProductRowVm(row));
            dgProduct.SelectedItem = null;
            _suppressProductSelection = false;
            _productId = 0;
            _productSelectedIndex = 0;
        }

        public void SetPageInfo(int currentPage, int totalPages, int totalCount)
        {
            lblProductPage.Text = totalCount == 0
                ? "Sin resultados"
                : $"Página {currentPage} de {totalPages}  ·  {totalCount} producto(s)";

            btnProductFirst.IsEnabled = btnProductPrev.IsEnabled = currentPage > 1;
            btnProductNext.IsEnabled = btnProductLast.IsEnabled = currentPage < totalPages;
        }

        void IProductManagementView.ClearForm()
        {
            _productId = 0;
            _productSelectedIndex = 0;
            txtCodeProduct.Clear();
            txtNameProduct.Clear();
            txtDescriptionProduct.Clear();
            chkTaxAffected.IsChecked = true;
            if (cboCategory.Items.Count > 0) cboCategory.SelectedIndex = 0;
            _suppressProductSelection = true;
            dgProduct.SelectedItem = null;
            _suppressProductSelection = false;
        }

        private void dgProduct_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_suppressProductSelection) return;
            if (!(dgProduct.SelectedItem is ProductRowVm row)) return;

            _productSelectedIndex = dgProduct.SelectedIndex + 1;
            _productId = row.Id;
            txtCodeProduct.Text = row.Code;
            txtNameProduct.Text = row.Name;
            txtDescriptionProduct.Text = row.Description;
            chkTaxAffected.IsChecked = row.TaxAffected;
            SelectComboByText(cboCategory, row.CategoryText);
        }

        private void btnSaveProduct_Click(object sender, RoutedEventArgs e) => _productPresenter.OnSave();
        private void btnDeleteProduct_Click(object sender, RoutedEventArgs e) => _productPresenter.OnDelete();
        private void btnCleanProduct_Click(object sender, RoutedEventArgs e) => ((IProductManagementView)this).ClearForm();
        private void btnSearchProduct_Click(object sender, RoutedEventArgs e) => _productPresenter.OnSearch();

        private void btnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            txtSearchProduct.Text = "";
            _productPresenter.OnSearch();
        }

        private void btnProductFirst_Click(object sender, RoutedEventArgs e) => _productPresenter.OnFirstPage();
        private void btnProductPrev_Click(object sender, RoutedEventArgs e) => _productPresenter.OnPreviousPage();
        private void btnProductNext_Click(object sender, RoutedEventArgs e) => _productPresenter.OnNextPage();
        private void btnProductLast_Click(object sender, RoutedEventArgs e) => _productPresenter.OnLastPage();

        private void btnProductLots_Click(object sender, RoutedEventArgs e)
        {
            if (_productId <= 0)
            {
                MessageBox.Show(this, "Seleccione un producto de la grilla primero.", "Lotes",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            IReadOnlyList<ProductLot> lots = _factories.LotsProvider(_productId);
            var window = new ProductLotsWindow(txtNameProduct.Text, lots) { Owner = this };
            window.ShowDialog();
        }

        #endregion

        #region IProductPriceView

        int IProductPriceView.SelectedProductId => _priceSelectedId;
        public string NewPriceText => txtNewPrice.Text;
        public string Reason => txtPriceReason.Text;

        public void LoadReleasable(IEnumerable<ProductPriceRow> rows) => FillPriceGrid(dgReleasable, _releasableRows, rows);
        public void LoadCommercialized(IEnumerable<ProductPriceRow> rows) => FillPriceGrid(dgCommercialized, _commercializedRows, rows);

        public void LoadHistory(IEnumerable<ProductPriceHistoryRow> entries)
        {
            _priceHistoryRows.Clear();
            foreach (ProductPriceHistoryRow entry in entries) _priceHistoryRows.Add(new PriceHistoryRowVm(entry));
        }

        public void ClearEntry()
        {
            txtNewPrice.Text = "";
            txtPriceReason.Text = "";
        }

        private void FillPriceGrid(DataGrid grid, ObservableCollection<PriceRowVm> target, IEnumerable<ProductPriceRow> rows)
        {
            _suppressPriceSelection = true;
            target.Clear();
            foreach (ProductPriceRow r in rows) target.Add(new PriceRowVm(r));
            grid.SelectedItem = null;
            _suppressPriceSelection = false;
        }

        private void dgPrice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_suppressPriceSelection) return;
            if (!(sender is DataGrid grid) || !(grid.SelectedItem is PriceRowVm row)) return;

            // The two price grids are mutually exclusive selections.
            _suppressPriceSelection = true;
            DataGrid other = ReferenceEquals(grid, dgReleasable) ? dgCommercialized : dgReleasable;
            other.SelectedItem = null;
            _suppressPriceSelection = false;

            _priceSelectedId = row.Id;
            lblSelectedProduct.Text = "Producto: " + row.Name + " (" + row.Code + ")";
            _productPricePresenter.OnSelectProduct(_priceSelectedId);
        }

        private void PriceEntry_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var box = (TextBox)sender;
            foreach (char c in e.Text)
            {
                if (char.IsDigit(c)) continue;
                bool dotAllowed = c == '.' && box.Text.Length > 0 && !box.Text.Contains(".");
                if (!dotAllowed) { e.Handled = true; return; }
            }
        }

        private void btnApplyPrice_Click(object sender, RoutedEventArgs e) => _productPricePresenter.OnApplyPrice();
        private void btnUnrelease_Click(object sender, RoutedEventArgs e) => _productPricePresenter.OnUnrelease();

        #endregion

        #region IStoreManagementView

        public string Document => txtTaxId.Text;
        public string CompanyName => txtLegalName.Text;
        public string Email => txtEmail.Text;
        public string Phone => txtPhone.Text;
        public string Address => txtAddress.Text;
        public string TaxRate => txtTaxRate.Text;
        public string DefaultDocumentType => cboDefaultDocType.SelectedItem?.ToString() ?? "";

        List<string> IStoreManagementView.Validate()
        {
            var errors = new List<string>();
            FieldRules.Check(errors, "Número Documento", txtTaxId.Text.Trim(), "NotEmpty", "ValidateDocument");
            FieldRules.Check(errors, "Razón Social", txtLegalName.Text.Trim(), "NotEmpty", "MaxLength150");
            FieldRules.Check(errors, "Correo", txtEmail.Text.Trim(), "NotEmpty", "ValidateEmail", "MaxLength120");
            FieldRules.Check(errors, "Teléfono", txtPhone.Text.Trim(), "NotEmpty", "OnlyNumbers");
            FieldRules.Check(errors, "Dirección", txtAddress.Text.Trim(), "NotEmpty", "MaxLength200");
            return errors;
        }

        public void LoadStoreFields(string document, string companyName, string email, string phone, string address)
        {
            txtTaxId.Text = document;
            txtLegalName.Text = companyName;
            txtEmail.Text = email;
            txtPhone.Text = phone;
            txtAddress.Text = address;
        }

        public void SetTaxRate(string value) => txtTaxRate.Text = value;

        public void LoadDocumentTypeOptions(IReadOnlyList<string> options, string selected)
        {
            cboDefaultDocType.Items.Clear();
            foreach (string option in options) cboDefaultDocType.Items.Add(option);

            int index = 0;
            for (int i = 0; i < options.Count; i++)
            {
                if (string.Equals(options[i], selected, StringComparison.OrdinalIgnoreCase)) { index = i; break; }
            }
            if (cboDefaultDocType.Items.Count > 0) cboDefaultDocType.SelectedIndex = index;
        }

        public void ShowInfo(string message) =>
            MessageBox.Show(this, message, "Mensaje", MessageBoxButton.OK, MessageBoxImage.Information);

        public void ShowError(string message) =>
            MessageBox.Show(this, message, "Mensaje", MessageBoxButton.OK, MessageBoxImage.Exclamation);

        private void btnSaveStore_Click(object sender, RoutedEventArgs e) => _storePresenter.OnSave();

        #endregion

        #region Row view-models

        public sealed class CategoryRowVm
        {
            public int Id { get; }
            public string Description { get; }
            public CategoryRowVm(CategoryRow row) { Id = row.Id; Description = row.Description; }
        }

        public sealed class ProductRowVm
        {
            public int Id { get; }
            public string Code { get; }
            public string Name { get; }
            public string Description { get; }
            public string CategoryText { get; }
            public string Stock { get; }
            public string ExpirationDateText { get; }
            public bool TaxAffected { get; }

            public ProductRowVm(ManagementProductRow row)
            {
                Id = row.Id;
                Code = row.Code;
                Name = row.Name;
                Description = row.Description;
                CategoryText = row.CategoryText;
                Stock = row.Stock;
                ExpirationDateText = row.ExpirationDateText;
                TaxAffected = row.TaxAffected;
            }
        }

        public sealed class PriceRowVm
        {
            public int Id { get; }
            public string Code { get; }
            public string Name { get; }
            public string Stock { get; }
            public string Cost { get; }
            public string SalePrice { get; }
            public string Margin { get; }
            public string Iva { get; }

            public PriceRowVm(ProductPriceRow r)
            {
                Id = r.Id;
                Code = r.Code;
                Name = r.Name;
                Stock = r.Stock.ToString();
                Cost = CultureInfoHelper.FormatAsCurrency(r.Cost);
                SalePrice = r.SalePrice.HasValue ? CultureInfoHelper.FormatAsCurrency(r.SalePrice.Value) : "-";
                Margin = r.MarginPercent.HasValue ? r.MarginPercent.Value.ToString("0.0") + " %" : "-";
                Iva = r.TaxAffected ? "Sí" : "No";
            }
        }

        public sealed class PriceHistoryRowVm
        {
            public string Date { get; }
            public string Event { get; }
            public string Price { get; }
            public string Cost { get; }
            public string User { get; }
            public string Reason { get; }

            public PriceHistoryRowVm(ProductPriceHistoryRow e)
            {
                Date = e.DateText;
                Event = e.EventText;
                Price = e.SalePriceText;
                Cost = e.CostText;
                User = e.UserName;
                Reason = e.Reason;
            }
        }

        #endregion
    }
}
