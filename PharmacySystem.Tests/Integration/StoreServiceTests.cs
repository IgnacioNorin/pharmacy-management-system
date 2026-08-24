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
                    address = "Updated address"
                });

                Assert.True(result);

                Store stored = StoreService.Instance.ListStore();
                Assert.Equal("Updated store", stored.companyName);
                Assert.Equal("1111111111", stored.document);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM store WHERE id = 1");
            }
        }
    }
}
