using System;
using Microsoft.Data.SqlClient;
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

        private static void AddLot(int productId, int quantity, DateTime? expiry)
        {
            SqlTestHelper.ExecuteNonQuery(
                "INSERT INTO product_lot(product_id, quantity, date_expired, unit_cost) VALUES (@p, @q, @d, 1)",
                new SqlParameter("@p", productId),
                new SqlParameter("@q", quantity),
                new SqlParameter("@d", (object)expiry ?? DBNull.Value));
        }

        private static int NewProduct(int categoryId, string name) => ProductRepo.Register(new Product
        {
            code = SqlTestHelper.NewTag(),
            name = name,
            description = name,
            oCategory = new Categories { IdCategory = categoryId }
        });

        // DEF-02 fase A (fase 2): the expiry alert is driven by the product's lots now, not the
        // single product.date_expired field.
        [Fact]
        public void ListExpirationDate_ReportsTheEarliestExpiringLotWithStock_AndItsQuantity()
        {
            int categoryId = CategoryRepo.Register(new Categories { description = SqlTestHelper.NewTag() });
            int productId = NewProduct(categoryId, "Lot expiry");
            AddLot(productId, 4, DateTime.Today.AddDays(10));   // near
            AddLot(productId, 20, DateTime.Today.AddDays(300)); // far

            try
            {
                var results = Repository.ListExpirationDate(days: 15);

                var row = Assert.Single(results, p => p.idProduct == productId);
                Assert.Equal(DateTime.Today.AddDays(10).Date, row.expirationDate.Date);
                Assert.Equal(4, row.stock); // only the units in the expiring lot
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product_lot WHERE product_id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
            }
        }

        [Fact]
        public void ListExpirationDate_NearLotSoldOut_NoLongerAlerts_EvenThoughAFarLotRemains()
        {
            int categoryId = CategoryRepo.Register(new Categories { description = SqlTestHelper.NewTag() });
            int productId = NewProduct(categoryId, "Near lot emptied");
            AddLot(productId, 0, DateTime.Today.AddDays(5));     // near lot, already consumed
            AddLot(productId, 20, DateTime.Today.AddDays(300));  // far lot still on hand

            try
            {
                var results = Repository.ListExpirationDate(days: 30);

                Assert.DoesNotContain(results, p => p.idProduct == productId);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product_lot WHERE product_id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
            }
        }

        [Fact]
        public void ListExpirationDate_ProductBeyondConfiguredDays_IsExcluded()
        {
            int categoryId = CategoryRepo.Register(new Categories { description = SqlTestHelper.NewTag() });
            int productId = NewProduct(categoryId, "Far expiration");
            AddLot(productId, 10, DateTime.Today.AddDays(30));

            try
            {
                var results = Repository.ListExpirationDate(days: 5);

                Assert.DoesNotContain(results, p => p.idProduct == productId);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product_lot WHERE product_id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
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
                var results = Repository.ListStock(criticalStock: 5);

                Assert.Contains(results, p => p.stock == 3);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
            }
        }

        // Coverage for Fase 1: the "at or below threshold" comparison moved from
        // MainFormPresenter's C# loop into this query. A product above the configured critical
        // stock must not come back.
        [Fact]
        public void ListStock_ProductAboveThreshold_IsExcluded()
        {
            int categoryId = CategoryRepo.Register(new Categories { description = SqlTestHelper.NewTag() });
            int productId = ProductRepo.Register(new Product
            {
                code = SqlTestHelper.NewTag(),
                name = "Well stocked",
                description = "Well stocked",
                oCategory = new Categories { IdCategory = categoryId }
            });
            SqlTestHelper.ExecuteNonQuery("UPDATE product SET stock = 50 WHERE id = @id", new SqlParameter("@id", productId));

            try
            {
                var results = Repository.ListStock(criticalStock: 5);

                Assert.DoesNotContain(results, p => p.stock == 50);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
            }
        }
    }
}
