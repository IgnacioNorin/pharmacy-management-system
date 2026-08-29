using System;
using System.Data.SqlClient;
using System.Linq;
using PharmacySystem.Data;
using PharmacySystem.Model;
using Xunit;

namespace PharmacySystem.Tests.Integration
{
    // Was ProductServiceTests, calling ProductService.Instance. Now exercises
    // ProductRepository directly (Report() has no repository equivalent yet - see
    // IProductRepository's comment). Category setup goes through CategoryRepository directly too.
    [Collection("Database")]
    public class ProductRepositoryTests
    {
        private static readonly IProductRepository Repository = new ProductRepository(SqlConnectionFactory.FromConfiguration());
        private static readonly ICategoryRepository CategoryRepo = new CategoryRepository(SqlConnectionFactory.FromConfiguration());

        private static int CreateCategory()
        {
            return CategoryRepo.Register(new Categories { description = SqlTestHelper.NewTag() });
        }

        private static Product NewProduct(int categoryId, string code = null)
        {
            return new Product
            {
                code = code ?? SqlTestHelper.NewTag(),
                name = "Test product",
                description = "Test product",
                oCategory = new Categories { IdCategory = categoryId }
            };
        }

        [Fact]
        public void SetPrices_WritesBothColumns_AndListReflectsThem()
        {
            int categoryId = CreateCategory();
            int productId = 0;

            try
            {
                productId = Repository.Register(NewProduct(categoryId));

                Assert.True(Repository.SetPrices(productId, 12.34m, 56.78m));

                Product listed = Repository.List().Single(p => p.idProduct == productId);
                Assert.Equal(12.34m, listed.purchasePrice);
                Assert.Equal(56.78m, listed.salePrice);

                // A second call overwrites, and a non-existent id returns false without throwing.
                Assert.True(Repository.SetPrices(productId, 1m, 2m));
                Assert.False(Repository.SetPrices(-1, 1m, 2m));
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
            }
        }

        [Fact]
        public void Register_New_IsListedAndVerifiable()
        {
            int categoryId = CreateCategory();
            int productId = 0;

            try
            {
                productId = Repository.Register(NewProduct(categoryId));

                Assert.True(productId > 0);
                Assert.True(Repository.Verify(productId));
                Assert.Contains(Repository.List(), p => p.idProduct == productId);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
            }
        }

        // Regression test for the Dapper migration: List() used to build Product manually with
        // `expirationDate = Convert.ToDateTime(date)` where `date` was null for a NULL DB column,
        // and Convert.ToDateTime(null) returns default(DateTime) rather than throwing. Dapper must
        // reproduce that exact fallback - a NULL date_expired should leave expirationDate at
        // default(DateTime), not throw and not silently corrupt the row.
        [Fact]
        public void List_ProductWithoutExpirationDate_DefaultsExpirationDateInsteadOfThrowing()
        {
            int categoryId = CreateCategory();
            int productId = Repository.Register(NewProduct(categoryId));

            try
            {
                Product product = Repository.List().Single(p => p.idProduct == productId);

                Assert.Equal(default(DateTime), product.expirationDate);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
            }
        }

        [Fact]
        public void Register_DuplicateCode_ReturnsZero()
        {
            int categoryId = CreateCategory();
            string code = SqlTestHelper.NewTag();
            int firstId = Repository.Register(NewProduct(categoryId, code));

            try
            {
                int secondId = Repository.Register(NewProduct(categoryId, code));

                Assert.Equal(0, secondId);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", firstId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
            }
        }

        [Fact]
        public void Update_ChangesFields()
        {
            int categoryId = CreateCategory();
            int productId = Repository.Register(NewProduct(categoryId));
            string newCode = SqlTestHelper.NewTag();

            try
            {
                bool result = Repository.Update(new Product
                {
                    idProduct = productId,
                    code = newCode,
                    name = "Updated name",
                    description = "Updated description",
                    taxAffected = false,
                    oCategory = new Categories { IdCategory = categoryId }
                });

                Assert.True(result);
                Product updated = Repository.List().Single(p => p.idProduct == productId);
                Assert.Equal(newCode, updated.code);
                Assert.Equal("Updated name", updated.name);
                Assert.False(updated.taxAffected); // exempt flag round-trips
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
            }
        }

        [Fact]
        public void Update_DuplicateCodeOnAnotherProduct_ReturnsFalse()
        {
            int categoryId = CreateCategory();
            string existingCode = SqlTestHelper.NewTag();
            int firstId = Repository.Register(NewProduct(categoryId, existingCode));
            int secondId = Repository.Register(NewProduct(categoryId));

            try
            {
                bool result = Repository.Update(new Product
                {
                    idProduct = secondId,
                    code = existingCode,
                    name = "Updated name",
                    description = "Updated description",
                    oCategory = new Categories { IdCategory = categoryId }
                });

                Assert.False(result);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id IN (@id1, @id2)",
                    new SqlParameter("@id1", firstId), new SqlParameter("@id2", secondId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
            }
        }

        [Fact]
        public void Verify_NonExistingId_ReturnsFalse()
        {
            Assert.False(Repository.Verify(-1));
        }

        [Fact]
        public void Delete_NotReferencedByPurchaseOrSale_HardDeletesRow()
        {
            int categoryId = CreateCategory();
            int productId = Repository.Register(NewProduct(categoryId));

            try
            {
                bool result = Repository.Delete(productId);

                Assert.True(result);
                Assert.Null(SqlTestHelper.ExecuteScalar("SELECT id FROM product WHERE id = @id", new SqlParameter("@id", productId)));
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
            }
        }

        [Fact]
        public void Delete_ReferencedByPurchaseOnly_SoftDeletesInstead()
        {
            int categoryId = CreateCategory();
            int productId = Repository.Register(NewProduct(categoryId));
            SqlTestHelper.ExecuteNonQuery(
                "INSERT INTO purchase_detail(purchase_id, product_id, stock, purchase_price, sale_price, total_amount) VALUES (NULL, @product_id, 1, 1.0, 1.0, 1.0)",
                new SqlParameter("@product_id", productId));

            try
            {
                bool result = Repository.Delete(productId);

                Assert.True(result);
                Assert.Equal(0, SqlTestHelper.ExecuteScalarInt("SELECT status FROM product WHERE id = @id", new SqlParameter("@id", productId)));
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM purchase_detail WHERE product_id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
            }
        }

        // Regression test for the sp_delete_product fix: before it, only purchase_detail was
        // checked, so a product that had only ever been sold (never purchased) would hit the
        // physical DELETE branch and violate the FK from sale_detail.
        [Fact]
        public void Delete_ReferencedBySaleOnly_SoftDeletesInsteadOfViolatingForeignKey()
        {
            int categoryId = CreateCategory();
            int productId = Repository.Register(NewProduct(categoryId));
            SqlTestHelper.ExecuteNonQuery(
                "INSERT INTO sale_detail(sale_id, product_id, stock, sale_price, subtotal) VALUES (NULL, @product_id, 1, 1.0, 1.0)",
                new SqlParameter("@product_id", productId));

            try
            {
                bool result = Repository.Delete(productId);

                Assert.True(result);
                Assert.NotNull(SqlTestHelper.ExecuteScalar("SELECT id FROM product WHERE id = @id", new SqlParameter("@id", productId)));
                Assert.Equal(0, SqlTestHelper.ExecuteScalarInt("SELECT status FROM product WHERE id = @id", new SqlParameter("@id", productId)));
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_detail WHERE product_id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
            }
        }
    }
}
