using System;
using System.Data.SqlClient;
using Dapper;
using PharmacySystem.Helpers;
using PharmacySystem.Infrastructure;

namespace PharmacySystem.Data
{
    public class SecurityEventRepository : ISecurityEventRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public SecurityEventRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public void Record(int? actorId, string action, string entity, int? entityId, string summary, string station)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    int id = oConnection.ExecuteScalar<int>(
                        "INSERT INTO security_event (actor_id, action, entity, entity_id, summary, station) " +
                        "VALUES (@actorId, @action, @entity, @entityId, @summary, @station); " +
                        "SELECT CAST(SCOPE_IDENTITY() AS INT);",
                        new
                        {
                            actorId = actorId == 0 ? (int?)null : actorId,
                            action,
                            entity,
                            entityId,
                            summary = Truncate(summary, 400),
                            station
                        });

                    // Cheap housekeeping: trim rows older than two years once in a while.
                    if (id % 1000 == 0)
                    {
                        oConnection.Execute("sp_purge_security_event", commandType: System.Data.CommandType.StoredProcedure);
                    }
                }
                catch (SqlException ex) when (SqlErrorCodes.IsConnectivityError(ex))
                {
                    Logger.LogError(ex);
                    throw new DataUnavailableException(DataUnavailableException.DefaultMessage, ex);
                }
                catch (Exception ex)
                {
                    // Never let an audit-write failure block the operation that triggered it.
                    Logger.LogError(ex);
                }
            }
        }

        private static string Truncate(string s, int max) =>
            string.IsNullOrEmpty(s) || s.Length <= max ? s : s.Substring(0, max);
    }
}
