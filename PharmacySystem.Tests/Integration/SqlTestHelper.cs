using System;
using System.Data.SqlClient;
using PharmacySystem.Logical;

namespace PharmacySystem.Tests.Integration
{
    // Thin ADO.NET helper for integration test setup/cleanup. Deliberately independent from
    // the Service classes under test, so assertions don't rely on the same code path they verify.
    internal static class SqlTestHelper
    {
        public static void ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(Connection.CN))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public static object ExecuteScalar(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(Connection.CN))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                connection.Open();
                return command.ExecuteScalar();
            }
        }

        public static int ExecuteScalarInt(string sql, params SqlParameter[] parameters)
        {
            return Convert.ToInt32(ExecuteScalar(sql, parameters));
        }

        // Short, unique, alphabetic-safe tag for names/codes/documents created by a test,
        // so parallel test runs (or leftover rows from a failed run) never collide.
        public static string NewTag()
        {
            return "T" + Guid.NewGuid().ToString("N").Substring(0, 10);
        }
    }
}
