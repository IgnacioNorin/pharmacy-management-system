using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using PharmacySystem.Helpers;
using PharmacySystem.Infrastructure;
using PharmacySystem.Model;

namespace PharmacySystem.Data
{
    public class NotificationConfigRepository : INotificationConfigRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public NotificationConfigRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public List<Product> ListExpirationDate(int days)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    // DEF-02 fase A (fase 2): the expiry alert is driven by the product's lots
                    // (product_lot), not the single product.date_expired field. One row per
                    // product, with the earliest-expiring lot that still has stock and its
                    // quantity, so:
                    //  - a newer lot with a far expiry can't hide the older stock on the shelf;
                    //  - selling that stock down (FEFO empties the near lot first) clears the
                    //    alert on its own;
                    //  - the detail can say how many units are expiring, not just "something is".
                    return oConnection.Query<Product>(
                        "SELECT p.id AS idProduct, p.code, p.name, " +
                        "MIN(pl.date_expired) AS expirationDate, " +
                        "SUM(CASE WHEN pl.date_expired <= DATEADD(day, @days, CAST(GETDATE() AS DATE)) THEN pl.quantity ELSE 0 END) AS stock " +
                        "FROM product p INNER JOIN product_lot pl ON pl.product_id = p.id " +
                        "WHERE p.status = 1 AND pl.quantity > 0 AND pl.date_expired IS NOT NULL " +
                        "GROUP BY p.id, p.code, p.name " +
                        "HAVING MIN(pl.date_expired) <= DATEADD(day, @days, CAST(GETDATE() AS DATE)) " +
                        "ORDER BY MIN(pl.date_expired) ASC",
                        new { days })
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
                    return new List<Product>();
                }
            }
        }

        public List<Product> ListStock(int criticalStock)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    return oConnection.Query<Product>(
                        "SELECT id AS idProduct, code, name, stock FROM product " +
                        "WHERE status = 1 AND stock <= @criticalStock " +
                        "ORDER BY stock ASC",
                        new { criticalStock })
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
                    return new List<Product>();
                }
            }
        }

        public int ConfigDay()
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    return oConnection.QueryFirstOrDefault<int>("SELECT notify_day FROM notification_settings WHERE id = 1");
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

        public int ConfigStock()
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    return oConnection.QueryFirstOrDefault<int>("SELECT critical_stock FROM notification_settings WHERE id = 1");
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

        public bool ConfigUpdate(NotificationConfig obj)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("critical_stock", obj.criticalStock);
                    parameters.Add("notify_day", obj.days);
                    parameters.Add("result", dbType: DbType.Boolean, direction: ParameterDirection.Output);

                    oConnection.Execute("sp_update_notificacion_settings", parameters, commandType: CommandType.StoredProcedure);

                    return parameters.Get<bool>("result");
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
    }
}
