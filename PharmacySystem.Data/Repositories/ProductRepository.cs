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
                    const string sql =
                        "INSERT INTO product(code, name, description, category_id, tax_affected) " +
                        "VALUES (@code, @name, @description, @category_id, @tax_affected); " +
                        "SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    return oConnection.ExecuteScalar<int>(sql, new
                    {
                        code = obj.code,
                        name = obj.name,
                        description = obj.description,
                        category_id = obj.oCategory.IdCategory,
                        tax_affected = obj.taxAffected
                    });
                }
                catch (Exception ex) when (SqlErrorCodes.IsUniqueViolation(ex))
                {
                    return 0; // a product with that code already exists
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

        public bool Update(Product obj)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    const string sql =
                        "UPDATE product SET code = @code, name = @name, description = @description, " +
                        "category_id = @category_id, tax_affected = @tax_affected WHERE id = @id_product;";

                    int affected = oConnection.Execute(sql, new
                    {
                        id_product = obj.idProduct,
                        code = obj.code,
                        name = obj.name,
                        description = obj.description,
                        category_id = obj.oCategory.IdCategory,
                        tax_affected = obj.taxAffected
                    });

                    // 0 rows: the product was deleted underneath - report the failure (DEF-39).
                    return affected > 0;
                }
                catch (Exception ex) when (SqlErrorCodes.IsUniqueViolation(ex))
                {
                    return false; // another product already uses that code
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

        // Sets the sale price, releases the product for sale and records the change - all in one
        // transaction. event_type is "liberacion" the first time (the product was not released),
        // "cambio" afterwards. The cost snapshot is the product's current purchase_price.
        public bool SetSalePrice(int idProduct, decimal salePrice, string reason, int? userId)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    oConnection.Open();
                    using (SqlTransaction tx = oConnection.BeginTransaction())
                    {
                        ProductPriceSnapshot current = oConnection.QueryFirstOrDefault<ProductPriceSnapshot>(
                            "SELECT is_released, ISNULL(average_cost, purchase_price) AS cost FROM product WHERE id = @idProduct",
                            new { idProduct }, tx);

                        if (current == null)
                        {
                            tx.Rollback();
                            return false;
                        }

                        oConnection.Execute(
                            "UPDATE product SET sale_price = @salePrice, is_released = 1 WHERE id = @idProduct",
                            new { idProduct, salePrice }, tx);

                        string eventType = current.is_released ? "cambio" : "liberacion";

                        oConnection.Execute(
                            "INSERT INTO product_price_history(product_id, event_type, sale_price, cost, user_id, reason) " +
                            "VALUES (@idProduct, @eventType, @salePrice, @cost, @userId, @reason)",
                            new { idProduct, eventType, salePrice, cost = current.cost, userId, reason }, tx);

                        tx.Commit();
                        return true;
                    }
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

        public bool Unrelease(int idProduct, string reason, int? userId)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    oConnection.Open();
                    using (SqlTransaction tx = oConnection.BeginTransaction())
                    {
                        int rows = oConnection.Execute(
                            "UPDATE product SET is_released = 0 WHERE id = @idProduct AND is_released = 1",
                            new { idProduct }, tx);

                        if (rows == 0)
                        {
                            tx.Rollback();
                            return false;
                        }

                        oConnection.Execute(
                            "INSERT INTO product_price_history(product_id, event_type, sale_price, cost, user_id, reason) " +
                            "SELECT id, 'retiro', ISNULL(sale_price, 0), ISNULL(average_cost, purchase_price), @userId, @reason FROM product WHERE id = @idProduct",
                            new { idProduct, userId, reason }, tx);

                        tx.Commit();
                        return true;
                    }
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

        public List<ProductPriceHistoryEntry> GetPriceHistory(int idProduct)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    const string sql =
                        "SELECT h.changed_at AS ChangedAt, h.event_type AS EventType, h.sale_price AS SalePrice, " +
                        "h.cost AS Cost, pe.name AS UserName, h.reason AS Reason " +
                        "FROM product_price_history h LEFT JOIN person pe ON pe.id = h.user_id " +
                        "WHERE h.product_id = @idProduct ORDER BY h.changed_at DESC, h.id DESC";

                    return oConnection.Query<ProductPriceHistoryEntry>(sql, new { idProduct }).ToList();
                }
                catch (SqlException ex) when (SqlErrorCodes.IsConnectivityError(ex))
                {
                    Logger.LogError(ex);
                    throw new DataUnavailableException(DataUnavailableException.DefaultMessage, ex);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return new List<ProductPriceHistoryEntry>();
                }
            }
        }

        // date_expired is left un-aliased to a matching name only when NULL - Dapper skips the
        // assignment for a DBNull cell rather than throwing, so a product with no expiration date
        // keeps expirationDate at its default(DateTime), the same 01/01/0001 that
        // Convert.ToDateTime(null) produced in the original code.
        private const string ProductSelect =
            "SELECT p.id AS idProduct, p.code, p.name, p.description AS description, p.stock, " +
            "p.purchase_price AS purchasePrice, p.average_cost AS averageCost, p.sale_price AS salePrice, " +
            "p.tax_affected AS taxAffected, p.is_released AS isReleased, p.date_expired AS expirationDate, " +
            "c.id AS IdCategory, c.description AS description " +
            "FROM product p INNER JOIN category c ON c.id = p.category_id " +
            "WHERE p.status = 1";

        private class ProductPriceSnapshot
        {
            public bool is_released { get; set; }
            public decimal? cost { get; set; }
        }

        // The lots of one product with stock left, earliest expiry first (undated last).
        public List<ProductLot> GetLots(int idProduct)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    const string sql =
                        "SELECT id, product_id AS productId, purchase_detail_id AS purchaseDetailId, quantity, " +
                        "date_expired AS dateExpired, unit_cost AS unitCost, received_at AS receivedAt " +
                        "FROM product_lot WHERE product_id = @idProduct AND quantity > 0 " +
                        "ORDER BY CASE WHEN date_expired IS NULL THEN 1 ELSE 0 END, date_expired, received_at, id";

                    return oConnection.Query<ProductLot>(sql, new { idProduct }).ToList();
                }
                catch (SqlException ex) when (SqlErrorCodes.IsConnectivityError(ex))
                {
                    Logger.LogError(ex);
                    throw new DataUnavailableException(DataUnavailableException.DefaultMessage, ex);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return new List<ProductLot>();
                }
            }
        }

        public List<Product> List() => QueryProducts(ProductSelect);

        // One page of active products, optionally filtered by a text that matches code, name or
        // description. The management grid used to load every product and filter in memory.
        // Count and page come back from a single command.
        public PagedResult<Product> ListPaged(int pageNumber, int pageSize, string search)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = PagedResult<Product>.DefaultPageSize;

            const string filter =
                " AND (@search = '' OR p.code LIKE @like OR p.name LIKE @like OR p.description LIKE @like)";

            string sql =
                "SELECT COUNT(*) FROM product p INNER JOIN category c ON c.id = p.category_id " +
                "WHERE p.status = 1" + filter + ";" +
                ProductSelect + filter +
                " ORDER BY p.name, p.id OFFSET @offset ROWS FETCH NEXT @take ROWS ONLY;";

            string term = (search ?? string.Empty).Trim();
            var param = new
            {
                search = term,
                like = "%" + term + "%",
                offset = (pageNumber - 1) * pageSize,
                take = pageSize
            };

            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    using (SqlMapper.GridReader multi = oConnection.QueryMultiple(sql, param))
                    {
                        int total = multi.ReadFirst<int>();
                        var items = multi.Read<Product, Categories, Product>(
                            (product, category) => { product.oCategory = category; return product; },
                            splitOn: "IdCategory")
                            .ToList();

                        return new PagedResult<Product>
                        {
                            Items = items,
                            TotalCount = total,
                            PageNumber = pageNumber,
                            PageSize = pageSize
                        };
                    }
                }
                catch (SqlException ex) when (SqlErrorCodes.IsConnectivityError(ex))
                {
                    Logger.LogError(ex);
                    throw new DataUnavailableException(DataUnavailableException.DefaultMessage, ex);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return PagedResult<Product>.Empty(pageSize);
                }
            }
        }

        public List<Product> ListSellable() => QueryProducts(ProductSelect + " AND p.is_released = 1");

        // One sellable product by code / id - the sale screen used to load the whole catalogue
        // and filter in memory on every scan and every add-to-cart (RNF-REN-01 / DEF-13).
        public Product GetSellableByCode(string code) =>
            QueryProducts(ProductSelect + " AND p.is_released = 1 AND p.code = @code", new { code }).FirstOrDefault();

        public Product GetSellableById(int idProduct) =>
            QueryProducts(ProductSelect + " AND p.is_released = 1 AND p.id = @idProduct", new { idProduct }).FirstOrDefault();

        private List<Product> QueryProducts(string sql, object param = null)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    return oConnection.Query<Product, Categories, Product>(
                        sql,
                        (product, category) => { product.oCategory = category; return product; },
                        param,
                        splitOn: "IdCategory")
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

        public bool Verify(int idProduct)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    int count = oConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM product WHERE id = @idProduct", new { idProduct });
                    return count > 0;
                }
                catch (SqlException ex) when (SqlErrorCodes.IsConnectivityError(ex))
                {
                    Logger.LogError(ex);
                    throw new DataUnavailableException(DataUnavailableException.DefaultMessage, ex);
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

        public List<ProductReportRow> Report(string categoryId)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    const string sql =
                        "SELECT p.date_created AS DateCreated, p.code AS Code, p.name AS Name, p.description AS Description, " +
                        "c.description AS CategoryDescription, p.stock AS Stock, p.purchase_price AS PurchasePrice, " +
                        "p.sale_price AS SalePrice, p.date_expired AS DateExpired, s.name AS StatusName, " +
                        "CAST(CASE WHEN p.status = 1 THEN 1 ELSE 0 END AS bit) AS Active, " +
                        // Stock valued at each lot's own cost; if the product has no lots, fall back
                        // to its stock at the weighted-average (or last purchase) cost.
                        "ISNULL(lot.lot_value, ISNULL(p.stock, 0) * ISNULL(p.average_cost, p.purchase_price)) AS StockCostValue " +
                        "FROM product p INNER JOIN category c ON c.id = p.category_id " +
                        "INNER JOIN state_product s ON s.id = p.status " +
                        "LEFT JOIN (SELECT product_id, SUM(quantity * ISNULL(unit_cost, 0)) AS lot_value " +
                        "           FROM product_lot WHERE quantity > 0 GROUP BY product_id) lot ON lot.product_id = p.id " +
                        "WHERE c.id = case @category_id when '0' then c.id when 0 then c.id else @category_id end";

                    return oConnection.Query<ProductReportRow>(sql, new { category_id = categoryId }).ToList();
                }
                catch (SqlException ex) when (SqlErrorCodes.IsConnectivityError(ex))
                {
                    Logger.LogError(ex);
                    throw new DataUnavailableException(DataUnavailableException.DefaultMessage, ex);
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
