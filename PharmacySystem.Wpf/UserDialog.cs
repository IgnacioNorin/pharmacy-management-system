using System;
using System.Windows.Interop;
using PharmacySystem.Presentation;

namespace PharmacySystem.Ui
{
    public static class UserDialog
    {
        public static void Show(IntPtr ownerHandle, bool canManage, Func<IUserView, UserPresenter> presenterFactory)
        {
            if (presenterFactory == null) throw new ArgumentNullException(nameof(presenterFactory));

            var window = new UserWindow(canManage, presenterFactory);
            if (ownerHandle != IntPtr.Zero)
            {
                new WindowInteropHelper(window) { Owner = ownerHandle };
            }
            window.ShowDialog();
        }
    }
}
