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
    }
}
