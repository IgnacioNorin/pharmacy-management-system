using System;
using System.Windows;
using System.Windows.Interop;
using PharmacySystem.Presentation;

namespace PharmacySystem.Wpf
{
    // Entry points the shell (WinForms today, WPF later) calls to open the WPF picker windows
    // modally over an owner. Each returns the picked row, or null if the dialog was cancelled.
    public static class SupplierPickerDialog
    {
        public static SupplierRow Show(IntPtr ownerHandle, Func<ISupplierPickerView, SupplierPickerPresenter> presenterFactory)
        {
            if (presenterFactory == null) throw new ArgumentNullException(nameof(presenterFactory));

            var window = new SupplierPickerWindow(presenterFactory);
            SetOwner(window, ownerHandle);
            return window.ShowDialog() == true ? window.Picked : null;
        }

        private static void SetOwner(Window window, IntPtr ownerHandle)
        {
            if (ownerHandle != IntPtr.Zero)
            {
                new WindowInteropHelper(window) { Owner = ownerHandle };
            }
        }
    }

    public static class ProductPickerDialog
    {
        public static ProductPickerRow Show(IntPtr ownerHandle, Func<IProductPickerView, ProductPickerPresenter> presenterFactory)
        {
            if (presenterFactory == null) throw new ArgumentNullException(nameof(presenterFactory));

            var window = new ProductPickerWindow(presenterFactory);
            if (ownerHandle != IntPtr.Zero)
            {
                new WindowInteropHelper(window) { Owner = ownerHandle };
            }
            return window.ShowDialog() == true ? window.Picked : null;
        }
    }

    public static class ClientPickerDialog
    {
        public static ClientRow Show(IntPtr ownerHandle, Func<IClientPickerView, ClientPickerPresenter> presenterFactory)
        {
            if (presenterFactory == null) throw new ArgumentNullException(nameof(presenterFactory));

            var window = new ClientPickerWindow(presenterFactory);
            if (ownerHandle != IntPtr.Zero)
            {
                new WindowInteropHelper(window) { Owner = ownerHandle };
            }
            return window.ShowDialog() == true ? window.Picked : null;
        }
    }
}
