using System.Data.SqlClient;
using System.Linq;
using PharmacySystem.Logical;
using PharmacySystem.Model;
using Xunit;

namespace PharmacySystem.Tests.Integration
{
    [Collection("Database")]
    public class CategoryServiceTests
    {
        [Fact]
        public void RegisterCategory_NewDescription_IsListedAsActive()
        {
            string description = SqlTestHelper.NewTag();
            int id = CategoryService.Instance.RegisterCategory(new Categories { description = description });

            try
            {
                Assert.True(id > 0);
                Assert.Contains(CategoryService.Instance.ListCategory(), c => c.IdCategory == id && c.description == description);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", id));
            }
        }

        [Fact]
        public void RegisterCategory_DuplicateDescription_ReturnsZero()
        {
            string description = SqlTestHelper.NewTag();
            int firstId = CategoryService.Instance.RegisterCategory(new Categories { description = description });

            try
            {
                int secondId = CategoryService.Instance.RegisterCategory(new Categories { description = description.ToUpperInvariant() });

                Assert.Equal(0, secondId);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", firstId));
            }
        }

        [Fact]
        public void RegisterCategory_ReRegisteringSoftDeletedDescription_ReactivatesSameRow()
        {
            string description = SqlTestHelper.NewTag();
            int id = CategoryService.Instance.RegisterCategory(new Categories { description = description });

            try
            {
                SqlTestHelper.ExecuteNonQuery("UPDATE category SET status = 0 WHERE id = @id", new SqlParameter("@id", id));

                int reactivatedId = CategoryService.Instance.RegisterCategory(new Categories { description = description });

                Assert.Equal(id, reactivatedId);
                Assert.Contains(CategoryService.Instance.ListCategory(), c => c.IdCategory == id);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", id));
            }
        }

        [Fact]
        public void UpdateCategory_NewDescription_IsPersisted()
        {
            int id = CategoryService.Instance.RegisterCategory(new Categories { description = SqlTestHelper.NewTag() });
            string newDescription = SqlTestHelper.NewTag();

            try
            {
                bool result = CategoryService.Instance.UpdateCategory(new Categories { IdCategory = id, description = newDescription });

                Assert.True(result);
                Assert.Contains(CategoryService.Instance.ListCategory(), c => c.IdCategory == id && c.description == newDescription);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", id));
            }
        }

        [Fact]
        public void DeleteCategory_NotReferencedByProducts_HardDeletesRow()
        {
            int id = CategoryService.Instance.RegisterCategory(new Categories { description = SqlTestHelper.NewTag() });

            bool result = CategoryService.Instance.DeleteCategory(id);

            Assert.True(result);
            Assert.Null(SqlTestHelper.ExecuteScalar("SELECT id FROM category WHERE id = @id", new SqlParameter("@id", id)));
        }

        [Fact]
        public void DeleteCategory_ReferencedByProduct_SoftDeletesInstead()
        {
            int categoryId = CategoryService.Instance.RegisterCategory(new Categories { description = SqlTestHelper.NewTag() });
            int productId = ProductService.Instance.RegisterProduct(new Product
            {
                code = SqlTestHelper.NewTag(),
                name = "Test product",
                description = "Test product",
                oCategory = new Categories { IdCategory = categoryId }
            });

            try
            {
                bool result = CategoryService.Instance.DeleteCategory(categoryId);

                Assert.True(result);
                Assert.Equal(0, SqlTestHelper.ExecuteScalarInt("SELECT status FROM category WHERE id = @id", new SqlParameter("@id", categoryId)));
                Assert.DoesNotContain(CategoryService.Instance.ListCategory(), c => c.IdCategory == categoryId);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
            }
        }
    }
}
