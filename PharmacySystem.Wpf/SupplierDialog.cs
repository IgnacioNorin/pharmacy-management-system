using System;
using System.Windows.Interop;
using PharmacySystem.Presentation;

namespace PharmacySystem.Wpf
{
    // Entry point the WinForms shell calls to open the WPF "Proveedores" screen modally over a
    // WinForms owner while the shell is still WinForms.
    public static class SupplierDialog
    {
        public static void Show(IntPtr ownerHandle, bool canManage,
            Func<ISupplierView, SupplierPresenter> presenterFactory)
        {
            if (presenterFactory == null) throw new ArgumentNullException(nameof(presenterFactory));

            var window = new SupplierWindow(canManage, presenterFactory);
            if (ownerHandle != IntPtr.Zero) new WindowInteropHelper(window) { Owner = ownerHandle };
            window.ShowDialog();
        }
    }
}
