using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace PharmacySystem.Helpers
{
    // Static error-logging facade kept so the ~170 existing Logger.LogError(ex) call sites stay
    // untouched. Internally it now routes through Microsoft.Extensions.Logging (Serilog file
    // sink) once Initialize() has run - config-driven level, structured, and the file sink
    // handles concurrent writes (shared: true) so the old cross-process mutex is gone. Before
    // Initialize() runs (e.g. a failure in CompositionRoot's static initializer) it falls back
    // to a plain guarded file append.
    public static class Logger
    {
        private static readonly string LogFilePath = Path.Combine(AppContext.BaseDirectory, "error.log");
        private const long MaxLogBytes = 5 * 1024 * 1024;

        private static readonly Mutex FileMutex = new Mutex(false, "PharmacySystem.ErrorLog");

        private static ILoggerFactory? _factory;
        private static ILogger? _logger;

        // Call once at startup with the application configuration. Reads the log file path from
        // "Logging:File:Path" and the minimum level from "Logging:LogLevel:Default"
        // (Trace/Debug/Information/Warning/Error/Critical), both optional.
        public static void Initialize(IConfiguration? configuration = null)
        {
            string? path = configuration?["Logging:File:Path"];
            if (string.IsNullOrWhiteSpace(path))
            {
                path = LogFilePath;
            }

            LogEventLevel level = ParseLevel(configuration?["Logging:LogLevel:Default"]);

            var serilog = new LoggerConfiguration()
                .MinimumLevel.Is(level)
                .Enrich.FromLogContext()
                .WriteTo.File(
                    path,
                    fileSizeLimitBytes: MaxLogBytes,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 3,
                    shared: true,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] {SourceMember} {Level:u3}: {Message:lj}{NewLine}{Exception}{NewLine}--------------------------------------------------------------------------------{NewLine}")
                .CreateLogger();

            _factory?.Dispose();
            _factory = new SerilogLoggerFactory(serilog, dispose: true);
            _logger = _factory.CreateLogger("PharmacySystem");
        }

        // Exposed so hosting code (and future DI) can share the same factory. Null until
        // Initialize() has run.
        public static ILoggerFactory? LoggerFactory => _factory;

        public static void LogError(Exception ex, [CallerMemberName] string memberName = "")
        {
            ILogger? logger = _logger;
            if (logger != null)
            {
                using (logger.BeginScope(new System.Collections.Generic.Dictionary<string, object> { ["SourceMember"] = memberName }))
                {
                    logger.LogError(ex, "{Member}", memberName);
                }
                return;
            }

            FallbackAppend(ex, memberName);
        }

        private static LogEventLevel ParseLevel(string? text)
        {
            switch ((text ?? "").Trim().ToLowerInvariant())
            {
                case "trace": return LogEventLevel.Verbose;
                case "debug": return LogEventLevel.Debug;
                case "warning": return LogEventLevel.Warning;
                case "error": return LogEventLevel.Error;
                case "critical": return LogEventLevel.Fatal;
                default: return LogEventLevel.Information;
            }
        }

        // Pre-Initialize path: the original guarded, size-rotated file append.
        private static void FallbackAppend(Exception ex, string memberName)
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
