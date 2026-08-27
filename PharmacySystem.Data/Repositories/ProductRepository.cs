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
    public class ProductRepository : IProductRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public ProductRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public int Register(Product obj)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("code", obj.code);
                    parameters.Add("name", obj.name);
                    parameters.Add("description", obj.description);
                    parameters.Add("category_id", obj.oCategory.IdCategory);
                    parameters.Add("result", dbType: DbType.Int32, direction: ParameterDirection.Output);

                    oConnection.Execute("sp_create_product", parameters, commandType: CommandType.StoredProcedure);

                    return parameters.Get<int>("result");
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return 0;
                }
            }
        }

        public bool Update(Product obj)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("id_product", obj.idProduct);
                    parameters.Add("code", obj.code);
                    parameters.Add("name", obj.name);
                    parameters.Add("description", obj.description);
                    parameters.Add("category_id", obj.oCategory.IdCategory);
                    parameters.Add("result", dbType: DbType.Boolean, direction: ParameterDirection.Output);

                    oConnection.Execute("sp_update_product", parameters, commandType: CommandType.StoredProcedure);

                    return parameters.Get<bool>("result");
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return false;
                }
            }
        }

        public List<Product> List()
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    // date_expired is left un-aliased to a matching name only when NULL - Dapper
                    // skips the assignment for a DBNull cell rather than throwing, so a product
                    // with no expiration date keeps expirationDate at its default(DateTime), the
                    // same 01/01/0001 that Convert.ToDateTime(null) produced in the original code.
                    const string sql =
                        "SELECT p.id AS idProduct, p.code, p.name, p.description AS description, p.stock, " +
                        "p.purchase_price AS purchasePrice, p.sale_price AS salePrice, p.date_expired AS expirationDate, " +
                        "c.id AS IdCategory, c.description AS description " +
                        "FROM product p INNER JOIN category c ON c.id = p.category_id " +
                        "WHERE p.status = 1";

                    return oConnection.Query<Product, Categories, Product>(
                        sql,
                        (product, category) => { product.oCategory = category; return product; },
                        splitOn: "IdCategory")
                        .ToList();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return new List<Product>();
                }
            }
        }

        public bool Verify(int idProduct)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    int count = oConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM product WHERE id = @idProduct", new { idProduct });
                    return count > 0;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return false;
                }
            }
        }

        public bool Delete(int idProduct)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("id_product", idProduct);
                    parameters.Add("result", dbType: DbType.Boolean, direction: ParameterDirection.Output);

                    oConnection.Execute("sp_delete_product", parameters, commandType: CommandType.StoredProcedure);

                    return parameters.Get<bool>("result");
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return false;
                }
            }
        }

        public List<ProductReportRow> Report(string categoryId)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    const string sql =
                        "SELECT p.date_created AS DateCreated, p.code AS Code, p.name AS Name, p.description AS Description, " +
                        "c.description AS CategoryDescription, p.stock AS Stock, p.purchase_price AS PurchasePrice, " +
                        "p.sale_price AS SalePrice, p.date_expired AS DateExpired, s.name AS StatusName " +
                        "FROM product p INNER JOIN category c ON c.id = p.category_id " +
                        "INNER JOIN state_product s ON s.id = p.status " +
                        "WHERE c.id = case @category_id when '0' then c.id when 0 then c.id else @category_id end";

                    return oConnection.Query<ProductReportRow>(sql, new { category_id = categoryId }).ToList();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return new List<ProductReportRow>();
                }
            }
        }
    }
}
