using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using PharmacySystem.Helpers;
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
                    // Same cutoff the caller used to compute in C# (today >= expirationDate - days,
                    // i.e. expirationDate <= today + days), now applied server-side so only the
                    // rows that actually matter cross the wire.
                    return oConnection.Query<Product>(
                        "SELECT date_expired AS expirationDate FROM product " +
                        "WHERE status = 1 AND date_expired IS NOT NULL " +
                        "AND date_expired <= DATEADD(day, @days, CAST(GETDATE() AS DATE))",
                        new { days })
                        .ToList();
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
                        "SELECT stock FROM product WHERE status = 1 AND stock <= @criticalStock",
                        new { criticalStock })
                        .ToList();
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
                    return oConnection.QueryFirstOrDefault<int>("SELECT notify_day FROM notification_settings");
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
                    return oConnection.QueryFirstOrDefault<int>("SELECT critical_stock FROM notification_settings");
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
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return false;
                }
            }
        }
    }
}
