using System;
using System.Windows;
using PharmacySystem.Model;
using PharmacySystem.Presentation;

namespace PharmacySystem.Wpf
{
    // WPF port of Login. Implements the same ILoginView; LoginPresenter is unchanged. Shown
    // modally by Program.Main: on a successful login it exposes the resolved CurrentUser in
    // Result and closes with DialogResult = true, and Program launches the (still WinForms)
    // MainForm. "Salir" closes with DialogResult = false and the app exits.
    public partial class LoginWindow : Window, ILoginView
    {
        private readonly LoginPresenter _presenter;
        private readonly Func<Person, CurrentUser> _sessionFactory;
        private readonly Func<int, IChangePasswordView, ChangePasswordPresenter> _changePasswordFactory;

        // The signed-in user's session; null until a login succeeds.
        public CurrentUser? Result { get; private set; }

        public LoginWindow(
            Func<ILoginView, LoginPresenter> presenterFactory,
            Func<Person, CurrentUser> sessionFactory,
            Func<int, IChangePasswordView, ChangePasswordPresenter> changePasswordFactory)
        {
            InitializeComponent();

            _sessionFactory = sessionFactory;
            _changePasswordFactory = changePasswordFactory;
            _presenter = presenterFactory(this);

            Loaded += (s, e) => txtDocument.Focus();
        }

        string ILoginView.Document => txtDocument.Text;
        string ILoginView.Password => txtPassword.Password;

        public void LoginSucceeded(Person person)
        {
            // Resolve the permission set from the user's role once, here, and hand it to Program
            // which passes it to MainForm.
            Result = _sessionFactory(person);
            DialogResult = true;
        }

        public void RequirePasswordChange(Person person)
        {
            var dialog = new ChangePasswordWindow(
                mandatory: true,
                presenterFactory: v => _changePasswordFactory(person.idPerson, v))
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true)
            {
                LoginSucceeded(person);
            }
            else
            {
                txtPassword.Clear();
                txtPassword.Focus();
            }
        }

        public void ShowError(string message) =>
            MessageBox.Show(this, message, "Mensaje", MessageBoxButton.OK, MessageBoxImage.Exclamation);

        private void btnEnter_Click(object sender, RoutedEventArgs e) => _presenter.OnLogin();

        private void btnExit_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
