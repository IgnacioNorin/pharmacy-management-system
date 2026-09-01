using System;
using System.IO;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using PharmacySystem.Infrastructure;

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

        // Builds the application configuration: appsettings.json (checked in, placeholder) is
        // overridden by appsettings.Local.json (git-ignored, real dev values) and then by
        // environment variables (ConnectionStrings__connection), used by CI and deployments.
        public static IConfiguration BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Local.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

        // Reads the named connection string from configuration. Fails loudly when it is missing:
        // a null connection string would otherwise surface much later as an opaque error from
        // deep inside a repository.
        public static SqlConnectionFactory FromConfiguration(string connectionName = DefaultConnectionName)
        {
            return FromConfiguration(BuildConfiguration(), connectionName);
        }

        public static SqlConnectionFactory FromConfiguration(IConfiguration configuration, string connectionName = DefaultConnectionName)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            string? connectionString = configuration.GetConnectionString(connectionName);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new MissingConfigurationException(
                    $"No connection string named '{connectionName}' was found. Set it in appsettings.Local.json " +
                    "or in the ConnectionStrings__connection environment variable.");
            }

            return new SqlConnectionFactory(connectionString);
        }

        public SqlConnection Create()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
