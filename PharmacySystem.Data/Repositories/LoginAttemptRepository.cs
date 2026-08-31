using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Dapper;
using PharmacySystem.Helpers;
using PharmacySystem.Infrastructure;

namespace PharmacySystem.Data
{
    public class LoginAttemptRepository : ILoginAttemptRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        // Only failures newer than the latest success row count toward the lockout; this is the
        // shared predicate for the count and the "oldest" query.
        private const string SinceLastResetFilter =
            "document_number = @document AND success = 0 " +
            "AND at >= DATEADD(MINUTE, -@window, GETDATE()) " +
            "AND at > ISNULL((SELECT MAX(at) FROM login_attempt WHERE document_number = @document AND success = 1), '1900-01-01')";

        public LoginAttemptRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public void Record(string document, bool success, string reason, int? actorId, string station)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    int id = oConnection.ExecuteScalar<int>(
                        "INSERT INTO login_attempt (document_number, success, reason, actor_id, station) " +
                        "VALUES (@document, @success, @reason, @actorId, @station); " +
                        "SELECT CAST(SCOPE_IDENTITY() AS INT);",
                        new
                        {
                            document = document ?? string.Empty,
                            success,
                            reason = string.IsNullOrEmpty(reason) ? "login" : reason,
                            actorId,
                            station
                        });

                    // Cheap housekeeping: trim rows older than 90 days once every ~500 inserts.
                    if (id % 500 == 0)
                    {
                        oConnection.Execute("sp_purge_login_attempts", commandType: System.Data.CommandType.StoredProcedure);
                    }
                }
                catch (SqlException ex) when (SqlErrorCodes.IsConnectivityError(ex))
                {
                    Logger.LogError(ex);
                    throw new DataUnavailableException(DataUnavailableException.DefaultMessage, ex);
                }
                catch (Exception ex)
                {
                    // Never let an audit-write failure block a login attempt from being evaluated.
                    Logger.LogError(ex);
                }
            }
        }

        public int CountFailuresSinceLastReset(string document, int windowMinutes)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    return oConnection.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM login_attempt WHERE " + SinceLastResetFilter,
                        new { document = document ?? string.Empty, window = windowMinutes });
                }
                catch (SqlException ex) when (SqlErrorCodes.IsConnectivityError(ex))
                {
                    Logger.LogError(ex);
                    throw new DataUnavailableException(DataUnavailableException.DefaultMessage, ex);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return 0;
                }
            }
        }

        public int? MinutesUntilUnlock(string document, int windowMinutes)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    // @window minus the age (in minutes) of the oldest counting failure, all
                    // server-side; NULL when there are no counting failures.
                    return oConnection.ExecuteScalar<int?>(
                        "SELECT @window - DATEDIFF(MINUTE, MIN(at), GETDATE()) " +
                        "FROM login_attempt WHERE " + SinceLastResetFilter,
                        new { document = document ?? string.Empty, window = windowMinutes });
                }
                catch (SqlException ex) when (SqlErrorCodes.IsConnectivityError(ex))
                {
                    Logger.LogError(ex);
                    throw new DataUnavailableException(DataUnavailableException.DefaultMessage, ex);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return null;
                }
            }
        }

        public ISet<string> ListLockedDocuments(int windowMinutes, int maxFailures)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    // Same "counting failures" rule as CountFailuresSinceLastReset, grouped by
                    // document, keeping the ones at or over the threshold.
                    const string sql =
                        "SELECT la.document_number FROM login_attempt la " +
                        "WHERE la.success = 0 " +
                        "AND la.at >= DATEADD(MINUTE, -@window, GETDATE()) " +
                        "AND la.at > ISNULL((SELECT MAX(s.at) FROM login_attempt s " +
                        "WHERE s.document_number = la.document_number AND s.success = 1), '1900-01-01') " +
                        "GROUP BY la.document_number HAVING COUNT(*) >= @max";

                    var rows = oConnection.Query<string>(sql, new { window = windowMinutes, max = maxFailures });
                    return new HashSet<string>(rows);
                }
                catch (SqlException ex) when (SqlErrorCodes.IsConnectivityError(ex))
                {
                    Logger.LogError(ex);
                    throw new DataUnavailableException(DataUnavailableException.DefaultMessage, ex);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return new HashSet<string>();
                }
            }
        }
    }
}
