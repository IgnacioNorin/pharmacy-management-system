using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PharmacySystem.Presentation;

namespace PharmacySystem.Ui
{
    // "Proveedores" screen, hosted inline in MainWindow. Implements the same ISupplierView;
    // SupplierPresenter is unchanged
    // (server-paged, synchronous). Row selection loads the supplier into the form; the pager and
    // the search box drive the presenter's paging.
    public partial class SupplierView : UserControl, ISupplierView
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
        private readonly bool _canManage;
        private int _editingId;
        private int _selectedIndex;

        public SupplierView(bool canManage, Func<ISupplierView, SupplierPresenter> presenterFactory)
        {
            InitializeComponent();

            _canManage = canManage;
            _presenter = presenterFactory(this);
            UpdateActionState();

            Loaded += (s, e) => _presenter.OnLoad();
        }

        // "Agregar" with the form empty, "Guardar cambios" while a row is selected. Eliminar is
        // only reachable with a selection.
        private void UpdateActionState()
        {
            bool editing = _editingId != 0;
            btnSave.Content = editing ? "Guardar cambios" : "Agregar";
            btnSave.IsEnabled = _canManage;
            btnDelete.IsEnabled = _canManage && editing;
        }

        // The hosting window, for owning message boxes.
        private Window Host => Window.GetWindow(this)!;

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
            MessageBox.Show(Host, "¿Desea eliminar el proveedor?", "Mensaje",
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
            _editingId = 0;
            UpdateActionState();
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
            UpdateActionState();
        }

        public void ShowMessage(string message) =>
            MessageBox.Show(Host, message, "Mensaje", MessageBoxButton.OK, MessageBoxImage.Exclamation);

        public void ShowValidationErrors(IReadOnlyList<string> errors) =>
            MessageBox.Show(Host, string.Join("\n", errors), "Errores de Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);

        #endregion

        private void dgData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(dgData.SelectedItem is SupplierRowVm row))
            {
                _selectedIndex = 0;
                _editingId = 0;
                UpdateActionState();
                return;
            }

            _selectedIndex = dgData.SelectedIndex + 1;
            _editingId = row.Id;
            txtDocument.Text = row.Document;
            txtCompanyName.Text = row.CompanyName;
            txtEmail.Text = row.Email;
            txtPhone.Text = row.Phone;
            UpdateActionState();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (_editingId != 0 &&
                MessageBox.Show(Host, "¿Guardar los cambios en el proveedor seleccionado?", "Confirmar",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }
            _presenter.OnSave();
        }
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
