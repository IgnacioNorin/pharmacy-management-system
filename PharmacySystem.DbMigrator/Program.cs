using System;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Reflection;
using DbUp;
using DbUp.Engine;

namespace PharmacySystem.DbMigrator
{
    // Applies the pending Database/Migrations/*.sql scripts to a SQL Server database, in order,
    // one transaction per script, tracking what ran in dbo.SchemaVersions. Replaces applying
    // them by hand with sqlcmd / SSMS (which is how the QUOTED_IDENTIFIER-OFF bug in migration
    // 013 slipped in).
    //
    // Connection string resolution, in order: first CLI argument, then the
    // PHARMACY_DB_CONNECTION environment variable, then appsettings[.Local].json ("connection").
    internal static class Program
    {
        private const string EmbeddedPrefix = "PharmacySystem.DbMigrator.Migrations.";

        private static int Main(string[] args)
        {
            string connectionString = ResolveConnectionString(args);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Console.Error.WriteLine(
                    "No connection string. Pass it as the first argument, set PHARMACY_DB_CONNECTION, " +
                    "or add a 'connection' entry to appsettings.Local.json next to the executable.");
                return 2;
            }

            Assembly scriptAssembly = Assembly.GetExecutingAssembly();

            try
            {
                RecordBaselineIfPreExistingDatabase(connectionString, scriptAssembly);

                UpgradeEngine upgrader = DeployChanges.To
                    .SqlDatabase(connectionString)
                    .WithScriptsEmbeddedInAssembly(scriptAssembly, name => name.StartsWith(EmbeddedPrefix, StringComparison.Ordinal))
                    .JournalToSqlTable("dbo", "SchemaVersions")
                    .WithTransactionPerScript()
                    .LogToConsole()
                    .Build();

                if (!upgrader.IsUpgradeRequired())
                {
                    Console.WriteLine("Database is up to date. Nothing to run.");
                    return 0;
                }

                DatabaseUpgradeResult result = upgrader.PerformUpgrade();
                if (result.Successful)
                {
                    Console.WriteLine("Migration complete.");
                    return 0;
                }

                Console.Error.WriteLine("Migration failed on '" + result.ErrorScript?.Name + "': " + result.Error);
                return 1;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Migration failed: " + ex);
                return 1;
            }
        }

        private static string ResolveConnectionString(string[] args)
        {
            if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
            {
                return args[0];
            }

            string fromEnv = Environment.GetEnvironmentVariable("PHARMACY_DB_CONNECTION");
            if (!string.IsNullOrWhiteSpace(fromEnv))
            {
                return fromEnv;
            }

            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Local.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            return config.GetConnectionString("connection");
        }

        // A database that predates this tool - or one created straight from PharmacyDB.sql, which
        // already carries every migration's effect - must not have 001..N re-run against it. If
        // dbo.person exists but dbo.SchemaVersions does not, create the journal and record every
        // script shipped in this build as already applied, so only newer migrations run.
        private static void RecordBaselineIfPreExistingDatabase(string connectionString, Assembly scriptAssembly)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                bool hasPerson = Scalar(connection, "SELECT CASE WHEN OBJECT_ID('dbo.person') IS NOT NULL THEN 1 ELSE 0 END") == 1;
                bool hasJournal = Scalar(connection, "SELECT CASE WHEN OBJECT_ID('dbo.SchemaVersions') IS NOT NULL THEN 1 ELSE 0 END") == 1;

                if (!hasPerson || hasJournal)
                {
                    return;
                }

                Console.WriteLine("Existing database without a migration journal: recording current migrations as already applied.");

                Execute(connection,
                    "CREATE TABLE dbo.SchemaVersions (" +
                    "SchemaVersionsID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SchemaVersions_Id PRIMARY KEY, " +
                    "ScriptName NVARCHAR(255) NOT NULL, " +
                    "Applied DATETIME NOT NULL)");

                var scripts = scriptAssembly.GetManifestResourceNames()
                    .Where(n => n.StartsWith(EmbeddedPrefix, StringComparison.Ordinal))
                    .OrderBy(n => n, StringComparer.Ordinal);

                foreach (string script in scripts)
                {
                    Execute(connection,
                        "INSERT INTO dbo.SchemaVersions (ScriptName, Applied) VALUES (@name, GETUTCDATE())",
                        new SqlParameter("@name", script));
                    Console.WriteLine("  baselined: " + script.Substring(EmbeddedPrefix.Length));
                }
            }
        }

        private static int Scalar(SqlConnection connection, string sql)
        {
            using (var command = new SqlCommand(sql, connection))
            {
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        private static void Execute(SqlConnection connection, string sql, params SqlParameter[] parameters)
        {
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddRange(parameters);
                command.ExecuteNonQuery();
            }
        }
    }
}
