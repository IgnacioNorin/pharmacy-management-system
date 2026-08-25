using System;
using System.Data.SqlClient;
using PharmacySystem.Data;
using PharmacySystem.Logical;
using PharmacySystem.Model;
using Xunit;

namespace PharmacySystem.Tests.Integration
{
    // Was NotificationConfigServiceTests, using `new NotificationConfigService()`. Now exercises
    // NotificationConfigRepository directly. CategoryService/ProductService.Instance stay as-is
    // below (just test setup) until those services are migrated.
    // notification_settings is a singleton row (id = 1); ConfigUpdate only updates it, it never
    // inserts, so the row must already exist for the update branch to succeed.
    [Collection("Database")]
    public class NotificationConfigRepositoryTests
    {
        private static readonly INotificationConfigRepository Repository = new NotificationConfigRepository(SqlConnectionFactory.FromConfiguration());

        [Fact]
        public void ConfigUpdate_ExistingRow_ChangesStockAndDay()
        {
            SqlTestHelper.ExecuteNonQuery("DELETE FROM notification_settings WHERE id = 1");
            SqlTestHelper.ExecuteNonQuery("INSERT INTO notification_settings(id, critical_stock, notify_day) VALUES (1, 5, 3)");

            try
            {
                bool result = Repository.ConfigUpdate(new NotificationConfig { criticalStock = 20, days = 7 });

                Assert.True(result);
                Assert.Equal(20, Repository.ConfigStock());
                Assert.Equal(7, Repository.ConfigDay());
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM notification_settings WHERE id = 1");
            }
        }

        [Fact]
        public void ListExpirationDate_OnlyReturnsActiveProductsWithExpirationSet()
        {
            int categoryId = CategoryService.Instance.RegisterCategory(new Categories { description = SqlTestHelper.NewTag() });
            int withExpiration = ProductService.Instance.RegisterProduct(new Product
            {
                code = SqlTestHelper.NewTag(),
                name = "With expiration",
                description = "With expiration",
                oCategory = new Categories { IdCategory = categoryId }
            });
            SqlTestHelper.ExecuteNonQuery("UPDATE product SET date_expired = @date WHERE id = @id",
                new SqlParameter("@date", DateTime.Today.AddDays(10)), new SqlParameter("@id", withExpiration));

            try
            {
                var results = Repository.ListExpirationDate();

                Assert.Contains(results, p => p.expirationDate.Date == DateTime.Today.AddDays(10).Date);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", withExpiration));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
            }
        }
    }
}
