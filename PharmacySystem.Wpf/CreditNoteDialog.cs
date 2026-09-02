using System;
using System.Windows.Interop;
using PharmacySystem.Presentation;

namespace PharmacySystem.Ui
{
    // Entry point the WinForms shell (frmSale) calls to open the WPF "Nota de crédito" window
    // modally over a WinForms owner while the shell is still WinForms.
    public static class CreditNoteDialog
    {
        public static void Show(IntPtr ownerHandle, Func<ICreditNoteView, CreditNotePresenter> presenterFactory)
        {
            if (presenterFactory == null) throw new ArgumentNullException(nameof(presenterFactory));

            var window = new CreditNoteWindow(presenterFactory);

            if (ownerHandle != IntPtr.Zero)
            {
                new WindowInteropHelper(window) { Owner = ownerHandle };
            }

            window.ShowDialog();
        }
    }
}
