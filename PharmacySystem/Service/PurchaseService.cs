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
    public class PurchaseService
    {
        private static PurchaseService _instance = null;

        public PurchaseService()
        {

        }

        public static PurchaseService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PurchaseService();
                }

                return _instance;
            }
        }


        public bool RegisterPurchase(Purchase purchase)
        {
            bool result = true;

            using (SqlConnection oConnection = new SqlConnection(Connection.CN))
            {
                try
                {
                    StringBuilder sb = new StringBuilder();

                    oConnection.Open();

                    SqlTransaction objTransacion = oConnection.BeginTransaction();


                    sb.AppendLine("DECLARE @id_purchase int = 0");
                    sb.AppendLine(string.Format("INSERT INTO purchase(person_id,supplier_id, total_amount, document_type, document_number) values({0}, {1}, '{2}', '{3}','{4}')"
                        ,purchase.oPerson.idPerson, purchase.oSupplier.idSupplier, CultureInfoHelper.CultureInfoConverterDecimal(purchase.totalAmount), purchase.documentType, purchase.documentNumber));
                        
                    sb.AppendLine("SET @id_purchase = SCOPE_IDENTITY()");
                    foreach (PurchaseDetail pd in purchase.oPurchaseDetail)
                    {
                        sb.AppendLine(string.Format("INSERT INTO purchase_detail(purchase_id,product_id,stock,purchase_price, sale_price, total_amount) values({0},{1},{2},'{3}','{4}','{5}')", 
                            "@id_purchase",pd.oProduct.idProduct,pd.quantity,CultureInfoHelper.CultureInfoConverterDecimal(pd.purchasePrice), 
                            CultureInfoHelper.CultureInfoConverterDecimal(pd.salePrice), CultureInfoHelper.CultureInfoConverterDecimal(pd.total)));

                        sb.AppendLine(string.Format("UPDATE product SET stock = (stock + {0}) , purchase_price = '{1}', sale_price = '{2}' , date_expired = '{3}' WHERE id = {4}", 
                            pd.quantity, CultureInfoHelper.CultureInfoConverterDecimal(pd.purchasePrice),CultureInfoHelper.CultureInfoConverterDecimal(pd.salePrice),DateHelper.FormatDateBackend(pd.expirationDate),pd.oProduct.idProduct));

                    }
                    sb.AppendLine("SELECT @id_purchase");

                    SqlCommand cmd = new SqlCommand(sb.ToString(), oConnection);
                    cmd.CommandType = CommandType.Text;
                    cmd.Transaction = objTransacion;
                    try
                    {
                        int idPurchase = 0; 
                        int.TryParse(cmd.ExecuteScalar().ToString(), out idPurchase);
                 
                        if (idPurchase != 0)
                        {
                            objTransacion.Commit();
                            result = true;
                        }
                        else
                        {
                            objTransacion.Rollback();
                            result = false;
                        }

                    }
                            catch (Exception e)
                    {
                        objTransacion.Rollback();
                        result = false;
                    }

                }
                catch (Exception ex)
                {
                    result = false;
                }
            }
            return result;
        }
        

        public DataTable ReportPurchase(string idSupplier, string startDate, string endDate)
        {
            DataTable dt = new DataTable();
            DataTable dtFinal = new DataTable();
            dtFinal.Columns.Add("Fecha Compra", typeof(string));
            dtFinal.Columns.Add("RUC", typeof(string));
            dtFinal.Columns.Add("Razon Social", typeof(string));
            dtFinal.Columns.Add("Tipo Documento", typeof(string));
            dtFinal.Columns.Add("Numero Documento", typeof(string));
            dtFinal.Columns.Add("Monto Total", typeof(string));
            dtFinal.Columns.Add("Nombre,", typeof(string));
            dtFinal.Columns.Add("Cantidad", typeof(string));
            dtFinal.Columns.Add("Precio Compra", typeof(string));
            dtFinal.Columns.Add("Precio Venta", typeof(string));
            using (SqlConnection oConnection = new SqlConnection(Connection.CN))
            {
                try
                {
                     StringBuilder sb = new StringBuilder();

                    sb.AppendLine("SELECT pu.date_registered,su.document_number AS document_number_supplier,su.company_name,");
                    sb.AppendLine("pu.document_type,pu.document_number AS document_number_employee,pu.total_amount,");
                    sb.AppendLine("pr.name,pd.stock,pd.purchase_price,pd.sale_price");
                    sb.AppendLine("FROM purchase pu");
                    sb.AppendLine("INNER JOIN supplier su ON su.id = pu.supplier_id");
                    sb.AppendLine("INNER JOIN purchase_detail pd ON pd.purchase_id = pu.id");
                    sb.AppendLine("INNER JOIN product pr on pr.id = pd.product_id");
                    sb.AppendLine("WHERE CAST(pu.date_registered AS DATE) BETWEEN @startDate and @endDate");
                    sb.AppendLine("and pu.supplier_id =  CASE @supplier_id WHEN '0' THEN pu.supplier_id ");
                    sb.AppendLine("WHEN 0 THEN pu.supplier_id ELSE @supplier_id END");

                    SqlCommand cmd = new SqlCommand(sb.ToString(), oConnection);
                    cmd.Parameters.AddWithValue("@startDate", startDate);
                    cmd.Parameters.AddWithValue("@endDate", endDate);
                    cmd.Parameters.AddWithValue("@supplier_id", idSupplier);
                    cmd.CommandType = CommandType.Text;

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                        foreach (DataRow row in dt.Rows) 
                        {
                            string datePurchase = DateHelper.FormatDatePresentation(Convert.ToDateTime(row["date_registered"]));
                            string idSupplierTemp = row["document_number_supplier"].ToString();
                            string companyName = row["company_name"].ToString();
                            string typeDocument = row["document_type"].ToString();
                            string numberDocument = row["document_number_employee"].ToString();
                            string totalAmount = CultureInfoHelper.FormatAsEcuadorCurrency(Convert.ToDecimal(row["total_amount"]));
                            string nameProduct = row["name"].ToString();
                            string quantity = row["stock"].ToString();
                            string pricePurchase = CultureInfoHelper.FormatAsEcuadorCurrency(Convert.ToDecimal(row["purchase_price"]));
                            string priceSale = CultureInfoHelper.FormatAsEcuadorCurrency(Convert.ToDecimal(row["sale_price"]));

                            dtFinal.Rows.Add( datePurchase, idSupplierTemp, 
                                            companyName, typeDocument, 
                                            numberDocument, totalAmount, 
                                            nameProduct, quantity, 
                                            pricePurchase, priceSale);

                           
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

        //Summations
        public decimal GetTotalAmount(string idSupplier, string startDate, string endDate)
        {
            decimal sum_obj;
            DataTable tl = new DataTable();
            using (SqlConnection oConnection = new SqlConnection(Connection.CN))
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("SET DATEFORMAT dmy");
                    sb.AppendLine("SELECT ISNULL(SUM(pu.total_amount),0) AS total_amount");
                    sb.AppendLine("FROM purchase pu");
                    sb.AppendLine("INNER JOIN supplier su ON su.id = pu.supplier_id");
                    sb.AppendLine("INNER JOIN purchase_detail pd ON pd.purchase_id = pu.id");
                    sb.AppendLine("INNER JOIN product pr ON pr.id = pd.product_id");
                    sb.AppendLine("WHERE CONVERT(DATE, pu.date_registered) BETWEEN @startDate AND @endDate");
                    sb.AppendLine("AND (@supplier_id = 0 OR pu.supplier_id = @supplier_id)");

                    SqlCommand cmd = new SqlCommand(sb.ToString(), oConnection);
                    cmd.Parameters.AddWithValue("@startDate", startDate);
                    cmd.Parameters.AddWithValue("@endDate", endDate);
                    cmd.Parameters.AddWithValue("@supplier_id", idSupplier);
                    cmd.CommandType = CommandType.Text;

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(tl);
                        sum_obj = Convert.ToDecimal(tl.Rows[0]["total_amount"]);
                    }

            }
            
            catch (Exception ex)
            {
                sum_obj = 0;
            }
        }
            return sum_obj;

        }
        public decimal GetTotalPurchasePrice(string idSupplier, string startDate, string endDate)
        {
            decimal sum_obj;
            DataTable tl = new DataTable();
            using (SqlConnection oConnection = new SqlConnection(Connection.CN))
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("SET DATEFORMAT dmy");
                    sb.AppendLine("SELECT ISNULL(SUM(pd.purchase_price),0) AS purchase_price");
                    sb.AppendLine("FROM purchase pu");
                    sb.AppendLine("INNER JOIN supplier su ON su.id = pu.supplier_id");
                    sb.AppendLine("INNER JOIN purchase_detail pd ON pd.purchase_id = pu.id");
                    sb.AppendLine("INNER JOIN product pr ON pr.id = pd.product_id");
                    sb.AppendLine("WHERE CONVERT(DATE,pu.date_registered) BETWEEN @startDate AND @endDate");
                    sb.AppendLine("AND pu.supplier_id = CASE @supplier_id WHEN '0' THEN pu.supplier_id WHEN 0 THEN pu.supplier_id ELSE @supplier_id END");

                    SqlCommand cmd = new SqlCommand(sb.ToString(), oConnection);
                    cmd.Parameters.AddWithValue("@startDate", startDate);
                    cmd.Parameters.AddWithValue("@endDate", endDate);
                    cmd.Parameters.AddWithValue("@supplier_id", idSupplier);
                    cmd.CommandType = CommandType.Text;
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(tl);
                        sum_obj = Convert.ToDecimal(tl.Rows[0]["purchase_price"]);
                    }

                }
                catch (Exception ex)
                {
                    sum_obj = 0;
                }
            }
            return sum_obj;

        }

        public int GetTotalQuantity(string idSupplier, string startDate, string endDate)
        {
            int sum_obj;
            DataTable tl = new DataTable();
            using (SqlConnection oConnection = new SqlConnection(Connection.CN))
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("SET DATEFORMAT dmy");
                    sb.AppendLine("SELECT ISNULL(SUM(pd.stock), 0) AS stock");
                    sb.AppendLine("FROM purchase pu");
                    sb.AppendLine("INNER JOIN supplier su ON su.id = pu.supplier_id");
                    sb.AppendLine("INNER JOIN purchase_detail pd ON pd.purchase_id = pu.id");
                    sb.AppendLine("INNER JOIN product pr ON pr.id = pd.product_id");
                    sb.AppendLine("WHERE CONVERT(DATE,pu.date_registered) BETWEEN @startDate AND @endDate");
                    sb.AppendLine("AND pu.supplier_id = CASE @supplier_id WHEN '0' THEN pu.supplier_id WHEN 0 THEN pu.supplier_id ELSE @supplier_id END");

                    SqlCommand cmd = new SqlCommand(sb.ToString(), oConnection);
                    cmd.Parameters.AddWithValue("@startDate", startDate);
                    cmd.Parameters.AddWithValue("@endDate", endDate);
                    cmd.Parameters.AddWithValue("@supplier_id", idSupplier);
                    cmd.CommandType = CommandType.Text;
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(tl);
                        sum_obj = Convert.ToInt16(tl.Rows[0]["stock"]);
                    }

                }
                catch (Exception ex)
                {
                    sum_obj = 0;
                }
            }
            return sum_obj;

        }

        public decimal GetTotalSalesPrice(string idSupplier, string startDate, string endDate)
        {
            decimal sum_obj;
            DataTable tl = new DataTable();
            using (SqlConnection oConnection = new SqlConnection(Connection.CN))
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("SET DATEFORMAT dmy");
                    sb.AppendLine("SELECT ISNULL(SUM(pd.sale_price), 0) AS sale_price");
                    sb.AppendLine("FROM purchase pu");
                    sb.AppendLine("INNER JOIN supplier su ON su.id = pu.supplier_id");
                    sb.AppendLine("INNER JOIN purchase_detail pd ON pd.purchase_id = pu.id");
                    sb.AppendLine("INNER JOIN product pr ON pr.id = pd.product_id");
                    sb.AppendLine("WHERE CONVERT(DATE,pu.date_registered) BETWEEN @startDate AND @endDate");
                    sb.AppendLine("AND pu.supplier_id = CASE @supplier_id WHEN '0' THEN pu.supplier_id WHEN 0 THEN pu.supplier_id ELSE @supplier_id END");

                    SqlCommand cmd = new SqlCommand(sb.ToString(), oConnection);
                    cmd.Parameters.AddWithValue("@startDate", startDate);
                    cmd.Parameters.AddWithValue("@endDate", endDate);
                    cmd.Parameters.AddWithValue("@supplier_id", idSupplier);
                    cmd.CommandType = CommandType.Text;
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(tl);
                        sum_obj = Convert.ToDecimal(tl.Rows[0]["sale_price"]);
                    }

                }
                catch (Exception ex)
                {
                    sum_obj = 0;
                }
            }
            return sum_obj;

        }

        public decimal GetSubTotal(string idSupplier, string startDate, string endDate)
        {
            decimal sum_obj;
            DataTable tl = new DataTable();
            using (SqlConnection oConnection = new SqlConnection(Connection.CN))
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("SET DATEFORMAT dmy");
                    sb.AppendLine("SELECT ISNULL(SUM(pd.total_amount), 0) AS total_amount");
                    sb.AppendLine("FROM purchase pu");
                    sb.AppendLine("INNER JOIN supplier su ON su.id = pu.supplier_id");
                    sb.AppendLine("INNER JOIN purchase_detail pd ON pd.purchase_id = pu.id");
                    sb.AppendLine("INNER JOIN product pr ON pr.id = pd.product_id");
                    sb.AppendLine("WHERE CONVERT(DATE,pu.date_registered) BETWEEN @startDate AND @endDate");
                    sb.AppendLine("AND pu.supplier_id = CASE @supplier_id WHEN '0' THEN pu.supplier_id WHEN 0 THEN pu.supplier_id ELSE @supplier_id END");

                    SqlCommand cmd = new SqlCommand(sb.ToString(), oConnection);
                    cmd.Parameters.AddWithValue("@fechainicio", startDate);
                    cmd.Parameters.AddWithValue("@fechafin", endDate);
                    cmd.Parameters.AddWithValue("@idproveedor", idSupplier);
                    cmd.CommandType = CommandType.Text;
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(tl);
                        sum_obj = Convert.ToDecimal(tl.Rows[0]["total_amount"]);
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
