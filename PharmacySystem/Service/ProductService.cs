using DocumentFormat.OpenXml.Office2010.Excel;
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
    public class ProductService
    {

        private static ProductService instance = null;

        public ProductService()
        {

        }

        public static ProductService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ProductService();
                }

                return instance;
            }
        }


        public int RegisterProduct(Product obj)
        {
            int result = 0;
            using (SqlConnection oConnection = new SqlConnection(Connection.CN))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("sp_create_product", oConnection);
                    cmd.Parameters.AddWithValue("code", obj.code);
                    cmd.Parameters.AddWithValue("name", obj.name);
                    cmd.Parameters.AddWithValue("description", obj.description);
                    cmd.Parameters.AddWithValue("category_id", obj.oCategory.IdCategory);
                    cmd.Parameters.Add("result", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;

                    oConnection.Open();

                    cmd.ExecuteNonQuery();

                    result = Convert.ToInt32(cmd.Parameters["result"].Value);

                }
                catch (Exception ex)
                {
                    result = 0;
                }
            }
            return result;
        }

        public bool UpdateProduct(Product obj)
        {
            bool result = true;
            using (SqlConnection oConnection = new SqlConnection(Connection.CN))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("sp_update_product", oConnection);
                    cmd.Parameters.AddWithValue("id_product", obj.idProduct);
                    cmd.Parameters.AddWithValue("code", obj.code);
                    cmd.Parameters.AddWithValue("name", obj.name);
                    cmd.Parameters.AddWithValue("description", obj.description);
                    cmd.Parameters.AddWithValue("category_id", obj.oCategory.IdCategory);
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


        public List<Product> ListProduct()
        {
            List<Product> List = new List<Product>();
            using (SqlConnection oConnection = new SqlConnection(Connection.CN))
            {
                try
                {
                    StringBuilder sb = new StringBuilder();

                    sb.AppendLine("SELECT p.id,p.code,p.name,p.description AS description_product,p.category_id,c.description");
                    sb.AppendLine("AS description_category,p.stock,p.purchase_price,p.sale_price,p.date_expired FROM product p");
                    sb.AppendLine("INNER JOIN category c on c.id = p.category_id");
                    sb.AppendLine("WHERE p.status = 1");


                    SqlCommand cmd = new SqlCommand(sb.ToString(), oConnection);
                    cmd.CommandType = CommandType.Text;
                    oConnection.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            var date = "";
                            if (dr["date_expired"] == DBNull.Value)
                            {
                                date = null;
                            }
                            else
                            {
                                date = dr["date_expired"].ToString();
                            }
                            List.Add(new Product()
                            {
                                idProduct = Convert.ToInt32(dr["id"]),
                                code = dr["code"].ToString(),
                                name = dr["name"].ToString(),
                                description = dr["description_product"].ToString(),
                                oCategory = new Categories() { IdCategory = Convert.ToInt32(dr["category_id"]), 
                                                               description = dr["description_category"].ToString() },
                                stock = Convert.ToInt32(dr["stock"]),
                                purchasePrice = Convert.ToDecimal(dr["purchase_price"]),
                                salePrice = Convert.ToDecimal(dr["sale_price"]),
                                expirationDate = Convert.ToDateTime(date)

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

        public bool VerifyProduct(int idProduct)
        {
            bool result = false;
            using (SqlConnection oConnection = new SqlConnection(Connection.CN))
            {
                try
                {
                    StringBuilder sb = new StringBuilder();

                    sb.AppendLine("SELECT COUNT(*) FROM product WHERE id = @idProduct");
     


                    SqlCommand cmd = new SqlCommand(sb.ToString(), oConnection);
                    cmd.Parameters.AddWithValue("@idProduct", idProduct);
                    cmd.CommandType = CommandType.Text;
                    oConnection.Open();
                    cmd.ExecuteNonQuery();
                    int count = (int)cmd.ExecuteScalar();
                    result = count > 0 ? true : false;
                }

                catch (Exception ex)
                {

                    result = false;
                }



            }
            return result;
        }


        public bool DeleteProduct(int id)
        {
            bool result = true;
            using (SqlConnection oConnection = new SqlConnection(Connection.CN))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("sp_delete_product", oConnection);
                    cmd.Parameters.AddWithValue("@id_product", id);
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

        public DataTable Report(string idcategory)
        {
            DataTable dt = new DataTable();
            DataTable dtFinal = new DataTable();

            dtFinal.Columns.Add("Fecha Registro", typeof(string));
            dtFinal.Columns.Add("Codigo", typeof(string));
            dtFinal.Columns.Add("Nombre", typeof(string));
            dtFinal.Columns.Add("Descripcion", typeof(string));
            dtFinal.Columns.Add("Categoria", typeof(string));
            dtFinal.Columns.Add("Stock", typeof(string));
            dtFinal.Columns.Add("Precio Compra", typeof(string));
            dtFinal.Columns.Add("Precio Venta", typeof(string));
            dtFinal.Columns.Add("Fecha Vencimiento", typeof(string));
            dtFinal.Columns.Add("Estado", typeof(string));



            using (SqlConnection oConnection = new SqlConnection(Connection.CN))
            {
                try
                {

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("SELECT p.date_created, p.code,p.name AS product_name,p.description AS description_product,c.description");
                    sb.AppendLine(" AS description_category ,p.stock,p.purchase_price,p.sale_price, p.date_expired,s.name AS status_name ");
                    sb.AppendLine("FROM product p INNER JOIN category c on c.id = p.category_id");
                    sb.AppendLine("INNER JOIN state_product s on s.id = p.status");
                    sb.AppendLine("WHERE c.id = case @category_id when '0' then c.id when 0 then c.id else @category_id end");
                    sb.AppendLine("and p.date_expired IS NOT NULL");

                    SqlCommand cmd = new SqlCommand(sb.ToString(), oConnection);
                    cmd.Parameters.AddWithValue("@category_id", idcategory);
                    cmd.CommandType = CommandType.Text;

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                        foreach (DataRow row in dt.Rows)
                        {
                            string createdDate = DateHelper.FormatDatePresentation(Convert.ToDateTime(row["date_created"]));
                            string codeProduct = row["code"].ToString();
                            string nameProduct = row["product_name"].ToString();
                            string descriptionProduct = row["description_product"].ToString();
                            string categoryDescription = row["description_category"].ToString();
                            string stockProduct = row["stock"].ToString();
                            string pricePurchase = CultureInfoHelper.FormatAsEcuadorCurrency(Convert.ToDecimal(row["purchase_price"]));
                            string priceSales = CultureInfoHelper.FormatAsEcuadorCurrency(Convert.ToDecimal(row["sale_price"]));
                            string expirationDate = DateHelper.FormatDatePresentation(Convert.ToDateTime(row["date_expired"]));
                            string state = row["status_name"].ToString();


                            dtFinal.Rows.Add( createdDate, codeProduct, 
                                            nameProduct, descriptionProduct, 
                                            categoryDescription, stockProduct, 
                                            pricePurchase, priceSales, 
                                            expirationDate,state);

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


    }
}
