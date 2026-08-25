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

        [Fact]
        public void HasOperationalData_NoSalesOrPurchases_ReturnsFalse()
        {
            Assert.False(StoreService.Instance.HasOperationalData());
        }

        [Fact]
        public void HasOperationalData_WithSale_ReturnsTrue()
        {
            SqlTestHelper.ExecuteNonQuery("INSERT INTO sale(amount_received) VALUES (0)");
            int saleId = SqlTestHelper.ExecuteScalarInt("SELECT MAX(id) FROM sale");

            try
            {
                Assert.True(StoreService.Instance.HasOperationalData());
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale WHERE id = @id", new SqlParameter("@id", saleId));
            }
        }

        [Fact]
        public void HasOperationalData_WithPurchase_ReturnsTrue()
        {
            SqlTestHelper.ExecuteNonQuery("INSERT INTO purchase(total_amount) VALUES (0)");
            int purchaseId = SqlTestHelper.ExecuteScalarInt("SELECT MAX(id) FROM purchase");

            try
            {
                Assert.True(StoreService.Instance.HasOperationalData());
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM purchase WHERE id = @id", new SqlParameter("@id", purchaseId));
            }
        }

        [Fact]
        public void UpdateStore_ChangingCurrencyWithNoOperationalData_Succeeds()
        {
            SqlTestHelper.ExecuteNonQuery(
                "INSERT INTO store(id, document_store, company_name, email, phone, address, currency_culture) VALUES (1, @doc, @name, @email, @phone, @address, 'es-EC')",
                new SqlParameter("@doc", "0000000000"),
                new SqlParameter("@name", "Store"),
                new SqlParameter("@email", "initial@test.local"),
                new SqlParameter("@phone", "0000000000"),
                new SqlParameter("@address", "Initial address"));

            try
            {
                bool result = StoreService.Instance.UpdateStore(new Store
                {
                    document = "0000000000",
                    companyName = "Store",
                    email = "initial@test.local",
                    phone = "0000000000",
                    address = "Initial address",
                    currencyCulture = "es-CL"
                });

                Assert.True(result);
                Assert.Equal("es-CL", StoreService.Instance.ListStore().currencyCulture);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM store WHERE id = 1");
            }
        }

        [Fact]
        public void UpdateStore_ChangingCurrencyWithOperationalData_IsRejectedAndCurrencyStaysUnchanged()
        {
            SqlTestHelper.ExecuteNonQuery(
                "INSERT INTO store(id, document_store, company_name, email, phone, address, currency_culture) VALUES (1, @doc, @name, @email, @phone, @address, 'es-EC')",
                new SqlParameter("@doc", "0000000000"),
                new SqlParameter("@name", "Store"),
                new SqlParameter("@email", "initial@test.local"),
                new SqlParameter("@phone", "0000000000"),
                new SqlParameter("@address", "Initial address"));
            SqlTestHelper.ExecuteNonQuery("INSERT INTO sale(amount_received) VALUES (0)");
            int saleId = SqlTestHelper.ExecuteScalarInt("SELECT MAX(id) FROM sale");

            try
            {
                bool result = StoreService.Instance.UpdateStore(new Store
                {
                    document = "0000000000",
                    companyName = "Store",
                    email = "initial@test.local",
                    phone = "0000000000",
                    address = "Initial address",
                    currencyCulture = "es-CL"
                });

                Assert.False(result);
                Assert.Equal("es-EC", StoreService.Instance.ListStore().currencyCulture);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale WHERE id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM store WHERE id = 1");
            }
        }

        [Fact]
        public void UpdateStore_SameCurrencyWithOperationalData_StillAllowsOtherFieldChanges()
        {
            // The lock only blocks an actual currency change; unrelated store edits (address,
            // phone, etc.) must keep working after the pharmacy has real sales/purchases.
            SqlTestHelper.ExecuteNonQuery(
                "INSERT INTO store(id, document_store, company_name, email, phone, address, currency_culture) VALUES (1, @doc, @name, @email, @phone, @address, 'es-EC')",
                new SqlParameter("@doc", "0000000000"),
                new SqlParameter("@name", "Store"),
                new SqlParameter("@email", "initial@test.local"),
                new SqlParameter("@phone", "0000000000"),
                new SqlParameter("@address", "Initial address"));
            SqlTestHelper.ExecuteNonQuery("INSERT INTO sale(amount_received) VALUES (0)");
            int saleId = SqlTestHelper.ExecuteScalarInt("SELECT MAX(id) FROM sale");

            try
            {
                bool result = StoreService.Instance.UpdateStore(new Store
                {
                    document = "0000000000",
                    companyName = "Store",
                    email = "initial@test.local",
                    phone = "0000000000",
                    address = "New address",
                    currencyCulture = "es-EC"
                });

                Assert.True(result);
                Assert.Equal("New address", StoreService.Instance.ListStore().address);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale WHERE id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM store WHERE id = 1");
            }
        }
    }
}
