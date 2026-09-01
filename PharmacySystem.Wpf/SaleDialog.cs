using System;
using System.Windows.Interop;
using PharmacySystem.Presentation;

namespace PharmacySystem.Ui
{
    // Entry point the WinForms shell calls to open the WPF "Registrar venta" screen modally over
    // a WinForms owner while the shell is still WinForms.
    public static class SaleDialog
    {
        public static void Show(IntPtr ownerHandle,
            Func<ISaleView, SalePresenter> presenterFactory,
            SaleShellHooks hooks)
        {
            if (presenterFactory == null) throw new ArgumentNullException(nameof(presenterFactory));
            if (hooks == null) throw new ArgumentNullException(nameof(hooks));

            var window = new SaleWindow(presenterFactory, hooks);

            if (ownerHandle != IntPtr.Zero)
            {
                new WindowInteropHelper(window) { Owner = ownerHandle };
            }

            window.ShowDialog();
        }
    }
}
