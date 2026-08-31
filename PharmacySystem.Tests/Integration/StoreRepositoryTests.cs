using Microsoft.Data.SqlClient;
using PharmacySystem.Data;
using PharmacySystem.Model;
using Xunit;

namespace PharmacySystem.Tests.Integration
{
    // The `store` table is a singleton row (id = 1) that both ListStore and UpdateStoreRow
    // hard-code, so these tests seed/remove that single row rather than creating N rows.
    // The system is CLP-only: there is no currency column anymore.
    [Collection("Database")]
    public class StoreRepositoryTests
    {
        private static readonly IStoreRepository Repository = new StoreRepository(SqlConnectionFactory.FromConfiguration());

        private static void ReplaceStoreRow()
        {
            SqlTestHelper.ExecuteNonQuery("DELETE FROM store WHERE id = 1");
            SqlTestHelper.ExecuteNonQuery(
                "INSERT INTO store(id, document_store, company_name, email, phone, address) " +
                "VALUES (1, @doc, @name, @email, @phone, @address)",
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
                "INSERT INTO store(id, document_store, company_name, email, phone, address) " +
                "VALUES (1, '', 'Mi Farmacia', '', '', '')");
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
                    defaultTaxRate = 21m,
                    defaultDocumentType = "Factura"
                });

                Assert.True(result);

                Store stored = Repository.ListStore();
                Assert.Equal("Updated store", stored.companyName);
                Assert.Equal("1111111111", stored.document);
                Assert.Equal(21m, stored.defaultTaxRate);
                Assert.Equal("Factura", stored.defaultDocumentType);
            }
            finally
            {
                RestoreStoreSeed();
            }
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
