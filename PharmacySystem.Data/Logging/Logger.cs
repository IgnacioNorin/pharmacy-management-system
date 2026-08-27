using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace PharmacySystem.Helpers
{
    public static class Logger
    {
        private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
        private const long MaxLogBytes = 5 * 1024 * 1024;

        // Named mutex, not a plain lock object: two copies of the app can share the same install
        // folder, so the guard has to be cross-process or the writes collide and log lines are
        // lost. Session-local name (no "Global\") so construction never needs elevated rights.
        private static readonly Mutex FileMutex = new Mutex(false, "PharmacySystem.ErrorLog");

        public static void LogError(Exception ex, [CallerMemberName] string memberName = "")
        {
            string entry = string.Format(
                "[{0:yyyy-MM-dd HH:mm:ss}] {1}: {2}{3}{4}",
                DateTime.Now, memberName, ex, Environment.NewLine, new string('-', 80) + Environment.NewLine);

            bool acquired = false;
            try
            {
                try { acquired = FileMutex.WaitOne(TimeSpan.FromSeconds(2)); }
                catch (AbandonedMutexException) { acquired = true; }

                RotateIfNeeded();
                File.AppendAllText(LogFilePath, entry);
            }
            catch
            {
                // Logging must never throw and mask the original error.
            }
            finally
            {
                if (acquired)
                {
                    try { FileMutex.ReleaseMutex(); } catch { }
                }
            }
        }

        private static void RotateIfNeeded()
        {
            try
            {
                var info = new FileInfo(LogFilePath);
                if (!info.Exists || info.Length < MaxLogBytes)
                {
                    return;
                }

                string archived = LogFilePath + ".1";
                if (File.Exists(archived))
                {
                    File.Delete(archived);
                }

                File.Move(LogFilePath, archived);
            }
            catch
            {
                // A rotation failure must not stop the actual log write.
            }
        }
    }
}
