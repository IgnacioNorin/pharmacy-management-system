using System;
using PharmacySystem.Model;
using PharmacySystem.Presentation;

namespace PharmacySystem.Wpf
{
    // Keeps all WPF types out of the still-WinForms exe. Program.Main calls RunLogin() in a loop:
    // it shows the WPF login window modally and returns the resolved session, or null when the
    // user chose "Salir". EnsureApplication() creates a single WPF Application with
    // OnExplicitShutdown so closing the login window does not tear down the dispatcher that the
    // WPF dialogs opened later from MainForm rely on.
    public static class LoginHost
    {
        private static System.Windows.Application _app;

        private static void EnsureApplication()
        {
            if (_app != null || System.Windows.Application.Current != null)
            {
                return;
            }

            _app = new System.Windows.Application
            {
                ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown
            };
        }

        public static CurrentUser RunLogin(
            Func<ILoginView, LoginPresenter> presenterFactory,
            Func<Person, CurrentUser> sessionFactory,
            Func<int, IChangePasswordView, ChangePasswordPresenter> changePasswordFactory)
        {
            EnsureApplication();

            var login = new LoginWindow(presenterFactory, sessionFactory, changePasswordFactory);
            return login.ShowDialog() == true ? login.Result : null;
        }

        public static void Shutdown()
        {
            _app?.Shutdown();
            _app = null;
        }
    }
}
