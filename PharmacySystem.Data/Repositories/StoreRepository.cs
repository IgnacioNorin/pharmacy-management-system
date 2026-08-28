using System;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using PharmacySystem.Helpers;
using PharmacySystem.Model;

namespace PharmacySystem.Data
{
    public class StoreRepository : IStoreRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public StoreRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public Store ListStore()
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    Store row = oConnection.Query<Store>(
                        "SELECT document_store AS document, company_name AS companyName, email, phone, address, " +
                        "currency_culture AS currencyCulture, default_tax_rate AS defaultTaxRate " +
                        "FROM store WHERE id = 1")
                        .FirstOrDefault();

                    if (row == null)
                    {
                        return new Store();
                    }

                    // Non-currency fields match the original dr["x"].ToString() behavior, which
                    // never returned null even for a NULL cell (DBNull.ToString() is ""). Only
                    // currency_culture is treated as genuinely nullable, same as before.
                    row.document = row.document ?? "";
                    row.companyName = row.companyName ?? "";
                    row.email = row.email ?? "";
                    row.phone = row.phone ?? "";
                    row.address = row.address ?? "";
                    return row;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return new Store();
                }
            }
        }

        public bool HasOperationalData()
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    return oConnection.ExecuteScalar<int>(
                        "SELECT CASE WHEN EXISTS (SELECT 1 FROM sale) OR EXISTS (SELECT 1 FROM purchase) THEN 1 ELSE 0 END") == 1;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    // Fail closed: if this can't be verified, the business layer must treat it
                    // as "yes, there is operational data" so a currency change is never silently
                    // let through on a DB hiccup.
                    return true;
                }
            }
        }

        public bool UpdateStoreRow(Store obj)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    var parameters = new
                    {
                        document = obj.document,
                        companyName = obj.companyName,
                        email = obj.email,
                        phone = obj.phone,
                        address = obj.address,
                        currencyCulture = obj.currencyCulture,
                        defaultTaxRate = obj.defaultTaxRate
                    };

                    int affected = oConnection.Execute(
                        "UPDATE store SET document_store = @document, company_name = @companyName, email = @email, " +
                        "phone = @phone, address = @address, currency_culture = @currencyCulture, " +
                        "default_tax_rate = @defaultTaxRate WHERE id = 1",
                        parameters);

                    // Fresh database: the singleton row may not have been seeded yet. Insert it so
                    // the store profile / currency is not silently dropped on a "successful" save.
                    if (affected == 0)
                    {
                        oConnection.Execute(
                            "INSERT INTO store(id, document_store, company_name, email, phone, address, currency_culture, default_tax_rate) " +
                            "VALUES (1, @document, @companyName, @email, @phone, @address, @currencyCulture, @defaultTaxRate)",
                            parameters);
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return false;
                }
            }
        }
    }
}
