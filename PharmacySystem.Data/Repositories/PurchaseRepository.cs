using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using PharmacySystem.Helpers;
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
                            "INSERT INTO purchase(person_id, supplier_id, total_amount, document_type, document_number) " +
                            "VALUES (@person_id, @supplier_id, @total_amount, @document_type, @document_number); " +
                            "SELECT SCOPE_IDENTITY();";

                        object rawId = oConnection.ExecuteScalar<object>(insertPurchaseQuery, new
                        {
                            person_id = purchase.oPerson.idPerson,
                            supplier_id = purchase.oSupplier.idSupplier,
                            total_amount = purchase.totalAmount,
                            document_type = purchase.documentType,
                            document_number = purchase.documentNumber
                        }, objTransacion);

                        int.TryParse(rawId?.ToString(), out int idPurchase);

                        if (idPurchase != 0)
                        {
                            const string insertDetailQuery =
                                "INSERT INTO purchase_detail(purchase_id, product_id, stock, purchase_price, sale_price, total_amount) " +
                                "VALUES (@purchase_id, @product_id, @stock, @purchase_price, @sale_price, @total_amount)";

                            const string updateProductQuery =
                                "UPDATE product SET stock = (stock + @quantity), purchase_price = @purchase_price, sale_price = @sale_price, date_expired = @date_expired WHERE id = @product_id";

                            foreach (PurchaseDetail pd in purchase.oPurchaseDetail)
                            {
                                oConnection.Execute(insertDetailQuery, new
                                {
                                    purchase_id = idPurchase,
                                    product_id = pd.oProduct.idProduct,
                                    stock = pd.quantity,
                                    purchase_price = pd.purchasePrice,
                                    sale_price = pd.salePrice,
                                    total_amount = pd.total
                                }, objTransacion);

                                oConnection.Execute(updateProductQuery, new
                                {
                                    quantity = pd.quantity,
                                    purchase_price = pd.purchasePrice,
                                    sale_price = pd.salePrice,
                                    date_expired = pd.expirationDate,
                                    product_id = pd.oProduct.idProduct
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
                    catch (Exception e)
                    {
                        Logger.LogError(e);
                        objTransacion.Rollback();
                        return false;
                    }
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
                        "pr.name AS ProductName, pd.stock AS Quantity, pd.purchase_price AS PurchasePrice, pd.sale_price AS SalePrice " +
                        "FROM purchase pu " +
                        "INNER JOIN supplier su ON su.id = pu.supplier_id " +
                        "INNER JOIN purchase_detail pd ON pd.purchase_id = pu.id " +
                        "INNER JOIN product pr on pr.id = pd.product_id " +
                        "WHERE pu.date_registered >= @startDate AND pu.date_registered < DATEADD(DAY, 1, @endDate) " +
                        "and pu.supplier_id =  CASE @supplier_id WHEN '0' THEN pu.supplier_id " +
                        "WHEN 0 THEN pu.supplier_id ELSE @supplier_id END";

                    return oConnection.Query<PurchaseReportRow>(sql, new { startDate = startDate.Date, endDate = endDate.Date, supplier_id = idSupplier }).ToList();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return new List<PurchaseReportRow>();
                }
            }
        }

        public decimal GetTotalAmount(string idSupplier, DateTime startDate, DateTime endDate)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    // total_amount is a purchase-header column: it must NOT be joined to
                    // purchase_detail, or the sum is multiplied by the number of detail lines
                    // (a 3-line purchase counted its total three times).
                    const string sql =
                        "SELECT ISNULL(SUM(pu.total_amount),0) AS total_amount " +
                        "FROM purchase pu " +
                        "WHERE pu.date_registered >= @startDate AND pu.date_registered < DATEADD(DAY, 1, @endDate) " +
                        "AND (@supplier_id = 0 OR pu.supplier_id = @supplier_id)";

                    return oConnection.ExecuteScalar<decimal>(sql, new { startDate = startDate.Date, endDate = endDate.Date, supplier_id = idSupplier });
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
