using System;
using System.Data.SqlClient;

namespace PharmacySystem.Data
{
    internal static class SqlErrorCodes
    {
        // 2601 = unique index violation, 2627 = unique/PK constraint violation. Both mean a row
        // with the same natural key already exists - the repositories rely on the unique indexes
        // for this instead of a race-prone "IF NOT EXISTS" pre-check.
        public static bool IsUniqueViolation(Exception ex) =>
            ex is SqlException sql && (sql.Number == 2601 || sql.Number == 2627);

        // Connection-level failures: the server is unreachable, the network path is gone, the
        // login was rejected, the database cannot be opened, or the command timed out. These
        // mean "no database", not "no data", so the critical paths rethrow them as
        // DataUnavailableException instead of swallowing them into a neutral result.
        public static bool IsConnectivityError(Exception ex)
        {
            if (!(ex is SqlException sql))
                return false;

            // Severity 20 and above is a fatal connection or server error in SQL Server.
            if (sql.Class >= 20)
                return true;

            switch (sql.Number)
            {
                case -2:     // command timeout
                case 2:      // server not found / no network path
                case 53:     // network path not found
                case 40:     // could not open a connection to SQL Server
                case 64:     // connection failed during the login process
                case 233:    // no process is on the other end of the pipe
                case 1225:   // the remote computer refused the network connection
                case 4060:   // cannot open the requested database
                case 10053:  // transport-level error (established connection aborted)
                case 10054:  // existing connection was forcibly closed by the remote host
                case 10060:  // connection attempt timed out
                case 11001:  // host not found
                case 18456:  // login failed for user
                    return true;
                default:
                    return false;
            }
        }
    }
}
