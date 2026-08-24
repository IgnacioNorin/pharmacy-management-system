using System.Data.SqlClient;
using PharmacySystem.Logical;
using PharmacySystem.Model;
using Xunit;

namespace PharmacySystem.Tests.Integration
{
    // The `store` table is a singleton row (id = 1) that both ListStore and UpdateStore
    // hard-code, so these tests seed/remove that single row rather than creating N rows.
    [Collection("Database")]
    public class StoreServiceTests
    {
        [Fact]
        public void UpdateStore_ThenListStore_ReflectsChanges()
        {
            SqlTestHelper.ExecuteNonQuery(
                "INSERT INTO store(id, document_store, company_name, email, phone, address) VALUES (1, @doc, @name, @email, @phone, @address)",
                new SqlParameter("@doc", "0000000000"),
                new SqlParameter("@name", "Initial store"),
                new SqlParameter("@email", "initial@test.local"),
                new SqlParameter("@phone", "0000000000"),
                new SqlParameter("@address", "Initial address"));

            try
            {
                bool result = StoreService.Instance.UpdateStore(new Store
                {
                    document = "1111111111",
                    companyName = "Updated store",
                    email = "updated@test.local",
                    phone = "1111111111",
                    address = "Updated address",
                    currencyCulture = "es-CL"
                });

                Assert.True(result);

                Store stored = StoreService.Instance.ListStore();
                Assert.Equal("Updated store", stored.companyName);
                Assert.Equal("1111111111", stored.document);
                Assert.Equal("es-CL", stored.currencyCulture);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM store WHERE id = 1");
            }
        }

        [Fact]
        public void ListStore_CurrencyColumnNull_MapsToNullInsteadOfThrowing()
        {
            // A row created before this feature existed would have NULL here; ListStore must
            // not throw converting DBNull, and CultureInfoHelper.SetCurrency(null) already
            // falls back to the default, so this is the safe legacy-row behavior.
            SqlTestHelper.ExecuteNonQuery(
                "INSERT INTO store(id, document_store, company_name, email, phone, address, currency_culture) VALUES (1, @doc, @name, @email, @phone, @address, NULL)",
                new SqlParameter("@doc", "0000000000"),
                new SqlParameter("@name", "Store without currency"),
                new SqlParameter("@email", "initial@test.local"),
                new SqlParameter("@phone", "0000000000"),
                new SqlParameter("@address", "Initial address"));

            try
            {
                Store stored = StoreService.Instance.ListStore();

                Assert.Null(stored.currencyCulture);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM store WHERE id = 1");
            }
        }

        [Fact]
        public void ListStore_NewRowUsesDatabaseDefaultCurrency()
        {
            SqlTestHelper.ExecuteNonQuery(
                "INSERT INTO store(id, document_store, company_name, email, phone, address) VALUES (1, @doc, @name, @email, @phone, @address)",
                new SqlParameter("@doc", "0000000000"),
                new SqlParameter("@name", "Store with default currency"),
                new SqlParameter("@email", "initial@test.local"),
                new SqlParameter("@phone", "0000000000"),
                new SqlParameter("@address", "Initial address"));

            try
            {
                Store stored = StoreService.Instance.ListStore();

                Assert.Equal("es-EC", stored.currencyCulture);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM store WHERE id = 1");
            }
        }
    }
}
