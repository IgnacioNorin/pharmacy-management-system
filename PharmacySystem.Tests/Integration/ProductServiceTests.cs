using System.Data.SqlClient;
using PharmacySystem.Logical;
using PharmacySystem.Model;
using Xunit;

namespace PharmacySystem.Tests.Integration
{
    [Collection("Database")]
    public class ProductServiceTests
    {
        private static int CreateCategory()
        {
            return CategoryService.Instance.RegisterCategory(new Categories { description = SqlTestHelper.NewTag() });
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
        public void RegisterProduct_New_IsListedAndVerifiable()
        {
            int categoryId = CreateCategory();
            int productId = 0;

            try
            {
                productId = ProductService.Instance.RegisterProduct(NewProduct(categoryId));

                Assert.True(productId > 0);
                Assert.True(ProductService.Instance.VerifyProduct(productId));
                Assert.Contains(ProductService.Instance.ListProduct(), p => p.idProduct == productId);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
            }
        }

        [Fact]
        public void RegisterProduct_DuplicateCode_ReturnsZero()
        {
            int categoryId = CreateCategory();
            string code = SqlTestHelper.NewTag();
            int firstId = ProductService.Instance.RegisterProduct(NewProduct(categoryId, code));

            try
            {
                int secondId = ProductService.Instance.RegisterProduct(NewProduct(categoryId, code));

                Assert.Equal(0, secondId);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", firstId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
            }
        }

        [Fact]
        public void UpdateProduct_ChangesFields()
        {
            int categoryId = CreateCategory();
            int productId = ProductService.Instance.RegisterProduct(NewProduct(categoryId));
            string newCode = SqlTestHelper.NewTag();

            try
            {
                bool result = ProductService.Instance.UpdateProduct(new Product
                {
                    idProduct = productId,
                    code = newCode,
                    name = "Updated name",
                    description = "Updated description",
                    oCategory = new Categories { IdCategory = categoryId }
                });

                Assert.True(result);
                Assert.Contains(ProductService.Instance.ListProduct(), p => p.idProduct == productId && p.code == newCode && p.name == "Updated name");
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
            }
        }

        [Fact]
        public void UpdateProduct_DuplicateCodeOnAnotherProduct_ReturnsFalse()
        {
            int categoryId = CreateCategory();
            string existingCode = SqlTestHelper.NewTag();
            int firstId = ProductService.Instance.RegisterProduct(NewProduct(categoryId, existingCode));
            int secondId = ProductService.Instance.RegisterProduct(NewProduct(categoryId));

            try
            {
                bool result = ProductService.Instance.UpdateProduct(new Product
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
        public void VerifyProduct_NonExistingId_ReturnsFalse()
        {
            Assert.False(ProductService.Instance.VerifyProduct(-1));
        }

        [Fact]
        public void DeleteProduct_NotReferencedByPurchaseOrSale_HardDeletesRow()
        {
            int categoryId = CreateCategory();
            int productId = ProductService.Instance.RegisterProduct(NewProduct(categoryId));

            try
            {
                bool result = ProductService.Instance.DeleteProduct(productId);

                Assert.True(result);
                Assert.Null(SqlTestHelper.ExecuteScalar("SELECT id FROM product WHERE id = @id", new SqlParameter("@id", productId)));
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
            }
        }

        [Fact]
        public void DeleteProduct_ReferencedByPurchaseOnly_SoftDeletesInstead()
        {
            int categoryId = CreateCategory();
            int productId = ProductService.Instance.RegisterProduct(NewProduct(categoryId));
            SqlTestHelper.ExecuteNonQuery(
                "INSERT INTO purchase_detail(purchase_id, product_id, stock, purchase_price, sale_price, total_amount) VALUES (NULL, @product_id, 1, 1.0, 1.0, 1.0)",
                new SqlParameter("@product_id", productId));

            try
            {
                bool result = ProductService.Instance.DeleteProduct(productId);

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
        public void DeleteProduct_ReferencedBySaleOnly_SoftDeletesInsteadOfViolatingForeignKey()
        {
            int categoryId = CreateCategory();
            int productId = ProductService.Instance.RegisterProduct(NewProduct(categoryId));
            SqlTestHelper.ExecuteNonQuery(
                "INSERT INTO sale_detail(sale_id, product_id, stock, sale_price, subtotal) VALUES (NULL, @product_id, 1, 1.0, 1.0)",
                new SqlParameter("@product_id", productId));

            try
            {
                bool result = ProductService.Instance.DeleteProduct(productId);

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
