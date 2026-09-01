using System;
using System.Windows.Interop;
using PharmacySystem.Presentation;

namespace PharmacySystem.Ui
{
    // Entry point the WinForms shell calls while the app is part WinForms, part WPF: shows the
    // WPF change-password window modally over a WinForms owner and reports whether the password
    // was changed. Keeps all WPF interop out of the WinForms project.
    public static class ChangePasswordDialog
    {
        public static bool Show(IntPtr ownerHandle, bool mandatory,
            Func<IChangePasswordView, ChangePasswordPresenter> presenterFactory)
        {
            if (presenterFactory == null) throw new ArgumentNullException(nameof(presenterFactory));

            var window = new ChangePasswordWindow(mandatory, presenterFactory);

            if (ownerHandle != IntPtr.Zero)
            {
                new WindowInteropHelper(window) { Owner = ownerHandle };
            }

            return window.ShowDialog() == true;
        }
    }
}
