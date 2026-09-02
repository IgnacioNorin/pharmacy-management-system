using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using PharmacySystem.Presentation;
using PharmacySystem.Validators;
using ComboBoxItem = PharmacySystem.Model.ComboBoxItem;

namespace PharmacySystem.Ui
{
    // WPF port of frmUser. Implements the same IUserView; UserPresenter is unchanged. The grid is
    // filtered through the default collection view so _rows stays intact and the presenter's
    // index-based ReplaceRow / RemoveRow keep working while a search filter is active.
    public partial class UserView : UserControl, IUserView
    {
        private readonly UserPresenter _presenter;
        private readonly ObservableCollection<UserRow> _rows = new ObservableCollection<UserRow>();
        private readonly bool _canManage;

        private int _userId;

        public UserView(bool canManage, Func<IUserView, UserPresenter> presenterFactory)
        {
            InitializeComponent();

            _canManage = canManage;
            dgUsers.ItemsSource = _rows;

            _presenter = presenterFactory(this);
            UpdateActionState();
            Loaded += (s, e) => _presenter.OnLoad();
        }

        // "Agregar" with the form empty, "Guardar cambios" while a row is selected. Eliminar is
        // only reachable with a selection.
        private void UpdateActionState()
        {
            bool editing = _userId != 0;
            btnSave.Content = editing ? "Guardar cambios" : "Agregar";
            btnSave.IsEnabled = _canManage;
            btnDelete.IsEnabled = _canManage && editing;
        }

        // The hosting window, for owning message boxes and the "Acciones" dialog.
        private Window Host => Window.GetWindow(this)!;

        #region IUserView

        public int SelectedIndex => dgUsers.SelectedItem is UserRow r ? _rows.IndexOf(r) + 1 : 0;
        public int RowCount => _rows.Count;
        public int UserId => _userId;
        public string Document => txtDocument.Text;
        string IUserView.Name => txtName.Text;
        public string Password => txtPassword.Password;
        public string ConfirmPassword => txtConfirm.Password;
        public int RoleId => (cboRole.SelectedItem as ComboBoxItem)?.Value is object v && int.TryParse(v.ToString(), out int id) ? id : 0;
        public string RoleText => (cboRole.SelectedItem as ComboBoxItem)?.Text ?? "";

        public List<string> Validate()
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(txtDocument.Text))
                errors.Add("Numero Documento : Este campo no puede estar vacío");
            else if (!DocumentValidator.IsValid(txtDocument.Text))
                errors.Add("Numero Documento : Documento inválido: use entre 3 y 20 letras, números, punto o guion");
            if (string.IsNullOrWhiteSpace(txtName.Text))
                errors.Add("Nombre Completo : Este campo no puede estar vacío");
            return errors;
        }

        public bool ConfirmDelete() =>
            MessageBox.Show(Host, "¿Desea eliminar el usuario?", "Mensaje",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

        public void LoadRoleOptions(IEnumerable<ComboBoxItem> options)
        {
            cboRole.ItemsSource = options.ToList();
            if (cboRole.Items.Count > 0) cboRole.SelectedIndex = 0;
        }

        public void LoadUsers(IEnumerable<UserRow> users)
        {
            _rows.Clear();
            foreach (UserRow row in users) _rows.Add(row);
            _userId = 0;
            UpdateActionState();
        }

        public void AddRow(UserRow row) => _rows.Add(row);

        public void ReplaceRow(int index, UserRow row)
        {
            if (index >= 0 && index < _rows.Count) _rows[index] = row;
        }

        public void RemoveRow(int index)
        {
            if (index >= 0 && index < _rows.Count) _rows.RemoveAt(index);
        }

        public void ClearForm()
        {
            _userId = 0;
            txtDocument.Clear();
            txtName.Clear();
            txtPassword.Clear();
            txtConfirm.Clear();
            if (cboRole.Items.Count > 0) cboRole.SelectedIndex = 0;
            dgUsers.SelectedIndex = -1;
            UpdateActionState();
        }

        public void ShowMessage(string message) =>
            MessageBox.Show(Host, message, "Mensaje", MessageBoxButton.OK, MessageBoxImage.Exclamation);

        public void ShowValidationErrors(IReadOnlyList<string> errors) =>
            MessageBox.Show(Host, string.Join("\n", errors), "Errores de validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);

        public void ShowPasswordMismatch() =>
            MessageBox.Show(Host, "Las contraseñas no coinciden\nRevise nuevamente", "Mensaje",
                MessageBoxButton.OK, MessageBoxImage.Exclamation);

        public void ShowTemporaryPassword(string tempPassword)
        {
            try { Clipboard.SetText(tempPassword); } catch { /* clipboard may be unavailable */ }
            MessageBox.Show(Host,
                $"Contraseña temporal: {tempPassword}\n\nYa se copió al portapapeles. Comuníquesela al usuario: " +
                "deberá cambiarla al iniciar sesión.",
                "Contraseña restablecida", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        private void dgUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(dgUsers.SelectedItem is UserRow row))
            {
                _userId = 0;
                UpdateActionState();
                return;
            }

            _userId = row.Id;
            txtDocument.Text = row.Document ?? "";
            txtName.Text = row.Name ?? "";
            txtPassword.Clear();
            txtConfirm.Clear();

            foreach (ComboBoxItem item in cboRole.Items)
            {
                if (item.Text == row.RoleText) { cboRole.SelectedItem = item; break; }
            }

            UpdateActionState();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string term = txtSearch.Text.Trim();
            ICollectionView view = CollectionViewSource.GetDefaultView(_rows);
            view.Filter = string.IsNullOrEmpty(term)
                ? (Predicate<object>?)null
                : o => o is UserRow r &&
                       ((r.Document ?? "").IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0
                        || (r.Name ?? "").IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0
                        || (r.RoleText ?? "").IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0
                        || (r.StatusText ?? "").IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void btnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            CollectionViewSource.GetDefaultView(_rows).Filter = null;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (_userId != 0 &&
                MessageBox.Show(Host, "¿Guardar los cambios en el usuario seleccionado?", "Confirmar",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }
            _presenter.OnSave();
        }

        private void btnClearForm_Click(object sender, RoutedEventArgs e) => ClearForm();
        private void btnDelete_Click(object sender, RoutedEventArgs e) => _presenter.OnDelete();

        private void btnActions_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as FrameworkElement)?.DataContext is UserRow row)) return;

            dgUsers.SelectedItem = row; // so the presenter's SelectedIndex / UserId point at it

            var dialog = new UserActionsWindow(row.Name ?? "", row.StatusText ?? "", row.StatusText != "Inactivo") { Owner = Host };
            if (dialog.ShowDialog() != true) return;

            switch (dialog.SelectedAction)
            {
                case UserAction.ResetPassword: _presenter.OnResetPassword(); break;
                case UserAction.Unlock: _presenter.OnUnlockUser(); break;
                case UserAction.ToggleActive: _presenter.OnSuspendUser(); break;
            }
        }
    }
}
