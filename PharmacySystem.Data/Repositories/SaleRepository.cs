using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
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
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    const string sql =
                        "SELECT id AS idSale, document_type AS typeDocument, document_number AS numberDocument, " +
                        "document_client AS documentClient, name_client AS nameClient, total_amount AS totalPay, " +
                        "amount_received AS payWith, change_amount AS change, date_registered AS registrationDate FROM sale";

                    return oConnection.Query<Sale>(sql).ToList();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return new List<Sale>();
                }
            }
        }

        public List<SaleDetail> ListSaleDetail()
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    // Product's own column (name) is placed last so its region is contiguous for
                    // Dapper's split-based multi-mapping - splitOn only supports a clean two-region
                    // split, unlike the original column order (name sandwiched in the middle).
                    const string sql =
                        "SELECT sd.id AS idSaleDetail, sd.sale_id AS idSale, sd.stock AS amount, sd.sale_price AS salePrice, sd.subtotal AS subtotal, " +
                        "p.name AS name " +
                        "FROM sale_detail sd INNER JOIN product p ON p.id = sd.product_id";

                    return oConnection.Query<SaleDetail, Product, SaleDetail>(
                        sql,
                        (detail, product) => { detail.oProduct = product; return detail; },
                        splitOn: "name")
                        .ToList();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return new List<SaleDetail>();
                }
            }
        }

        public bool ControlStock(int idproduct, int amount, bool subtract)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    // When subtracting, refuse to drive stock negative: the guard makes an oversell
                    // a no-op (0 rows) instead of leaving a product at a negative quantity. The
                    // return value now reflects whether a row actually changed, so a missing
                    // product id or an insufficient-stock line no longer reports success.
                    string query = subtract
                        ? "UPDATE product SET stock = (stock - @amount) WHERE id = @idproduct AND stock >= @amount"
                        : "UPDATE product SET stock = (stock + @amount) WHERE id = @idproduct";

                    return oConnection.Execute(query, new { amount, idproduct }) > 0;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return false;
                }
            }
        }

        public int Register(Sale obj)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    oConnection.Open();
                    SqlTransaction objTransacion = oConnection.BeginTransaction();

                    try
                    {
                        const string insertSaleQuery =
                            "INSERT INTO sale(document_type, document_number, user_id, document_client, name_client, total_amount, amount_received, change_amount) " +
                            "VALUES (@document_type, (SELECT RIGHT('000000' + CAST((SELECT count(*) + 1 FROM sale) AS VARCHAR), 6)), @user_id, @document_client, @name_client, @total_amount, @amount_received, @change_amount); " +
                            "SELECT SCOPE_IDENTITY();";

                        object rawId = oConnection.ExecuteScalar<object>(insertSaleQuery, new
                        {
                            document_type = obj.typeDocument,
                            user_id = obj.oPerson.idPerson,
                            document_client = obj.documentClient,
                            name_client = obj.nameClient,
                            total_amount = obj.totalPay,
                            amount_received = obj.payWith,
                            change_amount = obj.change
                        }, objTransacion);

                        int.TryParse(rawId?.ToString(), out int idSale);

                        if (idSale != 0)
                        {
                            const string insertDetailQuery =
                                "INSERT INTO sale_detail(sale_id, product_id, stock, sale_price, subtotal) " +
                                "VALUES (@sale_id, @product_id, @stock, @sale_price, @subtotal)";

                            foreach (SaleDetail dv in obj.oSaleDetail)
                            {
                                oConnection.Execute(insertDetailQuery, new
                                {
                                    sale_id = idSale,
                                    product_id = dv.oProduct.idProduct,
                                    stock = dv.amount,
                                    sale_price = dv.salePrice,
                                    subtotal = dv.subtotal
                                }, objTransacion);
                            }

                            objTransacion.Commit();
                            return idSale;
                        }
                        else
                        {
                            objTransacion.Rollback();
                            return 0;
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e);
                        objTransacion.Rollback();
                        return 0;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return 0;
                }
            }
        }

        public List<SaleReportRow> ReportSale(string startDate, string endDate)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    const string sql =
                        "SELECT s.date_registered AS DateRegistered, s.document_type AS DocumentType, s.document_number AS DocumentNumber, " +
                        "p.document_number AS SellerDocument, p.name AS SellerName, s.document_client AS ClientDocument, s.name_client AS ClientName, " +
                        "s.total_amount AS TotalAmount, s.amount_received AS AmountReceived, s.change_amount AS ChangeAmount " +
                        "FROM sale s " +
                        "INNER JOIN person p ON p.id = s.user_id " +
                        "WHERE CAST(s.date_registered AS DATE) BETWEEN @startDate AND @endDate";

                    return oConnection.Query<SaleReportRow>(sql, new { startDate, endDate }).ToList();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return new List<SaleReportRow>();
                }
            }
        }

        public decimal SumTotalPay(string startDate, string endDate)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    const string sql =
                        "SELECT ISNULL(SUM(s.total_amount), 0) AS total_amount " +
                        "FROM sale s INNER JOIN person p ON p.id = s.user_id " +
                        "WHERE CAST(s.date_registered AS DATE) BETWEEN @startDate AND @endDate";

                    return oConnection.ExecuteScalar<decimal>(sql, new { startDate, endDate });
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return 0;
                }
            }
        }

        public decimal SumAmountReceived(string startDate, string endDate)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    const string sql =
                        "SELECT ISNULL(SUM(s.amount_received), 0) AS amount_received " +
                        "FROM sale s INNER JOIN person p ON p.id = s.user_id " +
                        "WHERE CAST(s.date_registered AS DATE) BETWEEN @startDate AND @endDate";

                    return oConnection.ExecuteScalar<decimal>(sql, new { startDate, endDate });
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return 0;
                }
            }
        }

        public decimal SumChangeAmount(string startDate, string endDate)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    const string sql =
                        "SELECT ISNULL(SUM(s.change_amount), 0) AS change_amount " +
                        "FROM sale s INNER JOIN person p ON p.id = s.user_id " +
                        "WHERE CAST(s.date_registered AS DATE) BETWEEN @startDate and @endDate";

                    return oConnection.ExecuteScalar<decimal>(sql, new { startDate, endDate });
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return 0;
                }
            }
        }
    }
}
