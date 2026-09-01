using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using Dapper;
using PharmacySystem.Helpers;
using PharmacySystem.Infrastructure;
using PharmacySystem.Model;

namespace PharmacySystem.Data
{
    public class PurchaseRepository : IPurchaseRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public PurchaseRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public bool Register(Purchase purchase)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    oConnection.Open();

                    SqlTransaction objTransacion = oConnection.BeginTransaction();

                    try
                    {
                        const string insertPurchaseQuery =
                            "INSERT INTO purchase(person_id, supplier_id, total_amount, net_amount, tax_amount, exempt_amount, tax_rate, document_type, document_number) " +
                            "VALUES (@person_id, @supplier_id, @total_amount, @net_amount, @tax_amount, @exempt_amount, @tax_rate, @document_type, @document_number); " +
                            "SELECT SCOPE_IDENTITY();";

                        Person person = purchase.oPerson
                            ?? throw new ArgumentException("Purchase.oPerson must be set.", nameof(purchase));
                        Supplier supplier = purchase.oSupplier
                            ?? throw new ArgumentException("Purchase.oSupplier must be set.", nameof(purchase));

                        object? rawId = oConnection.ExecuteScalar<object>(insertPurchaseQuery, new
                        {
                            person_id = person.idPerson,
                            supplier_id = supplier.idSupplier,
                            total_amount = purchase.totalAmount,
                            net_amount = purchase.netAmount,
                            tax_amount = purchase.taxAmount,
                            exempt_amount = purchase.exemptAmount,
                            tax_rate = purchase.taxRate,
                            document_type = purchase.documentType,
                            document_number = purchase.documentNumber
                        }, objTransacion);

                        int.TryParse(rawId?.ToString(), out int idPurchase);

                        if (idPurchase != 0)
                        {
                            const string insertDetailQuery =
                                "INSERT INTO purchase_detail(purchase_id, product_id, stock, purchase_price, total_amount, date_expired) " +
                                "VALUES (@purchase_id, @product_id, @stock, @purchase_price, @total_amount, @date_expired); " +
                                "SELECT CAST(SCOPE_IDENTITY() AS INT);";

                            // One lot per purchase line: this batch's own quantity, expiry and cost,
                            // so alerts and valuation can work per lot instead of per product.
                            const string insertLotQuery =
                                "INSERT INTO product_lot(product_id, purchase_detail_id, quantity, date_expired, unit_cost) " +
                                "VALUES (@product_id, @purchase_detail_id, @quantity, @date_expired, @unit_cost)";

                            // A purchase only moves stock and cost. It never touches sale_price:
                            // the sale price is set deliberately in the Prices screen, which also
                            // releases the product for sale. The lot expiry is kept on the detail
                            // line above; on the product master the date only moves EARLIER, so a
                            // newer lot with a later expiry does not switch off the alert for older
                            // stock still on the shelf.
                            //
                            // average_cost is a moving weighted average. Every SET clause reads the
                            // pre-update column values, so `stock` here is the stock BEFORE this
                            // purchase: avg' = (oldStock*oldAvg + qty*buyCost) / (oldStock + qty).
                            const string updateProductQuery =
                                "UPDATE product SET " +
                                "average_cost = CASE WHEN ISNULL(stock, 0) + @quantity <= 0 THEN @purchase_price " +
                                "ELSE ROUND((ISNULL(stock, 0) * ISNULL(average_cost, @purchase_price) + @quantity * @purchase_price) " +
                                "/ (ISNULL(stock, 0) + @quantity), 2) END, " +
                                "stock = (stock + @quantity), purchase_price = @purchase_price, " +
                                "date_expired = CASE WHEN date_expired IS NULL OR @date_expired < date_expired THEN @date_expired ELSE date_expired END " +
                                "WHERE id = @product_id";

                            foreach (PurchaseDetail pd in purchase.oPurchaseDetail)
                            {
                                Product product = pd.oProduct
                                    ?? throw new ArgumentException("PurchaseDetail.oProduct must be set.", nameof(purchase));

                                int purchaseDetailId = oConnection.ExecuteScalar<int>(insertDetailQuery, new
                                {
                                    purchase_id = idPurchase,
                                    product_id = product.idProduct,
                                    stock = pd.quantity,
                                    purchase_price = pd.purchasePrice,
                                    total_amount = pd.total,
                                    date_expired = pd.expirationDate
                                }, objTransacion);

                                oConnection.Execute(insertLotQuery, new
                                {
                                    product_id = product.idProduct,
                                    purchase_detail_id = purchaseDetailId,
                                    quantity = pd.quantity,
                                    date_expired = pd.expirationDate,
                                    unit_cost = pd.purchasePrice
                                }, objTransacion);

                                oConnection.Execute(updateProductQuery, new
                                {
                                    quantity = pd.quantity,
                                    purchase_price = pd.purchasePrice,
                                    date_expired = pd.expirationDate,
                                    product_id = product.idProduct
                                }, objTransacion);
                            }

                            objTransacion.Commit();
                            return true;
                        }
                        else
                        {
                            objTransacion.Rollback();
                            return false;
                        }
                    }
                    catch (SqlException ex) when (SqlErrorCodes.IsUniqueViolation(ex))
                    {
                        // UX_purchase_supplier_document: this supplier invoice is already recorded.
                        Logger.LogError(ex);
                        objTransacion.Rollback();
                        throw new DuplicateInvoiceException(DuplicateInvoiceException.DefaultMessage, ex);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e);
                        objTransacion.Rollback();
                        return false;
                    }
                }
                catch (DuplicateInvoiceException)
                {
                    throw;
                }
                catch (SqlException ex) when (SqlErrorCodes.IsConnectivityError(ex))
                {
                    // The connection could not be opened: no transaction to roll back, and the
                    // caller must not read this as a plain "could not register the purchase".
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

        public List<PurchaseReportRow> ReportPurchase(string idSupplier, DateTime startDate, DateTime endDate)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    const string sql =
                        "SELECT pu.date_registered AS DateRegistered, su.document_number AS SupplierDocument, su.company_name AS CompanyName, " +
                        "pu.document_type AS DocumentType, pu.document_number AS DocumentNumber, pu.total_amount AS TotalAmount, " +
                        "pu.net_amount AS NetAmount, pu.tax_amount AS TaxAmount, pu.exempt_amount AS ExemptAmount, " +
                        "pr.name AS ProductName, pd.stock AS Quantity, pd.purchase_price AS PurchasePrice, " +
                        "pd.stock * pd.purchase_price AS LineTotal " +
                        "FROM purchase pu " +
                        "INNER JOIN supplier su ON su.id = pu.supplier_id " +
                        "INNER JOIN purchase_detail pd ON pd.purchase_id = pu.id " +
                        "INNER JOIN product pr on pr.id = pd.product_id " +
                        "WHERE pu.date_registered >= @startDate AND pu.date_registered < DATEADD(DAY, 1, @endDate) " +
                        "and pu.supplier_id =  CASE @supplier_id WHEN '0' THEN pu.supplier_id " +
                        "WHEN 0 THEN pu.supplier_id ELSE @supplier_id END";

                    return oConnection.Query<PurchaseReportRow>(sql, new { startDate = startDate.Date, endDate = endDate.Date, supplier_id = idSupplier }).ToList();
                }
                catch (SqlException ex) when (SqlErrorCodes.IsConnectivityError(ex))
                {
                    Logger.LogError(ex);
                    throw new DataUnavailableException(DataUnavailableException.DefaultMessage, ex);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return new List<PurchaseReportRow>();
                }
            }
        }

        public PurchaseReportTotals GetTotals(string idSupplier, DateTime startDate, DateTime endDate)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    // These are purchase-header columns: they must NOT be joined to
                    // purchase_detail, or each sum is multiplied by the number of detail lines
                    // (a 3-line purchase counted its total three times).
                    const string sql =
                        "SELECT ISNULL(SUM(pu.total_amount),0) AS TotalAmount, " +
                        "ISNULL(SUM(pu.net_amount),0) AS NetAmount, " +
                        "ISNULL(SUM(pu.tax_amount),0) AS TaxAmount, " +
                        "ISNULL(SUM(pu.exempt_amount),0) AS ExemptAmount " +
                        "FROM purchase pu " +
                        "WHERE pu.date_registered >= @startDate AND pu.date_registered < DATEADD(DAY, 1, @endDate) " +
                        "AND (@supplier_id = 0 OR pu.supplier_id = @supplier_id)";

                    return oConnection.QuerySingle<PurchaseReportTotals>(sql, new { startDate = startDate.Date, endDate = endDate.Date, supplier_id = idSupplier });
                }
                catch (SqlException ex) when (SqlErrorCodes.IsConnectivityError(ex))
                {
                    Logger.LogError(ex);
                    throw new DataUnavailableException(DataUnavailableException.DefaultMessage, ex);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return new PurchaseReportTotals();
                }
            }
        }

    }
}
