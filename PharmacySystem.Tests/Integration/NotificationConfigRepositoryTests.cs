using System;
using System.Data.SqlClient;
using PharmacySystem.Data;
using PharmacySystem.Model;
using Xunit;

namespace PharmacySystem.Tests.Integration
{
    // Was NotificationConfigServiceTests, using `new NotificationConfigService()`. Now exercises
    // NotificationConfigRepository directly. Category/product setup goes through their repositories
    // directly too, just as test fixture data.
    // notification_settings is a singleton row (id = 1); ConfigUpdate only updates it, it never
    // inserts, so the row must already exist for the update branch to succeed.
    [Collection("Database")]
    public class NotificationConfigRepositoryTests
    {
        private static readonly INotificationConfigRepository Repository = new NotificationConfigRepository(SqlConnectionFactory.FromConfiguration());
        private static readonly ICategoryRepository CategoryRepo = new CategoryRepository(SqlConnectionFactory.FromConfiguration());
        private static readonly IProductRepository ProductRepo = new ProductRepository(SqlConnectionFactory.FromConfiguration());

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
            int categoryId = CategoryRepo.Register(new Categories { description = SqlTestHelper.NewTag() });
            int withExpiration = ProductRepo.Register(new Product
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

        // Regression test: ListStock() used to require date_expired IS NOT NULL in its WHERE
        // clause, so a product with no expiration date could never trigger a critical-stock alert
        // no matter how low its stock got. Fixed by dropping that condition from the query.
        [Fact]
        public void ListStock_IncludesProductsWithoutExpirationDate()
        {
            int categoryId = CategoryRepo.Register(new Categories { description = SqlTestHelper.NewTag() });
            int productId = ProductRepo.Register(new Product
            {
                code = SqlTestHelper.NewTag(),
                name = "Without expiration",
                description = "Without expiration",
                oCategory = new Categories { IdCategory = categoryId }
            });
            SqlTestHelper.ExecuteNonQuery("UPDATE product SET stock = 3, date_expired = NULL WHERE id = @id",
                new SqlParameter("@id", productId));

            try
            {
                var results = Repository.ListStock();

                Assert.Contains(results, p => p.stock == 3);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
            }
        }
    }
}
