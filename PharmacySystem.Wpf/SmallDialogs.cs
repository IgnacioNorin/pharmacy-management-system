using System;
using System.Windows.Interop;
using PharmacySystem.Presentation;

namespace PharmacySystem.Wpf
{
    public static class CashCountDialog
    {
        public static void Show(IntPtr ownerHandle, Func<ICashCountView, CashCountPresenter> presenterFactory)
        {
            var window = new CashCountWindow(presenterFactory);
            if (ownerHandle != IntPtr.Zero) new WindowInteropHelper(window) { Owner = ownerHandle };
            window.ShowDialog();
        }
    }

    public static class NotificationConfigDialog
    {
        public static void Show(IntPtr ownerHandle, bool canConfigure,
            Func<INotificationConfigView, NotificationConfigPresenter> presenterFactory)
        {
            var window = new NotificationConfigWindow(canConfigure, presenterFactory);
            if (ownerHandle != IntPtr.Zero) new WindowInteropHelper(window) { Owner = ownerHandle };
            window.ShowDialog();
        }
    }
}
