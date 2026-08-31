using System;
using System.ComponentModel;
using System.Windows;
using PharmacySystem.Presentation;

namespace PharmacySystem.Wpf
{
    // WPF port of ModalChangePassword. Implements the same IChangePasswordView the presenter
    // already drives, so the presenter, service and everything below are unchanged. Shown
    // modally over the WinForms shell via ChangePasswordDialog while the migration is partial.
    public partial class ChangePasswordWindow : Window, IChangePasswordView
    {
        private readonly ChangePasswordPresenter _presenter;
        private readonly bool _mandatory;
        private bool _changed;

        public ChangePasswordWindow(bool mandatory, Func<IChangePasswordView, ChangePasswordPresenter> presenterFactory)
        {
            InitializeComponent();

            _mandatory = mandatory;
            _presenter = presenterFactory(this);

            if (mandatory)
            {
                lblMandatory.Visibility = Visibility.Visible;
                btnCancel.Visibility = Visibility.Collapsed;
                btnCancel.IsCancel = false;
            }

            Loaded += (s, e) => txtCurrent.Focus();
        }

        public string CurrentPassword => txtCurrent.Password;
        public string NewPassword => txtNew.Password;
        public string ConfirmPassword => txtConfirm.Password;
        public bool Mandatory => _mandatory;

        public void ShowError(string message) =>
            MessageBox.Show(this, message, "Mensaje", MessageBoxButton.OK, MessageBoxImage.Warning);

        // IChangePasswordView.Close - not Window.Close(). Setting DialogResult closes the modal.
        public void Close(bool changed)
        {
            _changed = changed;
            DialogResult = changed;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e) => _presenter.OnSave();

        private void btnCancel_Click(object sender, RoutedEventArgs e) => _presenter.OnCancel();

        protected override void OnClosing(CancelEventArgs e)
        {
            // Block the X / Alt+F4 on the mandatory dialog until the change succeeds.
            if (_mandatory && !_changed)
            {
                e.Cancel = true;
            }

            base.OnClosing(e);
        }
    }
}
