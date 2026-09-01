using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PharmacySystem.Presentation;

namespace PharmacySystem.Ui
{
    // WPF port of frmSupplier. Implements the same ISupplierView; SupplierPresenter is unchanged
    // (server-paged, synchronous). Row selection loads the supplier into the form; the pager and
    // the search box drive the presenter's paging.
    public partial class SupplierWindow : Window, ISupplierView
    {
        private sealed class SupplierRowVm
        {
            public int Id { get; set; }
            public string Document { get; set; } = string.Empty;
            public string CompanyName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
        }

        private readonly SupplierPresenter _presenter;
        private int _editingId;
        private int _selectedIndex;

        public SupplierWindow(bool canManage, Func<ISupplierView, SupplierPresenter> presenterFactory)
        {
            InitializeComponent();

            _presenter = presenterFactory(this);

            btnSave.IsEnabled = canManage;
            btnDelete.IsEnabled = canManage;

            Loaded += (s, e) => _presenter.OnLoad();
        }

        #region ISupplierView

        public int SelectedIndex => _selectedIndex;
        public int RowCount => (dgData.ItemsSource as IEnumerable<object>)?.Count() ?? 0;
        public int SupplierId => _editingId;
        public string Document => txtDocument.Text;
        public string CompanyName => txtCompanyName.Text;
        public string Email => txtEmail.Text;
        public string Phone => txtPhone.Text;
        public string SearchText => txtSearch.Text;

        // Mirrors frmSupplier's rule set (Validations.cs): document not empty + valid format;
        // company name not empty; email not empty + valid; phone not empty + valid.
        public List<string> Validate()
        {
            var errors = new List<string>();
            FieldRules.Check(errors, "Número Documento", txtDocument.Text.Trim(), "NotEmpty", "ValidateDocument");
            FieldRules.Check(errors, "Razón Social", txtCompanyName.Text.Trim(), "NotEmpty");
            FieldRules.Check(errors, "Correo", txtEmail.Text.Trim(), "NotEmpty", "ValidateEmail");
            FieldRules.Check(errors, "Teléfono", txtPhone.Text.Trim(), "NotEmpty", "OnlyNumbers");
            return errors;
        }

        public bool ConfirmDelete() =>
            MessageBox.Show(this, "¿Desea eliminar el proveedor?", "Mensaje",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

        public void LoadSuppliers(IEnumerable<SupplierRow> suppliers)
        {
            dgData.SelectionChanged -= dgData_SelectionChanged;
            dgData.ItemsSource = suppliers.Select(s => new SupplierRowVm
            {
                Id = s.Id,
                Document = s.Document,
                CompanyName = s.CompanyName,
                Email = s.Email,
                Phone = s.Phone
            }).ToList();
            dgData.SelectedIndex = -1;
            dgData.SelectionChanged += dgData_SelectionChanged;
        }

        public void SetPageInfo(int currentPage, int totalPages, int totalCount)
        {
            lblPage.Text = totalCount == 0
                ? "Sin resultados"
                : $"Página {currentPage} de {totalPages}  ·  {totalCount} proveedor(es)";

            btnFirst.IsEnabled = btnPrev.IsEnabled = currentPage > 1;
            btnNext.IsEnabled = btnLast.IsEnabled = currentPage < totalPages;
        }

        public void ClearForm()
        {
            _editingId = 0;
            _selectedIndex = 0;
            txtDocument.Clear();
            txtCompanyName.Clear();
            txtEmail.Clear();
            txtPhone.Clear();

            dgData.SelectionChanged -= dgData_SelectionChanged;
            dgData.SelectedIndex = -1;
            dgData.SelectionChanged += dgData_SelectionChanged;
        }

        public void ShowMessage(string message) =>
            MessageBox.Show(this, message, "Mensaje", MessageBoxButton.OK, MessageBoxImage.Exclamation);

        public void ShowValidationErrors(IReadOnlyList<string> errors) =>
            MessageBox.Show(this, string.Join("\n", errors), "Errores de Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);

        #endregion

        private void dgData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(dgData.SelectedItem is SupplierRowVm row))
            {
                _selectedIndex = 0;
                return;
            }

            _selectedIndex = dgData.SelectedIndex + 1;
            _editingId = row.Id;
            txtDocument.Text = row.Document;
            txtCompanyName.Text = row.CompanyName;
            txtEmail.Text = row.Email;
            txtPhone.Text = row.Phone;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e) => _presenter.OnSave();
        private void btnDelete_Click(object sender, RoutedEventArgs e) => _presenter.OnDelete();
        private void btnClearForm_Click(object sender, RoutedEventArgs e) => ClearForm();
        private void btnSearch_Click(object sender, RoutedEventArgs e) => _presenter.OnSearch();

        private void btnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
            _presenter.OnSearch();
        }

        private void btnFirst_Click(object sender, RoutedEventArgs e) => _presenter.OnFirstPage();
        private void btnPrev_Click(object sender, RoutedEventArgs e) => _presenter.OnPreviousPage();
        private void btnNext_Click(object sender, RoutedEventArgs e) => _presenter.OnNextPage();
        private void btnLast_Click(object sender, RoutedEventArgs e) => _presenter.OnLastPage();
    }
}
