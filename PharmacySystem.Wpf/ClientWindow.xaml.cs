using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PharmacySystem.Presentation;
using PharmacySystem.Validators;

namespace PharmacySystem.Ui
{
    // WPF port of frmClient. Implements the same IClientView; ClientPresenter is unchanged
    // (synchronous - the page query is 50 rows). Row selection loads the client into the form;
    // the pager and the search box drive the presenter's paging.
    public partial class ClientWindow : Window, IClientView
    {
        private readonly ClientPresenter _presenter;
        private int _editingId;

        public ClientWindow(bool canManage, Func<IClientView, ClientPresenter> presenterFactory)
        {
            InitializeComponent();

            _presenter = presenterFactory(this);

            btnSave.IsEnabled = canManage;
            btnDelete.IsEnabled = canManage;

            Loaded += (s, e) => _presenter.OnLoad();
        }

        #region IClientView

        public int SelectedIndex => dgClients.SelectedItem != null ? dgClients.SelectedIndex + 1 : 0;
        public int PersonId => _editingId;
        public string Document => txtDocument.Text;
        string IClientView.Name => txtName.Text;
        public string Address => txtAddress.Text;
        public string Phone => txtPhone.Text;
        public string BusinessName => txtBusinessName.Text;
        public string Activity => txtActivity.Text;
        public string Commune => txtCommune.Text;
        public string Email => txtEmail.Text;
        public bool IsCompany => chkIsCompany.IsChecked == true;
        public string SearchText => txtSearch.Text;

        // Mirrors frmClient's rule set (Validations.cs): document not empty + valid format;
        // name / address / phone not empty. Reimplemented here so the WPF project does not
        // depend on the WinForms project.
        public List<string> Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(txtDocument.Text))
                errors.Add("Numero Documento : Este campo no puede estar vacío");
            else if (!DocumentValidator.IsValid(txtDocument.Text))
                errors.Add("Numero Documento : Documento inválido: use entre 3 y 20 letras, números, punto o guion");

            if (string.IsNullOrWhiteSpace(txtName.Text))
                errors.Add("Nombre Completo : Este campo no puede estar vacío");
            if (string.IsNullOrWhiteSpace(txtAddress.Text))
                errors.Add("Dirección : Este campo no puede estar vacío");
            if (string.IsNullOrWhiteSpace(txtPhone.Text))
                errors.Add("Teléfono : Este campo no puede estar vacío");

            return errors;
        }

        public bool ConfirmDelete() =>
            MessageBox.Show(this, "¿Desea eliminar el cliente?", "Mensaje",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

        public void LoadClients(IEnumerable<ClientRow> clients)
        {
            dgClients.SelectionChanged -= dgClients_SelectionChanged;
            dgClients.ItemsSource = clients.ToList();
            dgClients.SelectedIndex = -1;
            dgClients.SelectionChanged += dgClients_SelectionChanged;
        }

        public void SetPageInfo(int currentPage, int totalPages, int totalCount)
        {
            lblPage.Text = totalCount == 0
                ? "Sin resultados"
                : $"Página {currentPage} de {totalPages}  ·  {totalCount} cliente(s)";

            btnFirst.IsEnabled = btnPrev.IsEnabled = currentPage > 1;
            btnNext.IsEnabled = btnLast.IsEnabled = currentPage < totalPages;
        }

        public void ClearForm()
        {
            _editingId = 0;
            txtDocument.Clear();
            txtName.Clear();
            txtAddress.Clear();
            txtPhone.Clear();
            txtBusinessName.Clear();
            txtActivity.Clear();
            txtCommune.Clear();
            txtEmail.Clear();
            chkIsCompany.IsChecked = false;

            dgClients.SelectionChanged -= dgClients_SelectionChanged;
            dgClients.SelectedIndex = -1;
            dgClients.SelectionChanged += dgClients_SelectionChanged;
        }

        public void ShowMessage(string message) =>
            MessageBox.Show(this, message, "Mensaje", MessageBoxButton.OK, MessageBoxImage.Exclamation);

        public void ShowValidationErrors(IReadOnlyList<string> errors) =>
            MessageBox.Show(this, string.Join("\n", errors), "Errores de validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);

        #endregion

        private void dgClients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(dgClients.SelectedItem is ClientRow row))
            {
                return;
            }

            _editingId = row.Id;
            txtDocument.Text = row.Document ?? "";
            txtName.Text = row.Name ?? "";
            txtAddress.Text = row.Address ?? "";
            txtPhone.Text = row.Phone ?? "";
            txtBusinessName.Text = row.BusinessName ?? "";
            txtActivity.Text = row.Activity ?? "";
            txtCommune.Text = row.Commune ?? "";
            txtEmail.Text = row.Email ?? "";
            chkIsCompany.IsChecked = row.IsCompany;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e) => _presenter.OnSearch();

        private void btnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            _presenter.OnSearch();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e) => _presenter.OnSave();
        private void btnClearForm_Click(object sender, RoutedEventArgs e) => ClearForm();
        private void btnDelete_Click(object sender, RoutedEventArgs e) => _presenter.OnDelete();

        private void btnFirst_Click(object sender, RoutedEventArgs e) => _presenter.OnFirstPage();
        private void btnPrev_Click(object sender, RoutedEventArgs e) => _presenter.OnPreviousPage();
        private void btnNext_Click(object sender, RoutedEventArgs e) => _presenter.OnNextPage();
        private void btnLast_Click(object sender, RoutedEventArgs e) => _presenter.OnLastPage();
    }
}
