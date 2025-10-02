using PharmacySystem.Helpers;
using PharmacySystem.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacySystem.Logical
{
    public class SaleService
    {
        private static SaleService instance = null;

        public SaleService()
        {

        }

        public static SaleService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SaleService();
                }
                return instance;
            }
        }

        public List<Sale> ListSale() {
            List<Sale> List = new List<Sale>();

            using (SqlConnection oConnection = new SqlConnection(Connection.CN))
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
                    List = new List<Sale>();
                }
            }
            return List;
        }


        public List<SaleDetail> ListSaleDetail()
        {
            List<SaleDetail> List = new List<SaleDetail>();

            using (SqlConnection oConnection = new SqlConnection(Connection.CN))
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
                    List = new List<SaleDetail>();
                }
            }
            return List;
        }


        public bool ControlStock(int idproduct,int amount, bool subtract)
        {
            bool result = true;
            using (SqlConnection oConnection = new SqlConnection(Connection.CN))
            {
                try
                {
                    string query = string.Format("UPDATE product SET stock = (stock {0} {1}) WHERE id = {2}", subtract ? "-" : "+", amount, idproduct);
                    SqlCommand cmd = new SqlCommand(query.ToString(), oConnection);
                    cmd.CommandType = CommandType.Text;
                    oConnection.Open();
                    cmd.ExecuteNonQuery();
                    result = true;
                }
                catch (Exception ex)
                {
                    result = false;
                }
            }

            return result;

        }


        public int RegisterSale(Sale obj)
        {
            int result = 0;

            using (SqlConnection oConnection = new SqlConnection(Connection.CN))
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    oConnection.Open();
                    SqlTransaction objTransacion = oConnection.BeginTransaction();


                    sb.AppendLine("DECLARE @sale_id int = 0");
                    sb.AppendLine(string.Format("INSERT INTO sale(document_type,document_number,user_id,document_client,name_client, total_amount, amount_received, change_amount) VALUES('{0}',({1}),{2},'{3}','{4}','{5}','{6}','{7}')"
                        , obj.typeDocument
                        , "SELECT RIGHT('000000' + CAST((SELECT count(*) + 1 FROM sale) AS VARCHAR), 6)"
                        , obj.oPerson.idPerson
                        , obj.documentClient
                        , obj.nameClient
                        , CultureInfoHelper.CultureInfoConverterDecimal(obj.totalPay)
                        , CultureInfoHelper.CultureInfoConverterDecimal(obj.payWith)
                        , CultureInfoHelper.CultureInfoConverterDecimal(obj.change)));

                    sb.AppendLine("SET @sale_id = SCOPE_IDENTITY()");
                    foreach (SaleDetail dv in obj.oSaleDetail)
                    {
                        sb.AppendLine(string.Format("INSERT INTO sale_detail(sale_id, product_id, stock, sale_price, subtotal) values({0},{1},{2},'{3}','{4}')",
                            "@sale_id", dv.oProduct.idProduct, dv.amount, CultureInfoHelper.CultureInfoConverterDecimal(dv.salePrice), CultureInfoHelper.CultureInfoConverterDecimal(dv.subtotal)));
                    }
                    sb.AppendLine("SELECT @sale_id");

                    SqlCommand cmd = new SqlCommand(sb.ToString(), oConnection);
                    cmd.CommandType = CommandType.Text;
                    cmd.Transaction = objTransacion;
                    try
                    {
                        int idSale = 0;
                        int.TryParse(cmd.ExecuteScalar().ToString(), out idSale);

                        if (idSale != 0)
                        {
                            objTransacion.Commit();
                            result = idSale;
                        }
                        else
                        {
                            objTransacion.Rollback();
                            result = idSale;
                        }

                    }
                    catch (Exception e)
                    {
                        objTransacion.Rollback();
                        result = 0;
                    }

                }
                catch (Exception ex)
                {
                    result = 0;
                }
            }
            return result;
        }

        public DataTable ReportSale(string startDate , string endDate)
        {
            DataTable dt = new DataTable();
            DataTable dtFinal = new DataTable();
 
            dtFinal.Columns.Add("Fecha Venta", typeof(string));
            dtFinal.Columns.Add("Tipo Documento", typeof(string));
            dtFinal.Columns.Add("Numero Documento", typeof(string));
            dtFinal.Columns.Add("CI Vendedor", typeof(string));
            dtFinal.Columns.Add("Nombre Vendedor", typeof(string));
            dtFinal.Columns.Add("CI Cliente", typeof(string));
            dtFinal.Columns.Add("Nombre Cliente", typeof(string));
            dtFinal.Columns.Add("Total Pagar", typeof(string));
            dtFinal.Columns.Add("Pago Con", typeof(string));
            dtFinal.Columns.Add("Cambio", typeof(string));


            using (SqlConnection oConnection = new SqlConnection(Connection.CN))
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

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd)) {
                        //fill the datatable with sql query data
                        da.Fill(dt);
                        foreach (DataRow row in dt.Rows)
                        {
                            string dateRegister = DateHelper.FormatDatePresentation(Convert.ToDateTime(row["date_registered"]));
                            string typeDocument = row["document_type"].ToString();
                            string numberDocument = row["document_tribute_number"].ToString();
                            string idDocument = row["document_number_person"].ToString();
                            string nameVendor = row["name"].ToString();
                            string idDocumentClient = row["document_client"].ToString();
                            string nameClient = row["name_client"].ToString();
                            string totalPay = CultureInfoHelper.FormatAsEcuadorCurrency(Convert.ToDecimal(row["total_amount"]));
                            string amountReceived = CultureInfoHelper.FormatAsEcuadorCurrency(Convert.ToDecimal(row["amount_received"]));
                            string changeAmount = CultureInfoHelper.FormatAsEcuadorCurrency(Convert.ToDecimal(row["change_amount"]));

                            dtFinal.Rows.Add(dateRegister, typeDocument,
                                            numberDocument, idDocument,
                                            nameVendor, idDocumentClient,
                                            nameClient, totalPay,
                                            amountReceived, changeAmount);

                        }



                    }
                }
                catch (Exception ex)
                {
                    dt = new DataTable();
                    dtFinal = new DataTable();
                }
            }
            return dtFinal;

        }

        //SUMATORIA
        public decimal SumTotalPay(string startDate, string endDate)
        {
            decimal sum_obj;
            DataTable dt = new DataTable();
            using (SqlConnection oConnection = new SqlConnection(Connection.CN))
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
                    sum_obj = 0;
                }
            }
            return sum_obj;




        }
        public decimal SumAmountReceived(string startDate, string endDate)
        {
            decimal sum_obj;
            DataTable dt = new DataTable();
            using (SqlConnection oConnection = new SqlConnection(Connection.CN))
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
                    sum_obj = 0;
                }
            }
            return sum_obj;




        }
        public decimal SumChangeAmount(string startDate, string endDate)
        {
            decimal sum_obj;
            DataTable dt = new DataTable();
            using (SqlConnection oConnection = new SqlConnection(Connection.CN))
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
                    sum_obj = 0;
                }
            }
            return sum_obj;
        }
     
    }
}
