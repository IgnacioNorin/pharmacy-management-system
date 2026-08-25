using System.Data.SqlClient;
using PharmacySystem.Data;
using PharmacySystem.Model;
using Xunit;

namespace PharmacySystem.Tests.Integration
{
    // Was StoreServiceTests. The currency-lock rule moved out of this layer entirely (it's
    // business logic, not persistence) - see StoreServiceTests in PharmacySystem.Tests.Business
    // for that, tested against a fake repository with no database. This file only covers what
    // the repository itself does: raw reads/writes and the HasOperationalData data fact.
    // The `store` table is a singleton row (id = 1) that both ListStore and UpdateStoreRow
    // hard-code, so these tests seed/remove that single row rather than creating N rows.
    [Collection("Database")]
    public class StoreRepositoryTests
    {
        private static readonly IStoreRepository Repository = new StoreRepository(SqlConnectionFactory.FromConfiguration());

        [Fact]
        public void UpdateStoreRow_ThenListStore_ReflectsChanges()
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
                bool result = Repository.UpdateStoreRow(new Store
                {
                    document = "1111111111",
                    companyName = "Updated store",
                    email = "updated@test.local",
                    phone = "1111111111",
                    address = "Updated address",
                    currencyCulture = "es-CL"
                });

                Assert.True(result);

                Store stored = Repository.ListStore();
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
            SqlTestHelper.ExecuteNonQuery(
                "INSERT INTO store(id, document_store, company_name, email, phone, address, currency_culture) VALUES (1, @doc, @name, @email, @phone, @address, NULL)",
                new SqlParameter("@doc", "0000000000"),
                new SqlParameter("@name", "Store without currency"),
                new SqlParameter("@email", "initial@test.local"),
                new SqlParameter("@phone", "0000000000"),
                new SqlParameter("@address", "Initial address"));

            try
            {
                Store stored = Repository.ListStore();

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
                Store stored = Repository.ListStore();

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
            Assert.False(Repository.HasOperationalData());
        }

        [Fact]
        public void HasOperationalData_WithSale_ReturnsTrue()
        {
            SqlTestHelper.ExecuteNonQuery("INSERT INTO sale(amount_received) VALUES (0)");
            int saleId = SqlTestHelper.ExecuteScalarInt("SELECT MAX(id) FROM sale");

            try
            {
                Assert.True(Repository.HasOperationalData());
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
                Assert.True(Repository.HasOperationalData());
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM purchase WHERE id = @id", new SqlParameter("@id", purchaseId));
            }
        }
    }
}
