using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using PharmacySystem.Model;
using PharmacySystem.Presentation;

namespace PharmacySystem.Wpf
{
    // WPF port of ModalSupplier. Read-only picker: implements ISupplierPickerView (just the
    // initial load); filtering and selection are view concerns, as in the original.
    public partial class SupplierPickerWindow : Window, ISupplierPickerView
    {
        private readonly SupplierPickerPresenter _presenter;
        private List<SupplierRow> _all = new List<SupplierRow>();

        public SupplierRow Picked { get; private set; }

        public SupplierPickerWindow(Func<ISupplierPickerView, SupplierPickerPresenter> presenterFactory)
        {
            InitializeComponent();
            _presenter = presenterFactory(this);
            Loaded += (s, e) => _presenter.OnLoad();
        }

        public void LoadSuppliers(IEnumerable<SupplierRow> suppliers)
        {
            _all = suppliers.ToList();
            dgSuppliers.ItemsSource = _all;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string term = txtSearch.Text.Trim();
            dgSuppliers.ItemsSource = string.IsNullOrEmpty(term)
                ? _all
                : _all.Where(s => Contains(s.Document, term) || Contains(s.CompanyName, term)
                                  || Contains(s.Email, term) || Contains(s.Phone, term)).ToList();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            dgSuppliers.ItemsSource = _all;
        }

        private static bool Contains(string value, string term) =>
            (value ?? "").IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;

        private void dgSuppliers_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) => SelectCurrent();
        private void btnSelect_Click(object sender, RoutedEventArgs e) => SelectCurrent();

        private void SelectCurrent()
        {
            if (dgSuppliers.SelectedItem is SupplierRow row)
            {
                Picked = row;
                DialogResult = true;
            }
        }
    }
}
