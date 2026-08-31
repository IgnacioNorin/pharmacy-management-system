using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Data.SqlClient;

namespace PharmacySystem.Data
{
    public class SqlConnectionFactory : ISqlConnectionFactory
    {
        private const string DefaultConnectionName = "connection";

        private readonly string _connectionString;

        public SqlConnectionFactory(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("The connection string cannot be empty.", nameof(connectionString));
            }

            _connectionString = connectionString;
        }

        // Reads the "connection" entry from the caller's configuration. Fails loudly when it is
        // missing: a null connection string would otherwise surface much later as an opaque error
        // from deep inside a repository.
        public static SqlConnectionFactory FromConfiguration(string connectionName = DefaultConnectionName)
        {
            // 1. Ambient config (App.config -> <assembly>.dll.config). This is what the running
            //    application uses.
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[connectionName];
            if (settings != null && !string.IsNullOrWhiteSpace(settings.ConnectionString))
            {
                return new SqlConnectionFactory(settings.ConnectionString);
            }

            // 2. Fall back to ConnectionStrings.config next to the running assembly. The test host
            //    (dotnet test / vstest) does not surface a test project's App.config through
            //    ConfigurationManager, but the file is copied to the output directory all the same.
            string fromFile = ReadConnectionStringFromFile(
                Path.Combine(AppContext.BaseDirectory, "ConnectionStrings.config"), connectionName);
            if (!string.IsNullOrWhiteSpace(fromFile))
            {
                return new SqlConnectionFactory(fromFile);
            }

            throw new ConfigurationErrorsException(
                $"No connection string named '{connectionName}' was found in the configuration file.");
        }

        private static string ReadConnectionStringFromFile(string path, string connectionName)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                return XDocument.Load(path).Root?
                    .Elements("add")
                    .FirstOrDefault(e => string.Equals((string)e.Attribute("name"), connectionName, StringComparison.Ordinal))?
                    .Attribute("connectionString")?.Value;
            }
            catch (System.Xml.XmlException)
            {
                return null;
            }
        }

        public SqlConnection Create()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
