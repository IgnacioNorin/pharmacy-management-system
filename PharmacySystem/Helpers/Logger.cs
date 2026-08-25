using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace PharmacySystem.Helpers
{
    public static class Logger
    {
        private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
        private static readonly object LockObj = new object();

        public static void LogError(Exception ex, [CallerMemberName] string memberName = "")
        {
            string entry = string.Format(
                "[{0:yyyy-MM-dd HH:mm:ss}] {1}: {2}{3}{4}",
                DateTime.Now, memberName, ex, Environment.NewLine, new string('-', 80) + Environment.NewLine);

            lock (LockObj)
            {
                try
                {
                    File.AppendAllText(LogFilePath, entry);
                }
                catch
                {
                    // Logging must never throw and mask the original error.
                }
            }
        }
    }
}
