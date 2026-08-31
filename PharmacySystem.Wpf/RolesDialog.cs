using System;
using System.Windows.Interop;
using PharmacySystem.Presentation;

namespace PharmacySystem.Wpf
{
    public static class RolesDialog
    {
        public static void Show(IntPtr ownerHandle, Func<IRolesView, RolesPresenter> presenterFactory)
        {
            if (presenterFactory == null) throw new ArgumentNullException(nameof(presenterFactory));

            var window = new RolesWindow(presenterFactory);
            if (ownerHandle != IntPtr.Zero)
            {
                new WindowInteropHelper(window) { Owner = ownerHandle };
            }
            window.ShowDialog();
        }
    }
}
