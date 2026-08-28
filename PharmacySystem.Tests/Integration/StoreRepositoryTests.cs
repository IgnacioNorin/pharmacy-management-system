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

        // The schema now seeds the singleton store row (id = 1). These tests need to own that
        // row's contents, so they replace it and then restore it to the canonical seed value.
        private static void ReplaceStoreRow(string extraColumns = "", string extraValues = "")
        {
            SqlTestHelper.ExecuteNonQuery("DELETE FROM store WHERE id = 1");
            SqlTestHelper.ExecuteNonQuery(
                "INSERT INTO store(id, document_store, company_name, email, phone, address" + extraColumns + ") " +
                "VALUES (1, @doc, @name, @email, @phone, @address" + extraValues + ")",
                new SqlParameter("@doc", "0000000000"),
                new SqlParameter("@name", "Initial store"),
                new SqlParameter("@email", "initial@test.local"),
                new SqlParameter("@phone", "0000000000"),
                new SqlParameter("@address", "Initial address"));
        }

        private static void RestoreStoreSeed()
        {
            SqlTestHelper.ExecuteNonQuery("DELETE FROM store WHERE id = 1");
            SqlTestHelper.ExecuteNonQuery(
                "INSERT INTO store(id, document_store, company_name, email, phone, address, currency_culture) " +
                "VALUES (1, '', 'Mi Farmacia', '', '', '', 'es-EC')");
        }

        [Fact]
        public void UpdateStoreRow_ThenListStore_ReflectsChanges()
        {
            ReplaceStoreRow();

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
                RestoreStoreSeed();
            }
        }

        [Fact]
        public void ListStore_CurrencyColumnNull_MapsToNullInsteadOfThrowing()
        {
            ReplaceStoreRow(", currency_culture", ", NULL");

            try
            {
                Store stored = Repository.ListStore();

                Assert.Null(stored.currencyCulture);
            }
            finally
            {
                RestoreStoreSeed();
            }
        }

        [Fact]
        public void ListStore_NewRowUsesDatabaseDefaultCurrency()
        {
            ReplaceStoreRow();

            try
            {
                Store stored = Repository.ListStore();

                Assert.Equal("es-EC", stored.currencyCulture);
            }
            finally
            {
                RestoreStoreSeed();
            }
        }

        // The "returns false with an empty database" case cannot be asserted reliably against a
        // shared dev database that may already hold real sales/purchases. The two positive cases
        // below cover HasOperationalData(); the negative branch is a plain EXISTS OR EXISTS.

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
