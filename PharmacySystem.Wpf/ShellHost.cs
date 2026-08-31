using PharmacySystem.Presentation;

namespace PharmacySystem.Wpf
{
    // Keeps the WPF shell out of the still-WinForms exe. Program.Main calls RunShell() after a
    // successful login; it shows MainWindow and blocks until the user closes it, then Program's
    // loop returns to the login screen.
    public static class ShellHost
    {
        public static void RunShell(CurrentUser session, ShellServices services)
        {
            new MainWindow(session, services).ShowDialog();
        }
    }
}
