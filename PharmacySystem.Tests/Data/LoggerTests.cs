using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Configuration;
using PharmacySystem.Helpers;
using Xunit;

namespace PharmacySystem.Tests.Data
{
    // Logger is a static facade over Microsoft.Extensions.Logging (Serilog file sink). These
    // pin down that Initialize() honours the configured path and that an exception ends up in
    // the file. Logger is process-global, so the test restores the default sink at the end.
    public class LoggerTests
    {
        [Fact]
        public void LogError_AfterInitialize_WritesTheExceptionToTheConfiguredFile()
        {
            string dir = Path.Combine(Path.GetTempPath(), "pharmlog_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, "error.log");

            try
            {
                IConfiguration config = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["Logging:File:Path"] = path,
                        ["Logging:LogLevel:Default"] = "Information"
                    })
                    .Build();

                Logger.Initialize(config);
                Logger.LogError(new InvalidOperationException("logger-test-boom"));

                string content = ReadWithRetry(path);

                Assert.Contains("logger-test-boom", content);
                Assert.Contains(nameof(InvalidOperationException), content);
            }
            finally
            {
                Logger.Initialize(null);
                try { Directory.Delete(dir, recursive: true); } catch { /* best effort */ }
            }
        }

        private static string ReadWithRetry(string path)
        {
            for (int attempt = 0; attempt < 20; attempt++)
            {
                if (File.Exists(path))
                {
                    try
                    {
                        using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        using (var reader = new StreamReader(stream))
                        {
                            string text = reader.ReadToEnd();
                            if (text.Length > 0)
                            {
                                return text;
                            }
                        }
                    }
                    catch (IOException)
                    {
                        // sink still has the handle; retry
                    }
                }

                Thread.Sleep(100);
            }

            return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
        }
    }
}
