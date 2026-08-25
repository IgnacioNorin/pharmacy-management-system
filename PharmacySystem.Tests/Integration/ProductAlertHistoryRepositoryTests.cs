using System;
using System.Data.SqlClient;
using System.Linq;
using PharmacySystem.Data;
using PharmacySystem.Model;
using Xunit;

namespace PharmacySystem.Tests.Integration
{
    // Fase 4 of the alerts rework (traceability): product_alert_history is backed by a filtered
    // index (ix_product_alert_history_open), same QUOTED_IDENTIFIER trap as sp_delete_product
    // earlier - already fixed at CREATE TABLE time in the schema script, verified here by simply
    // exercising every write path against the real database.
    [Collection("Database")]
    public class ProductAlertHistoryRepositoryTests
    {
        private static readonly IProductAlertHistoryRepository Repository = new ProductAlertHistoryRepository(SqlConnectionFactory.FromConfiguration());
        private static readonly ICategoryRepository CategoryRepo = new CategoryRepository(SqlConnectionFactory.FromConfiguration());
        private static readonly IProductRepository ProductRepo = new ProductRepository(SqlConnectionFactory.FromConfiguration());
        private static readonly IPersonRepository PersonRepo = new PersonRepository(SqlConnectionFactory.FromConfiguration());

        private static int PersonTypeId() => SqlTestHelper.ExecuteScalarInt("SELECT TOP 1 id FROM person_type");

        private static int CreateCategory() => CategoryRepo.Register(new Categories { description = SqlTestHelper.NewTag() });

        private static int CreateProduct(int categoryId) => ProductRepo.Register(new Product
        {
            code = SqlTestHelper.NewTag(),
            name = "Test product",
            description = "Test product",
            oCategory = new Categories { IdCategory = categoryId }
        });

        private static int CreatePerson()
        {
            string document = SqlTestHelper.NewTag();
            PersonRepo.Register(new Person
            {
                document = document,
                name = "Alert tester",
                address = "Address",
                phone = "0999999999",
                password = "Passw0rd!",
                oPersonType = new TypePerson { idPersonType = PersonTypeId() }
            });
            return PersonRepo.GetByDocument(document).idPerson;
        }

        [Fact]
        public void Insert_ThenGetOpenAlerts_ReturnsTheNewRow()
        {
            int categoryId = CreateCategory();
            int productId = CreateProduct(categoryId);
            int historyId = 0;

            try
            {
                historyId = Repository.Insert(productId, AlertType.Stock, AlertSeverity.Critical, 0m);

                Assert.True(historyId > 0);
                var open = Repository.GetOpenAlerts();
                Assert.Contains(open, o => o.Id == historyId && o.ProductId == productId && o.AlertType == AlertType.Stock && o.Severity == AlertSeverity.Critical);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product_alert_history WHERE id = @id", new SqlParameter("@id", historyId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
            }
        }

        [Fact]
        public void UpdateSeverity_ChangesSeverityOnTheOpenRow()
        {
            int categoryId = CreateCategory();
            int productId = CreateProduct(categoryId);
            int historyId = Repository.Insert(productId, AlertType.Stock, AlertSeverity.Low, 3m);

            try
            {
                Repository.UpdateSeverity(historyId, AlertSeverity.Critical, 0m);

                var open = Repository.GetOpenAlerts();
                var entry = Assert.Single(open, o => o.Id == historyId);
                Assert.Equal(AlertSeverity.Critical, entry.Severity);
                Assert.Equal(0m, entry.TriggerValue);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product_alert_history WHERE id = @id", new SqlParameter("@id", historyId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
            }
        }

        [Fact]
        public void Resolve_RemovesRowFromOpenAlerts()
        {
            int categoryId = CreateCategory();
            int productId = CreateProduct(categoryId);
            int historyId = Repository.Insert(productId, AlertType.Stock, AlertSeverity.Critical, 0m);

            try
            {
                Repository.Resolve(historyId);

                Assert.DoesNotContain(Repository.GetOpenAlerts(), o => o.Id == historyId);
                Assert.NotNull(SqlTestHelper.ExecuteScalar("SELECT resolved_at FROM product_alert_history WHERE id = @id", new SqlParameter("@id", historyId)));
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product_alert_history WHERE id = @id", new SqlParameter("@id", historyId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
            }
        }

        [Fact]
        public void Acknowledge_SetsAcknowledgedByAndAt()
        {
            int categoryId = CreateCategory();
            int productId = CreateProduct(categoryId);
            int personId = CreatePerson();
            int historyId = Repository.Insert(productId, AlertType.Stock, AlertSeverity.Critical, 0m);

            try
            {
                bool result = Repository.Acknowledge(historyId, personId);

                Assert.True(result);
                var entry = Repository.GetHistory(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1)).Single(h => h.Id == historyId);
                Assert.Equal(personId, entry.AcknowledgedBy);
                Assert.NotNull(entry.AcknowledgedAt);
                Assert.Equal("Alert tester", entry.AcknowledgedByName);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product_alert_history WHERE id = @id", new SqlParameter("@id", historyId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM person WHERE id = @id", new SqlParameter("@id", personId));
            }
        }

        [Fact]
        public void GetHistory_OutsideDateRange_IsExcluded()
        {
            int categoryId = CreateCategory();
            int productId = CreateProduct(categoryId);
            int historyId = Repository.Insert(productId, AlertType.Stock, AlertSeverity.Critical, 0m);

            try
            {
                var history = Repository.GetHistory(DateTime.Today.AddDays(-10), DateTime.Today.AddDays(-5));

                Assert.DoesNotContain(history, h => h.Id == historyId);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product_alert_history WHERE id = @id", new SqlParameter("@id", historyId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
            }
        }
    }
}
