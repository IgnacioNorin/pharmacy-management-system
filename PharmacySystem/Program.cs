using System;
using System.Windows.Forms;
using PharmacySystem.Helpers;
using PharmacySystem.Presentation;
using PharmacySystem.Wpf;

namespace PharmacySystem
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Send WinForms UI-thread exceptions to our handler instead of the default crash dialog,
            // and catch everything else through the AppDomain hook.
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (sender, e) => Report(e.Exception, fatal: false);
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => Report(e.ExceptionObject as Exception, fatal: true);

            try
            {
                // PrintSale is still a WinForms dialog; hand the shell a callback to open it.
                var shellServices = CompositionRoot.CreateShellServices(
                    idSale => { using (var print = new PrintSale(idSale)) print.ShowDialog(); });

                // Log in, run the shell, and when the shell closes come back to the login screen
                // (log in as someone else without restarting). "Salir" on the login screen ends
                // the loop and the app. LoginHost/ShellHost keep the WPF types in PharmacySystem.Wpf.
                while (true)
                {
                    CurrentUser session = LoginHost.RunLogin(
                        CompositionRoot.CreateLoginPresenter,
                        CompositionRoot.CreateCurrentUser,
                        (personId, view) => CompositionRoot.CreateChangePasswordPresenter(view, personId));

                    if (session == null)
                    {
                        break;
                    }

                    ShellHost.RunShell(session, shellServices);
                }
            }
            catch (Exception ex)
            {
                // A failure while building the first screen lands here, before the message loop
                // starts: e.g. a missing ConnectionStrings.config makes CompositionRoot's static
                // initializer throw. Without this the app would die with the bare .NET dialog.
                Report(ex, fatal: true);
            }

            LoginHost.Shutdown();
        }

        // Logs every unhandled exception and shows the user a message they can act on. A database
        // or configuration problem is always terminal (it will not fix itself mid-session); any
        // other UI-thread error is reported and the app keeps running.
        private static void Report(Exception ex, bool fatal)
        {
            if (ex == null)
            {
                return;
            }

            Logger.LogError(ex);

            // A lost database connection mid-session (DataUnavailableException from a repository) is
            // recoverable: report it and let the user retry. Only a startup/AppDomain failure or a
            // real configuration problem is terminal.
            bool terminal = fatal ||
                (StartupError.IsDatabaseOrConfig(ex) && !StartupError.IsTransientDataFailure(ex));
            string message = StartupError.DescribeForUser(ex);
            if (!terminal)
            {
                message += "\n\nPuede intentar la operacion de nuevo.";
            }

            try
            {
                MessageBox.Show(
                    message,
                    terminal ? "PharmacySystem no puede continuar" : "PharmacySystem",
                    MessageBoxButtons.OK,
                    terminal ? MessageBoxIcon.Error : MessageBoxIcon.Warning);
            }
            catch
            {
                // No UI available on a very early failure: the log entry above is the record.
            }

            if (terminal)
            {
                Environment.Exit(1);
            }
        }
    }
}
