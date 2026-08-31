using System;
using System.Windows.Interop;
using PharmacySystem.Presentation;

namespace PharmacySystem.Wpf
{
    // Entry point the WinForms shell calls to open the WPF "Bitácora" window modally over a
    // WinForms owner. Keeps the WPF interop out of the WinForms project.
    public static class SecurityLogDialog
    {
        public static void Show(IntPtr ownerHandle, Func<ISecurityLogView, SecurityLogPresenter> presenterFactory)
        {
            if (presenterFactory == null) throw new ArgumentNullException(nameof(presenterFactory));

            var window = new SecurityLogWindow(presenterFactory);

            if (ownerHandle != IntPtr.Zero)
            {
                new WindowInteropHelper(window) { Owner = ownerHandle };
            }

            window.ShowDialog();
        }
    }
}
