using PharmacySystem.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;


namespace PharmacySystem.Logical
{
    public class NotificationConfigService
    {
        public List<Product> ListExpirationDate()
        {
            List<Product> List = new List<Product>();
            using (SqlConnection oConnection = new SqlConnection(Connection.CN))
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
                            if (dr["date_expired"] != DBNull.Value) {
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
                    List = new List<Product>();
                }
            }
            return List;
        }

        public List<Product> ListStock()
        {
            List<Product> List = new List<Product>();
            using (SqlConnection oConnection = new SqlConnection(Connection.CN))
            {
                try
                {


                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("SELECT stock FROM product");
                    sb.AppendLine("WHERE status = 1 AND date_expired IS NOT NULL");
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
                    List = new List<Product>();
                }
            }
            return List;
        }

        public int ConfigDay()
        {
            NotificationConfig day = new NotificationConfig();
            int notifyDay = 0;
            object a;
            using (SqlConnection oConnection = new SqlConnection(Connection.CN))
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

                }
            }
            return notifyDay;
        }

        public int ConfigStock()
        {
            NotificationConfig day = new NotificationConfig();
            int criticalStock = 0;
            using (SqlConnection oConnection = new SqlConnection(Connection.CN))
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

                }
            }
            return criticalStock;
        }

        public bool ConfigUpdate(NotificationConfig obj)
        {
            bool result = true;
            using (SqlConnection oConnection = new SqlConnection(Connection.CN))
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
                    result = false;
                }

            }

            return result;

        }
    }
}
