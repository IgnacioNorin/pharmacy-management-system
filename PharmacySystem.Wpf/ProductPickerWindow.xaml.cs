using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using PharmacySystem.Presentation;

namespace PharmacySystem.Ui
{
    // WPF port of ModalProduct. Read-only picker: implements IProductPickerView (initial load
    // only); the origin ("frmSale" / "frmPurchase") that decides which products show is baked
    // into the presenter by the caller. Filtering and selection stay in the view.
    public partial class ProductPickerWindow : Window, IProductPickerView
    {
        private readonly ProductPickerPresenter _presenter;
        private List<ProductPickerRow> _all = new List<ProductPickerRow>();

        public ProductPickerRow? Picked { get; private set; }

        public ProductPickerWindow(Func<IProductPickerView, ProductPickerPresenter> presenterFactory)
        {
            InitializeComponent();
            _presenter = presenterFactory(this);
            Loaded += (s, e) => _presenter.OnLoad();
        }

        public void LoadProducts(IEnumerable<ProductPickerRow> products)
        {
            _all = products.ToList();
            dgProducts.ItemsSource = _all;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string term = txtSearch.Text.Trim();
            dgProducts.ItemsSource = string.IsNullOrEmpty(term)
                ? _all
                : _all.Where(p => Contains(p.Code, term) || Contains(p.Name, term)
                                  || Contains(p.Description, term) || Contains(p.CategoryDescription, term)).ToList();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            dgProducts.ItemsSource = _all;
        }

        private static bool Contains(string value, string term) =>
            (value ?? "").IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;

        private void dgProducts_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) => SelectCurrent();
        private void btnSelect_Click(object sender, RoutedEventArgs e) => SelectCurrent();

        private void SelectCurrent()
        {
            if (dgProducts.SelectedItem is ProductPickerRow row)
            {
                Picked = row;
                DialogResult = true;
            }
        }
    }
}
