using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using PharmacySystem.Helpers;
using PharmacySystem.Model;

namespace PharmacySystem.Data
{
    public class SaleRepository : ISaleRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public SaleRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public List<Sale> ListSale()
        {
            List<Sale> List = new List<Sale>();

            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("SELECT id, document_type, document_number, document_client, name_client, total_amount, amount_received, change_amount, date_registered FROM sale");

                    SqlCommand cmd = new SqlCommand(sb.ToString(), oConnection);
                    cmd.CommandType = CommandType.Text;

                    oConnection.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            List.Add(new Sale()
                            {
                                idSale = Convert.ToInt32(dr["id"].ToString()),
                                typeDocument = dr["document_type"].ToString(),
                                numberDocument = dr["document_number"].ToString(),
                                documentClient = dr["document_client"].ToString(),
                                nameClient = dr["name_client"].ToString(),
                                totalPay = Convert.ToDecimal(dr["total_amount"]),
                                payWith = Convert.ToDecimal(dr["amount_received"]),
                                change = Convert.ToDecimal(dr["change_amount"]),
                                registrationDate = Convert.ToDateTime(dr["date_registered"])
                            });
                        }
                    }

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    List = new List<Sale>();
                }
            }
            return List;
        }

        public List<SaleDetail> ListSaleDetail()
        {
            List<SaleDetail> List = new List<SaleDetail>();

            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("SELECT sd.id, sd.sale_id ,p.name, sd.stock, sd.sale_price, sd.subtotal FROM sale_detail sd");
                    sb.AppendLine("INNER JOIN product p ON p.id = sd.product_id");

                    SqlCommand cmd = new SqlCommand(sb.ToString(), oConnection);
                    cmd.CommandType = CommandType.Text;

                    oConnection.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            List.Add(new SaleDetail()
                            {
                                idSaleDetail = Convert.ToInt32(dr["id"].ToString()),
                                idSale = Convert.ToInt32(dr["sale_id"].ToString()),
                                oProduct = new Product() { name = dr["name"].ToString() },
                                amount = Convert.ToInt32(dr["stock"].ToString()),
                                salePrice = Convert.ToDecimal(dr["sale_price"]),
                                subtotal = Convert.ToDecimal(dr["subtotal"])
                            });
                        }
                    }

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    List = new List<SaleDetail>();
                }
            }
            return List;
        }

        public bool ControlStock(int idproduct, int amount, bool subtract)
        {
            bool result = true;
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    string query = string.Format("UPDATE product SET stock = (stock {0} @amount) WHERE id = @idproduct", subtract ? "-" : "+");
                    SqlCommand cmd = new SqlCommand(query, oConnection);
                    cmd.Parameters.AddWithValue("@amount", amount);
                    cmd.Parameters.AddWithValue("@idproduct", idproduct);
                    cmd.CommandType = CommandType.Text;
                    oConnection.Open();
                    cmd.ExecuteNonQuery();
                    result = true;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    result = false;
                }
            }

            return result;
        }

        public int Register(Sale obj)
        {
            int result = 0;

            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    oConnection.Open();
                    SqlTransaction objTransacion = oConnection.BeginTransaction();

                    try
                    {
                        string insertSaleQuery = "INSERT INTO sale(document_type, document_number, user_id, document_client, name_client, total_amount, amount_received, change_amount) " +
                            "VALUES (@document_type, (SELECT RIGHT('000000' + CAST((SELECT count(*) + 1 FROM sale) AS VARCHAR), 6)), @user_id, @document_client, @name_client, @total_amount, @amount_received, @change_amount); " +
                            "SELECT SCOPE_IDENTITY();";

                        SqlCommand cmdSale = new SqlCommand(insertSaleQuery, oConnection, objTransacion);
                        cmdSale.Parameters.AddWithValue("@document_type", obj.typeDocument);
                        cmdSale.Parameters.AddWithValue("@user_id", obj.oPerson.idPerson);
                        cmdSale.Parameters.AddWithValue("@document_client", obj.documentClient);
                        cmdSale.Parameters.AddWithValue("@name_client", obj.nameClient);
                        cmdSale.Parameters.AddWithValue("@total_amount", obj.totalPay);
                        cmdSale.Parameters.AddWithValue("@amount_received", obj.payWith);
                        cmdSale.Parameters.AddWithValue("@change_amount", obj.change);

                        int idSale = 0;
                        int.TryParse(cmdSale.ExecuteScalar()?.ToString(), out idSale);

                        if (idSale != 0)
                        {
                            string insertDetailQuery = "INSERT INTO sale_detail(sale_id, product_id, stock, sale_price, subtotal) " +
                                "VALUES (@sale_id, @product_id, @stock, @sale_price, @subtotal)";

                            foreach (SaleDetail dv in obj.oSaleDetail)
                            {
                                SqlCommand cmdDetail = new SqlCommand(insertDetailQuery, oConnection, objTransacion);
                                cmdDetail.Parameters.AddWithValue("@sale_id", idSale);
                                cmdDetail.Parameters.AddWithValue("@product_id", dv.oProduct.idProduct);
                                cmdDetail.Parameters.AddWithValue("@stock", dv.amount);
                                cmdDetail.Parameters.AddWithValue("@sale_price", dv.salePrice);
                                cmdDetail.Parameters.AddWithValue("@subtotal", dv.subtotal);
                                cmdDetail.ExecuteNonQuery();
                            }

                            objTransacion.Commit();
                            result = idSale;
                        }
                        else
                        {
                            objTransacion.Rollback();
                            result = 0;
                        }

                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e);
                        objTransacion.Rollback();
                        result = 0;
                    }

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    result = 0;
                }
            }
            return result;
        }

        public List<SaleReportRow> ReportSale(string startDate, string endDate)
        {
            List<SaleReportRow> rows = new List<SaleReportRow>();

            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    StringBuilder sb = new StringBuilder();

                    sb.AppendLine("SELECT s.date_registered,s.document_type,s.document_number AS document_tribute_number,p.document_number AS document_number_person,p.name,s.document_client,s.name_client,");
                    sb.AppendLine("s.total_amount,s.amount_received,s.change_amount FROM sale s");
                    sb.AppendLine("INNER JOIN person p ON p.id = s.user_id");
                    sb.AppendLine("WHERE CAST(s.date_registered AS DATE) BETWEEN @startDate AND @endDate");

                    SqlCommand cmd = new SqlCommand(sb.ToString(), oConnection);
                    cmd.Parameters.AddWithValue("@startDate", startDate);
                    cmd.Parameters.AddWithValue("@endDate", endDate);
                    cmd.CommandType = CommandType.Text;

                    oConnection.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            rows.Add(new SaleReportRow
                            {
                                DateRegistered = Convert.ToDateTime(dr["date_registered"]),
                                DocumentType = dr["document_type"].ToString(),
                                DocumentNumber = dr["document_tribute_number"].ToString(),
                                SellerDocument = dr["document_number_person"].ToString(),
                                SellerName = dr["name"].ToString(),
                                ClientDocument = dr["document_client"].ToString(),
                                ClientName = dr["name_client"].ToString(),
                                TotalAmount = Convert.ToDecimal(dr["total_amount"]),
                                AmountReceived = Convert.ToDecimal(dr["amount_received"]),
                                ChangeAmount = Convert.ToDecimal(dr["change_amount"])
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    rows = new List<SaleReportRow>();
                }
            }
            return rows;
        }

        public decimal SumTotalPay(string startDate, string endDate)
        {
            decimal sum_obj;
            DataTable dt = new DataTable();
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("SELECT ISNULL(SUM(s.total_amount), 0) AS total_amount");
                    sb.AppendLine("FROM sale s");
                    sb.AppendLine("INNER JOIN person p ON p.id = s.user_id");
                    sb.AppendLine("WHERE CAST(s.date_registered AS DATE) BETWEEN @startDate AND @endDate");

                    SqlCommand cmd = new SqlCommand(sb.ToString(), oConnection);
                    cmd.Parameters.AddWithValue("@startDate", startDate);
                    cmd.Parameters.AddWithValue("@endDate", endDate);
                    cmd.CommandType = CommandType.Text;

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                        sum_obj = Convert.ToDecimal(dt.Rows[0]["total_amount"]);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    sum_obj = 0;
                }
            }
            return sum_obj;
        }

        public decimal SumAmountReceived(string startDate, string endDate)
        {
            decimal sum_obj;
            DataTable dt = new DataTable();
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("SELECT ISNULL(SUM(s.amount_received), 0) AS amount_received");
                    sb.AppendLine("FROM sale s");
                    sb.AppendLine("INNER JOIN person p ON p.id = s.user_id");
                    sb.AppendLine("WHERE CAST(s.date_registered AS DATE) BETWEEN @startDate AND @endDate");

                    SqlCommand cmd = new SqlCommand(sb.ToString(), oConnection);
                    cmd.Parameters.AddWithValue("@startDate", startDate);
                    cmd.Parameters.AddWithValue("@endDate", endDate);
                    cmd.CommandType = CommandType.Text;

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                        sum_obj = Convert.ToDecimal(dt.Rows[0]["amount_received"]);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    sum_obj = 0;
                }
            }
            return sum_obj;
        }

        public decimal SumChangeAmount(string startDate, string endDate)
        {
            decimal sum_obj;
            DataTable dt = new DataTable();
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("SELECT ISNULL(SUM(s.change_amount), 0) AS change_amount");
                    sb.AppendLine("FROM sale s");
                    sb.AppendLine("INNER JOIN person p ON p.id = s.user_id");
                    sb.AppendLine("WHERE CAST(s.date_registered AS DATE) BETWEEN @startDate and @endDate");

                    SqlCommand cmd = new SqlCommand(sb.ToString(), oConnection);
                    cmd.Parameters.AddWithValue("@startDate", startDate);
                    cmd.Parameters.AddWithValue("@endDate", endDate);
                    cmd.CommandType = CommandType.Text;

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                        sum_obj = Convert.ToDecimal(dt.Rows[0]["change_amount"]);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    sum_obj = 0;
                }
            }
            return sum_obj;
        }
    }
}
