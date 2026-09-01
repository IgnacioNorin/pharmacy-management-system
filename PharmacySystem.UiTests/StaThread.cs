using System;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace PharmacySystem.UiTests
{
    // xUnit runs tests on threadpool (MTA) threads, but WinForms controls generally assume an STA
    // apartment (Clipboard, drag-drop, some ActiveX-backed controls like WebBrowser). Running each
    // Form construction on its own STA thread avoids relying on incidental MTA-compatibility.
    internal static class StaThread
    {
        public static void Run(Action action)
        {
            ExceptionDispatchInfo? capturedException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    capturedException = ExceptionDispatchInfo.Capture(ex);
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            capturedException?.Throw();
        }
    }
}
