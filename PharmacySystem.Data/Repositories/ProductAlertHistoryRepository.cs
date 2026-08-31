using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using PharmacySystem.Helpers;
using PharmacySystem.Infrastructure;
using PharmacySystem.Model;

namespace PharmacySystem.Data
{
    public class ProductAlertHistoryRepository : IProductAlertHistoryRepository
    {
        private const string SelectColumns =
            "h.id AS Id, h.product_id AS ProductId, p.code AS ProductCode, p.name AS ProductName, " +
            "h.alert_type AS AlertType, h.severity AS Severity, h.trigger_value AS TriggerValue, " +
            "h.detected_at AS DetectedAt, h.resolved_at AS ResolvedAt, " +
            "h.acknowledged_by AS AcknowledgedBy, h.acknowledged_at AS AcknowledgedAt, " +
            "h.muted_at AS MutedAt";

        private readonly ISqlConnectionFactory _connectionFactory;

        public ProductAlertHistoryRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public List<ProductAlertHistoryEntry> GetOpenAlerts()
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    return oConnection.Query<ProductAlertHistoryEntry>(
                        "SELECT " + SelectColumns + " " +
                        "FROM product_alert_history h INNER JOIN product p ON p.id = h.product_id " +
                        "WHERE h.resolved_at IS NULL")
                        .ToList();
                }
                catch (SqlException ex) when (SqlErrorCodes.IsConnectivityError(ex))
                {
                    Logger.LogError(ex);
                    throw new DataUnavailableException(DataUnavailableException.DefaultMessage, ex);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return new List<ProductAlertHistoryEntry>();
                }
            }
        }

        public int Insert(int productId, AlertType alertType, AlertSeverity severity, decimal? triggerValue)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    return oConnection.ExecuteScalar<int>(
                        "INSERT INTO product_alert_history (product_id, alert_type, severity, trigger_value, detected_at) " +
                        "VALUES (@productId, @alertType, @severity, @triggerValue, GETDATE()); " +
                        "SELECT CAST(SCOPE_IDENTITY() AS INT);",
                        new { productId, alertType, severity, triggerValue });
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

        public void UpdateSeverity(int historyId, AlertSeverity severity, decimal? triggerValue)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    // A severity change means the condition the mute applied to no longer holds -
                    // clear it here too, in case the caller ever updates severity without going
                    // through NotificationConfigService.SyncAlertHistory's own reset.
                    oConnection.Execute(
                        "UPDATE product_alert_history SET severity = @severity, trigger_value = @triggerValue, muted_at = NULL, muted_by = NULL WHERE id = @historyId",
                        new { historyId, severity, triggerValue });
                }
                catch (SqlException ex) when (SqlErrorCodes.IsConnectivityError(ex))
                {
                    Logger.LogError(ex);
                    throw new DataUnavailableException(DataUnavailableException.DefaultMessage, ex);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                }
            }
        }

        public void Resolve(int historyId)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    oConnection.Execute(
                        "UPDATE product_alert_history SET resolved_at = GETDATE() WHERE id = @historyId AND resolved_at IS NULL",
                        new { historyId });
                }
                catch (SqlException ex) when (SqlErrorCodes.IsConnectivityError(ex))
                {
                    Logger.LogError(ex);
                    throw new DataUnavailableException(DataUnavailableException.DefaultMessage, ex);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                }
            }
        }

        public bool Acknowledge(int historyId, int personId)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    // Guard against two users acknowledging the same alert at once: only the
                    // first write lands, and it does not overwrite an alert that already
                    // resolved on its own (DEF-38).
                    int rows = oConnection.Execute(
                        "UPDATE product_alert_history SET acknowledged_by = @personId, acknowledged_at = GETDATE() " +
                        "WHERE id = @historyId AND acknowledged_at IS NULL AND resolved_at IS NULL",
                        new { historyId, personId });
                    return rows > 0;
                }
                catch (SqlException ex) when (SqlErrorCodes.IsConnectivityError(ex))
                {
                    Logger.LogError(ex);
                    throw new DataUnavailableException(DataUnavailableException.DefaultMessage, ex);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return false;
                }
            }
        }

        public bool Mute(int historyId, int personId)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    int rows = oConnection.Execute(
                        "UPDATE product_alert_history SET muted_at = GETDATE(), muted_by = @personId WHERE id = @historyId AND resolved_at IS NULL",
                        new { historyId, personId });
                    return rows > 0;
                }
                catch (SqlException ex) when (SqlErrorCodes.IsConnectivityError(ex))
                {
                    Logger.LogError(ex);
                    throw new DataUnavailableException(DataUnavailableException.DefaultMessage, ex);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return false;
                }
            }
        }

        public bool Unmute(int historyId)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    int rows = oConnection.Execute(
                        "UPDATE product_alert_history SET muted_at = NULL, muted_by = NULL WHERE id = @historyId",
                        new { historyId });
                    return rows > 0;
                }
                catch (SqlException ex) when (SqlErrorCodes.IsConnectivityError(ex))
                {
                    Logger.LogError(ex);
                    throw new DataUnavailableException(DataUnavailableException.DefaultMessage, ex);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return false;
                }
            }
        }

        public List<ProductAlertHistoryEntry> GetHistory(DateTime startDate, DateTime endDate)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    return oConnection.Query<ProductAlertHistoryEntry>(
                        "SELECT " + SelectColumns + ", per.name AS AcknowledgedByName " +
                        "FROM product_alert_history h " +
                        "INNER JOIN product p ON p.id = h.product_id " +
                        "LEFT JOIN person per ON per.id = h.acknowledged_by " +
                        "WHERE CAST(h.detected_at AS DATE) BETWEEN @startDate AND @endDate " +
                        "ORDER BY h.detected_at DESC",
                        new { startDate, endDate })
                        .ToList();
                }
                catch (SqlException ex) when (SqlErrorCodes.IsConnectivityError(ex))
                {
                    Logger.LogError(ex);
                    throw new DataUnavailableException(DataUnavailableException.DefaultMessage, ex);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return new List<ProductAlertHistoryEntry>();
                }
            }
        }
    }
}
