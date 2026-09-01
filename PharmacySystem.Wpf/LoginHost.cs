using System;
using System.Windows.Media;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using Wpf.Ui.Appearance;

namespace PharmacySystem.Ui
{
    // Keeps all WPF types out of the still-WinForms exe. Program.Main calls RunLogin() in a loop:
    // it shows the WPF login window modally and returns the resolved session, or null when the
    // user chose "Salir". EnsureApplication() creates a single WPF Application with
    // OnExplicitShutdown so closing the login window does not tear down the dispatcher that the
    // WPF dialogs opened later from MainForm rely on.
    public static class LoginHost
    {
        private static System.Windows.Application? _app;

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

            ApplyTheme(_app);
        }

        // Brand accent. WPF-UI derives the primary / secondary / tertiary shades from this.
        private static readonly Color BrandAccent = Color.FromRgb(0x1E, 0x6F, 0xB8);

        // Merges the Fluent design system and the app's design tokens into the Application's
        // resources. Done here (not in an App.xaml, which this project does not have) so every
        // window created afterwards picks up the theme. Best-effort: a theming failure must not
        // stop the app from starting.
        private static void ApplyTheme(System.Windows.Application app)
        {
            try
            {
                // Order matters: Fluent theme, then Fluent controls, then the app's own tokens.
                app.Resources.MergedDictionaries.Add(
                    new Wpf.Ui.Markup.ThemesDictionary { Theme = ApplicationTheme.Light });
                app.Resources.MergedDictionaries.Add(new Wpf.Ui.Markup.ControlsDictionary());
                app.Resources.MergedDictionaries.Add(new System.Windows.ResourceDictionary
                {
                    Source = new Uri(
                        "pack://application:,,,/PharmacySystem.Wpf;component/Themes/AppResources.xaml",
                        UriKind.Absolute)
                });

                ApplicationThemeManager.Apply(
                    ApplicationTheme.Light, global::Wpf.Ui.Controls.WindowBackdropType.Mica, updateAccent: true);
                ApplicationAccentColorManager.Apply(
                    BrandAccent, ApplicationTheme.Light, systemGlassColor: false, systemAccentColor: false);
            }
            catch
            {
                // Fall back to the platform default look.
            }
        }

        public static CurrentUser? RunLogin(
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
