using System;
using System.Windows.Interop;
using PharmacySystem.Presentation;

namespace PharmacySystem.Wpf
{
    // Entry point the WinForms shell calls to open the WPF "Registrar compra" screen modally
    // over a WinForms owner while the shell is still WinForms.
    public static class PurchaseDialog
    {
        public static void Show(IntPtr ownerHandle,
            Func<IPurchaseView, PurchasePresenter> presenterFactory,
            PickerFactories pickers)
        {
            if (presenterFactory == null) throw new ArgumentNullException(nameof(presenterFactory));
            if (pickers == null) throw new ArgumentNullException(nameof(pickers));

            var window = new PurchaseWindow(presenterFactory, pickers);

            if (ownerHandle != IntPtr.Zero)
            {
                new WindowInteropHelper(window) { Owner = ownerHandle };
            }

            window.ShowDialog();
        }
    }
}
