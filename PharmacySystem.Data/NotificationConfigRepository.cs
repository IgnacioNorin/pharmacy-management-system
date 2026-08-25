using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
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

        public List<Product> ListExpirationDate()
        {
            List<Product> List = new List<Product>();
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("SELECT date_expired FROM product");
                    sb.AppendLine("WHERE status = 1");
                    SqlCommand cmd = new SqlCommand(sb.ToString(), oConnection);
                    cmd.CommandType = CommandType.Text;

                    oConnection.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr["date_expired"] != DBNull.Value)
                            {
                                List.Add(new Product()
                                {
                                    expirationDate = Convert.ToDateTime(dr["date_expired"])
                                });
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    List = new List<Product>();
                }
            }
            return List;
        }

        public List<Product> ListStock()
        {
            List<Product> List = new List<Product>();
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("SELECT stock FROM product");
                    sb.AppendLine("WHERE status = 1");
                    SqlCommand cmd = new SqlCommand(sb.ToString(), oConnection);
                    cmd.CommandType = CommandType.Text;

                    oConnection.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            List.Add(new Product()
                            {
                                stock = Convert.ToInt32(dr["stock"])
                            });
                        }
                    }

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    List = new List<Product>();
                }
            }
            return List;
        }

        public int ConfigDay()
        {
            int notifyDay = 0;
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    oConnection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("SELECT notify_day FROM notification_settings");
                    SqlCommand cmd = new SqlCommand(sb.ToString(), oConnection);
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        notifyDay = int.Parse(dr.GetValue(0).ToString());
                    }

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                }
            }
            return notifyDay;
        }

        public int ConfigStock()
        {
            int criticalStock = 0;
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    oConnection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("SELECT critical_stock FROM notification_settings");
                    SqlCommand cmd = new SqlCommand(sb.ToString(), oConnection);
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        criticalStock = int.Parse(dr.GetValue(0).ToString());
                    }

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                }
            }
            return criticalStock;
        }

        public bool ConfigUpdate(NotificationConfig obj)
        {
            bool result = true;
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("sp_update_notificacion_settings", oConnection);
                    cmd.Parameters.AddWithValue("@critical_stock", obj.criticalStock);
                    cmd.Parameters.AddWithValue("@notify_day", obj.days);
                    cmd.Parameters.Add("result", SqlDbType.Bit).Direction = ParameterDirection.Output;

                    cmd.CommandType = CommandType.StoredProcedure;

                    oConnection.Open();

                    cmd.ExecuteNonQuery();

                    result = Convert.ToBoolean(cmd.Parameters["result"].Value);

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    result = false;
                }

            }

            return result;
        }
    }
}
