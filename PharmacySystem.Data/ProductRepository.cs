using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using PharmacySystem.Helpers;
using PharmacySystem.Model;

namespace PharmacySystem.Data
{
    public class ProductRepository : IProductRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public ProductRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public int Register(Product obj)
        {
            int result = 0;
            using (SqlConnection oConnection = _connectionFactory.Create())
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
                    Logger.LogError(ex);
                    result = 0;
                }
            }
            return result;
        }

        public bool Update(Product obj)
        {
            bool result = true;
            using (SqlConnection oConnection = _connectionFactory.Create())
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
                    Logger.LogError(ex);
                    result = false;
                }

            }

            return result;
        }

        public List<Product> List()
        {
            List<Product> List = new List<Product>();
            using (SqlConnection oConnection = _connectionFactory.Create())
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
                    Logger.LogError(ex);

                    List = new List<Product>();
                }
            }
            return List;
        }

        public bool Verify(int idProduct)
        {
            bool result = false;
            using (SqlConnection oConnection = _connectionFactory.Create())
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
                    Logger.LogError(ex);

                    result = false;
                }
            }
            return result;
        }

        public bool Delete(int idProduct)
        {
            bool result = true;
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("sp_delete_product", oConnection);
                    cmd.Parameters.AddWithValue("@id_product", idProduct);
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

        public List<ProductReportRow> Report(string categoryId)
        {
            List<ProductReportRow> rows = new List<ProductReportRow>();
            using (SqlConnection oConnection = _connectionFactory.Create())
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
                    cmd.Parameters.AddWithValue("@category_id", categoryId);
                    cmd.CommandType = CommandType.Text;

                    oConnection.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            rows.Add(new ProductReportRow
                            {
                                DateCreated = Convert.ToDateTime(dr["date_created"]),
                                Code = dr["code"].ToString(),
                                Name = dr["product_name"].ToString(),
                                Description = dr["description_product"].ToString(),
                                CategoryDescription = dr["description_category"].ToString(),
                                Stock = Convert.ToInt32(dr["stock"]),
                                PurchasePrice = Convert.ToDecimal(dr["purchase_price"]),
                                SalePrice = Convert.ToDecimal(dr["sale_price"]),
                                DateExpired = Convert.ToDateTime(dr["date_expired"]),
                                StatusName = dr["status_name"].ToString()
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    rows = new List<ProductReportRow>();
                }
            }
            return rows;
        }
    }
}
