using System.Data.SqlClient;
using PharmacySystem.Data;
using PharmacySystem.Model;
using Xunit;

namespace PharmacySystem.Tests.Integration
{
    // Was CategoryServiceTests, calling CategoryService.Instance. Now exercises
    // CategoryRepository directly. The cross-entity FK scenario below sets up its product
    // through ProductRepository directly too, just as test fixture data.
    [Collection("Database")]
    public class CategoryRepositoryTests
    {
        private static readonly ICategoryRepository Repository = new CategoryRepository(SqlConnectionFactory.FromConfiguration());
        private static readonly IProductRepository ProductRepo = new ProductRepository(SqlConnectionFactory.FromConfiguration());

        [Fact]
        public void Register_NewDescription_IsListedAsActive()
        {
            string description = SqlTestHelper.NewTag();
            int id = Repository.Register(new Categories { description = description });

            try
            {
                Assert.True(id > 0);
                Assert.Contains(Repository.List(), c => c.IdCategory == id && c.description == description);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", id));
            }
        }

        [Fact]
        public void Register_DuplicateDescription_ReturnsZero()
        {
            string description = SqlTestHelper.NewTag();
            int firstId = Repository.Register(new Categories { description = description });

            try
            {
                int secondId = Repository.Register(new Categories { description = description.ToUpperInvariant() });

                Assert.Equal(0, secondId);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", firstId));
            }
        }

        [Fact]
        public void Register_ReRegisteringSoftDeletedDescription_ReactivatesSameRow()
        {
            string description = SqlTestHelper.NewTag();
            int id = Repository.Register(new Categories { description = description });

            try
            {
                SqlTestHelper.ExecuteNonQuery("UPDATE category SET status = 0 WHERE id = @id", new SqlParameter("@id", id));

                int reactivatedId = Repository.Register(new Categories { description = description });

                Assert.Equal(id, reactivatedId);
                Assert.Contains(Repository.List(), c => c.IdCategory == id);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", id));
            }
        }

        [Fact]
        public void Update_NewDescription_IsPersisted()
        {
            int id = Repository.Register(new Categories { description = SqlTestHelper.NewTag() });
            string newDescription = SqlTestHelper.NewTag();

            try
            {
                bool result = Repository.Update(new Categories { IdCategory = id, description = newDescription });

                Assert.True(result);
                Assert.Contains(Repository.List(), c => c.IdCategory == id && c.description == newDescription);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", id));
            }
        }

        [Fact]
        public void Delete_NotReferencedByProducts_HardDeletesRow()
        {
            int id = Repository.Register(new Categories { description = SqlTestHelper.NewTag() });

            bool result = Repository.Delete(id);

            Assert.True(result);
            Assert.Null(SqlTestHelper.ExecuteScalar("SELECT id FROM category WHERE id = @id", new SqlParameter("@id", id)));
        }

        [Fact]
        public void Delete_ReferencedByProduct_SoftDeletesInstead()
        {
            int categoryId = Repository.Register(new Categories { description = SqlTestHelper.NewTag() });
            int productId = ProductRepo.Register(new Product
            {
                code = SqlTestHelper.NewTag(),
                name = "Test product",
                description = "Test product",
                oCategory = new Categories { IdCategory = categoryId }
            });

            try
            {
                bool result = Repository.Delete(categoryId);

                Assert.True(result);
                Assert.Equal(0, SqlTestHelper.ExecuteScalarInt("SELECT status FROM category WHERE id = @id", new SqlParameter("@id", categoryId)));
                Assert.DoesNotContain(Repository.List(), c => c.IdCategory == categoryId);

                // ...but it must still show in the product-form combo, because an active product
                // still points at it - otherwise editing that product silently reassigns it (DEF-10).
                Assert.Contains(Repository.ListForProductForm(), c => c.IdCategory == categoryId);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
            }
        }
    }
}
