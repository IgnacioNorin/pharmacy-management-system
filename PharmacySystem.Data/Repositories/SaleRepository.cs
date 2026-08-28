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
                        "amount_received AS payWith, change_amount AS change, " +
                        "net_amount AS netAmount, tax_amount AS taxAmount, exempt_amount AS exemptAmount, " +
                        "recipient_tax_id AS recipientTaxId, recipient_business_name AS recipientBusinessName, " +
                        "recipient_activity AS recipientActivity, recipient_address AS recipientAddress, " +
                        "recipient_commune AS recipientCommune, " +
                        "date_registered AS registrationDate FROM sale";

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
                        // Receipt number comes from a sequence, generated inside the transaction
                        // so it is concurrency-safe (the old RIGHT(..., COUNT(*) + 1) handed the
                        // same number to two simultaneous sales).
                        const string insertSaleQuery =
                            "DECLARE @folio INT; " +
                            "IF @document_type = 'Factura' SET @folio = NEXT VALUE FOR dbo.seq_folio_factura; " +
                            "ELSE SET @folio = NEXT VALUE FOR dbo.seq_folio_boleta; " +
                            "INSERT INTO sale(document_type, document_number, user_id, document_client, name_client, total_amount, amount_received, change_amount, net_amount, tax_amount, exempt_amount, recipient_tax_id, recipient_business_name, recipient_activity, recipient_address, recipient_commune) " +
                            "VALUES (@document_type, RIGHT('000000' + CAST(@folio AS VARCHAR(20)), 6), @user_id, @document_client, @name_client, @total_amount, @amount_received, @change_amount, @net_amount, @tax_amount, @exempt_amount, @recipient_tax_id, @recipient_business_name, @recipient_activity, @recipient_address, @recipient_commune); " +
                            "SELECT CAST(SCOPE_IDENTITY() AS INT);";

                        int idSale = oConnection.ExecuteScalar<int>(insertSaleQuery, new
                        {
                            document_type = obj.typeDocument,
                            user_id = obj.oPerson.idPerson,
                            document_client = obj.documentClient,
                            name_client = obj.nameClient,
                            total_amount = obj.totalPay,
                            amount_received = obj.payWith,
                            change_amount = obj.change,
                            net_amount = obj.netAmount,
                            tax_amount = obj.taxAmount,
                            exempt_amount = obj.exemptAmount,
                            recipient_tax_id = obj.recipientTaxId,
                            recipient_business_name = obj.recipientBusinessName,
                            recipient_activity = obj.recipientActivity,
                            recipient_address = obj.recipientAddress,
                            recipient_commune = obj.recipientCommune
                        }, objTransacion);

                        if (idSale != 0)
                        {
                            const string subtractStockQuery =
                                "UPDATE product SET stock = stock - @amount WHERE id = @product_id AND stock >= @amount";

                            const string insertDetailQuery =
                                "INSERT INTO sale_detail(sale_id, product_id, stock, sale_price, subtotal, tax_affected) " +
                                "VALUES (@sale_id, @product_id, @stock, @sale_price, @subtotal, @tax_affected)";

                            foreach (SaleDetail dv in obj.oSaleDetail)
                            {
                                // Stock is discounted in the same transaction as the sale rows.
                                // The stock >= @amount guard makes an oversell (or a missing
                                // product) update 0 rows, which aborts the whole sale - no
                                // partial, un-rolled-back inventory movement.
                                int stockRows = oConnection.Execute(subtractStockQuery, new
                                {
                                    amount = dv.amount,
                                    product_id = dv.oProduct.idProduct
                                }, objTransacion);

                                if (stockRows == 0)
                                {
                                    objTransacion.Rollback();
                                    return 0;
                                }

                                oConnection.Execute(insertDetailQuery, new
                                {
                                    sale_id = idSale,
                                    product_id = dv.oProduct.idProduct,
                                    stock = dv.amount,
                                    sale_price = dv.salePrice,
                                    subtotal = dv.subtotal,
                                    tax_affected = dv.taxAffected
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

        public List<SaleReportRow> ReportSale(DateTime startDate, DateTime endDate)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    const string sql =
                        "SELECT s.date_registered AS DateRegistered, s.document_type AS DocumentType, s.document_number AS DocumentNumber, " +
                        "p.document_number AS SellerDocument, p.name AS SellerName, s.document_client AS ClientDocument, s.name_client AS ClientName, " +
                        "s.net_amount AS NetAmount, s.tax_amount AS TaxAmount, s.exempt_amount AS ExemptAmount, " +
                        "s.total_amount AS TotalAmount, s.amount_received AS AmountReceived, s.change_amount AS ChangeAmount " +
                        "FROM sale s " +
                        "INNER JOIN person p ON p.id = s.user_id " +
                        "WHERE s.date_registered >= @startDate AND s.date_registered < DATEADD(DAY, 1, @endDate)";

                    return oConnection.Query<SaleReportRow>(sql, new { startDate = startDate.Date, endDate = endDate.Date }).ToList();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return new List<SaleReportRow>();
                }
            }
        }

        public decimal SumTotalPay(DateTime startDate, DateTime endDate)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    const string sql =
                        "SELECT ISNULL(SUM(s.total_amount), 0) AS total_amount " +
                        "FROM sale s INNER JOIN person p ON p.id = s.user_id " +
                        "WHERE s.date_registered >= @startDate AND s.date_registered < DATEADD(DAY, 1, @endDate)";

                    return oConnection.ExecuteScalar<decimal>(sql, new { startDate = startDate.Date, endDate = endDate.Date });
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return 0;
                }
            }
        }

        public decimal SumAmountReceived(DateTime startDate, DateTime endDate)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    const string sql =
                        "SELECT ISNULL(SUM(s.amount_received), 0) AS amount_received " +
                        "FROM sale s INNER JOIN person p ON p.id = s.user_id " +
                        "WHERE s.date_registered >= @startDate AND s.date_registered < DATEADD(DAY, 1, @endDate)";

                    return oConnection.ExecuteScalar<decimal>(sql, new { startDate = startDate.Date, endDate = endDate.Date });
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return 0;
                }
            }
        }

        public decimal SumChangeAmount(DateTime startDate, DateTime endDate)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    const string sql =
                        "SELECT ISNULL(SUM(s.change_amount), 0) AS change_amount " +
                        "FROM sale s INNER JOIN person p ON p.id = s.user_id " +
                        "WHERE s.date_registered >= @startDate AND s.date_registered < DATEADD(DAY, 1, @endDate)";

                    return oConnection.ExecuteScalar<decimal>(sql, new { startDate = startDate.Date, endDate = endDate.Date });
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
