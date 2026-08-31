using System;
using System.Collections.Generic;
using System.Windows.Interop;
using PharmacySystem.Business;
using PharmacySystem.Model;
using PharmacySystem.Presentation;

namespace PharmacySystem.Wpf
{
    public static class AlertsDialog
    {
        // Opens the notification center modally over a WinForms owner and returns the product code
        // the user clicked "Ver" on, or null if the window was just closed.
        public static string Show(
            IntPtr ownerHandle,
            IReadOnlyList<ProductAlert> alerts,
            INotificationConfigService notificationService,
            int currentPersonId,
            bool canAcknowledge,
            bool canMute,
            bool canConfigure,
            Func<INotificationConfigView, NotificationConfigPresenter> configPresenterFactory)
        {
            var window = new AlertsWindow(alerts, notificationService, currentPersonId,
                canAcknowledge, canMute, canConfigure, configPresenterFactory);
            if (ownerHandle != IntPtr.Zero) new WindowInteropHelper(window) { Owner = ownerHandle };

            bool picked = window.ShowDialog() == true;
            return picked ? window.SelectedProductCode : null;
        }
    }

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
