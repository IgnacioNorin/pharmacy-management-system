using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using PharmacySystem.Fiscal;
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
                        "recipient_commune AS recipientCommune, client_id AS clientId, reference_id AS referenceId, reference_reason AS referenceReason, " +
                        "fiscal_status AS fiscalStatus, fiscal_track_id AS fiscalTrackId, fiscal_barcode AS fiscalBarcode, " +
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
                            "INSERT INTO sale(document_type, document_number, user_id, document_client, name_client, total_amount, amount_received, change_amount, net_amount, tax_amount, exempt_amount, recipient_tax_id, recipient_business_name, recipient_activity, recipient_address, recipient_commune, client_id) " +
                            "VALUES (@document_type, RIGHT('000000' + CAST(@folio AS VARCHAR(20)), 6), @user_id, @document_client, @name_client, @total_amount, @amount_received, @change_amount, @net_amount, @tax_amount, @exempt_amount, @recipient_tax_id, @recipient_business_name, @recipient_activity, @recipient_address, @recipient_commune, @client_id); " +
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
                            recipient_commune = obj.recipientCommune,
                            client_id = obj.clientId
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

        // Stores back the fiscal issuer's outcome. document_number is only overwritten when the
        // issuer assigned its own folio (result.DocumentNumber not null); otherwise the number
        // set by Register stays.
        public void SaveFiscalResult(int saleId, FiscalDocumentResult result)
        {
            if (result == null)
                return;

            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    const string sql =
                        "UPDATE sale SET fiscal_status = @status, fiscal_track_id = @trackId, fiscal_barcode = @barcode, " +
                        "document_number = COALESCE(@documentNumber, document_number) WHERE id = @id";

                    oConnection.Execute(sql, new
                    {
                        id = saleId,
                        status = result.Status,
                        trackId = result.TrackId,
                        barcode = result.Barcode,
                        documentNumber = result.DocumentNumber
                    });
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                }
            }
        }

        public SaleLookup FindByDocument(string documentType, string documentNumber)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    const string sql =
                        "SELECT s.id AS Id, s.document_type AS DocumentType, s.document_number AS DocumentNumber, " +
                        "s.date_registered AS Date, s.name_client AS ClientName, s.total_amount AS TotalAmount, " +
                        "CAST(CASE WHEN s.document_type = 'Nota de Credito' THEN 1 ELSE 0 END AS BIT) AS IsCreditNote, " +
                        "CAST(CASE WHEN EXISTS (SELECT 1 FROM sale nc WHERE nc.reference_id = s.id) THEN 1 ELSE 0 END AS BIT) AS AlreadyCreditNoted " +
                        "FROM sale s WHERE s.document_type = @documentType AND s.document_number = @documentNumber";

                    return oConnection.QueryFirstOrDefault<SaleLookup>(sql, new { documentType, documentNumber });
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return null;
                }
            }
        }

        private class OriginalSaleRow
        {
            public string DocumentType { get; set; }
            public string DocumentClient { get; set; }
            public string NameClient { get; set; }
            public decimal TotalAmount { get; set; }
            public decimal NetAmount { get; set; }
            public decimal TaxAmount { get; set; }
            public decimal ExemptAmount { get; set; }
            public string RecipientTaxId { get; set; }
            public string RecipientBusinessName { get; set; }
            public string RecipientActivity { get; set; }
            public string RecipientAddress { get; set; }
            public string RecipientCommune { get; set; }
            public int? ClientId { get; set; }
        }

        private class OriginalDetailRow
        {
            public int ProductId { get; set; }
            public int Amount { get; set; }
            public decimal SalePrice { get; set; }
            public decimal Subtotal { get; set; }
            public bool TaxAffected { get; set; }
        }

        // Issues a Nota de Credito that reverses an existing sale: a new sale row with negative
        // amounts and reference_id set, and the stock of every line put back. Atomic.
        public CreditNoteResult CreateCreditNote(int originalSaleId, int userId, string reason)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                oConnection.Open();
                SqlTransaction tx = oConnection.BeginTransaction();
                try
                {
                    OriginalSaleRow original = oConnection.QueryFirstOrDefault<OriginalSaleRow>(
                        "SELECT document_type AS DocumentType, document_client AS DocumentClient, name_client AS NameClient, " +
                        "total_amount AS TotalAmount, net_amount AS NetAmount, tax_amount AS TaxAmount, exempt_amount AS ExemptAmount, " +
                        "recipient_tax_id AS RecipientTaxId, recipient_business_name AS RecipientBusinessName, " +
                        "recipient_activity AS RecipientActivity, recipient_address AS RecipientAddress, recipient_commune AS RecipientCommune, " +
                        "client_id AS ClientId " +
                        "FROM sale WITH (UPDLOCK) WHERE id = @id", new { id = originalSaleId }, tx);

                    if (original == null)
                    {
                        tx.Rollback();
                        return CreditNoteResult.NotFound;
                    }
                    if (original.DocumentType == "Nota de Credito")
                    {
                        tx.Rollback();
                        return CreditNoteResult.NotAllowedOnCreditNote;
                    }
                    if (oConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM sale WHERE reference_id = @id", new { id = originalSaleId }, tx) > 0)
                    {
                        tx.Rollback();
                        return CreditNoteResult.AlreadyCreditNoted;
                    }

                    const string insertNc =
                        "DECLARE @folio INT = NEXT VALUE FOR dbo.seq_folio_nota_credito; " +
                        "INSERT INTO sale(document_type, document_number, user_id, document_client, name_client, total_amount, amount_received, change_amount, net_amount, tax_amount, exempt_amount, recipient_tax_id, recipient_business_name, recipient_activity, recipient_address, recipient_commune, client_id, reference_id, reference_reason) " +
                        "VALUES ('Nota de Credito', RIGHT('000000' + CAST(@folio AS VARCHAR(20)), 6), @user_id, @document_client, @name_client, @total, 0, 0, @net, @tax, @exempt, @rtaxid, @rname, @ractivity, @raddress, @rcommune, @client_id, @reference, @reason); " +
                        "SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    int ncId = oConnection.ExecuteScalar<int>(insertNc, new
                    {
                        user_id = userId,
                        document_client = original.DocumentClient,
                        name_client = original.NameClient,
                        total = -original.TotalAmount,
                        net = -original.NetAmount,
                        tax = -original.TaxAmount,
                        exempt = -original.ExemptAmount,
                        rtaxid = original.RecipientTaxId,
                        rname = original.RecipientBusinessName,
                        ractivity = original.RecipientActivity,
                        raddress = original.RecipientAddress,
                        rcommune = original.RecipientCommune,
                        client_id = original.ClientId,
                        reference = originalSaleId,
                        reason = reason
                    }, tx);

                    var lines = oConnection.Query<OriginalDetailRow>(
                        "SELECT product_id AS ProductId, stock AS Amount, sale_price AS SalePrice, subtotal AS Subtotal, tax_affected AS TaxAffected " +
                        "FROM sale_detail WHERE sale_id = @id", new { id = originalSaleId }, tx);

                    foreach (OriginalDetailRow line in lines)
                    {
                        oConnection.Execute("UPDATE product SET stock = stock + @amount WHERE id = @product_id",
                            new { amount = line.Amount, product_id = line.ProductId }, tx);

                        oConnection.Execute(
                            "INSERT INTO sale_detail(sale_id, product_id, stock, sale_price, subtotal, tax_affected) " +
                            "VALUES (@sale_id, @product_id, @amount, @sale_price, @subtotal, @tax_affected)",
                            new
                            {
                                sale_id = ncId,
                                product_id = line.ProductId,
                                amount = line.Amount,
                                sale_price = line.SalePrice,
                                subtotal = line.Subtotal,
                                tax_affected = line.TaxAffected
                            }, tx);
                    }

                    tx.Commit();
                    return CreditNoteResult.Ok;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    try { tx.Rollback(); } catch { }
                    return CreditNoteResult.Error;
                }
            }
        }

        public List<SaleReportRow> ReportSale(DateTime startDate, DateTime endDate, int clientId)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    const string sql =
                        "SELECT s.date_registered AS DateRegistered, s.document_type AS DocumentType, s.document_number AS DocumentNumber, " +
                        "p.document_number AS SellerDocument, p.name AS SellerName, s.document_client AS ClientDocument, s.name_client AS ClientName, " +
                        "s.recipient_tax_id AS RecipientTaxId, s.recipient_business_name AS RecipientBusinessName, " +
                        "s.net_amount AS NetAmount, s.tax_amount AS TaxAmount, s.exempt_amount AS ExemptAmount, " +
                        "s.total_amount AS TotalAmount, s.amount_received AS AmountReceived, s.change_amount AS ChangeAmount " +
                        "FROM sale s " +
                        "INNER JOIN person p ON p.id = s.user_id " +
                        "WHERE s.date_registered >= @startDate AND s.date_registered < DATEADD(DAY, 1, @endDate) " +
                        "AND (@clientId = 0 OR s.client_id = @clientId)";

                    return oConnection.Query<SaleReportRow>(sql, new { startDate = startDate.Date, endDate = endDate.Date, clientId }).ToList();
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

    }
}
