using System;
using System.Configuration;
using System.Data.SqlClient;

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

        // Reads the "connection" entry from the caller's configuration file. Fails loudly when it
        // is missing: a null connection string would otherwise surface much later as an opaque
        // error from deep inside a repository.
        public static SqlConnectionFactory FromConfiguration(string connectionName = DefaultConnectionName)
        {
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[connectionName];

            if (settings == null)
            {
                throw new ConfigurationErrorsException(
                    $"No connection string named '{connectionName}' was found in the configuration file.");
            }

            return new SqlConnectionFactory(settings.ConnectionString);
        }

        public SqlConnection Create()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
