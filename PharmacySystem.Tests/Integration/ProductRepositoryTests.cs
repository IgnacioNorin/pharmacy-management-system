using System;
using Microsoft.Data.SqlClient;
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
        public void SetSalePrice_ReleasesTheProduct_WritesHistory_AndSaleListPicksItUp()
        {
            int categoryId = CreateCategory();
            int productId = 0;

            try
            {
                productId = Repository.Register(NewProduct(categoryId));

                // A brand-new product is not released, so it is not sellable.
                Assert.DoesNotContain(Repository.ListSellable(), p => p.idProduct == productId);
                Assert.False(Repository.List().Single(p => p.idProduct == productId).isReleased);

                Assert.True(Repository.SetSalePrice(productId, 56.78m, "primera carga", null));

                Product listed = Repository.List().Single(p => p.idProduct == productId);
                Assert.Equal(56.78m, listed.salePrice);
                Assert.True(listed.isReleased);
                Assert.Contains(Repository.ListSellable(), p => p.idProduct == productId);

                // Re-price it.
                Assert.True(Repository.SetSalePrice(productId, 60m, "ajuste", null));

                var history = Repository.GetPriceHistory(productId);
                Assert.Equal(2, history.Count);
                Assert.Equal("cambio", history[0].EventType);       // newest first
                Assert.Equal(60m, history[0].SalePrice);
                Assert.Equal("liberacion", history[1].EventType);

                // Withdraw from sale.
                Assert.True(Repository.Unrelease(productId, "reformulacion", null));
                Assert.False(Repository.List().Single(p => p.idProduct == productId).isReleased);
                Assert.DoesNotContain(Repository.ListSellable(), p => p.idProduct == productId);
                Assert.Equal(3, Repository.GetPriceHistory(productId).Count);
                Assert.Equal("retiro", Repository.GetPriceHistory(productId)[0].EventType);

                // Unreleasing again is a no-op.
                Assert.False(Repository.Unrelease(productId, "otra vez", null));
                Assert.False(Repository.SetSalePrice(-1, 1m, "x", null));
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product_price_history WHERE product_id = @id", new SqlParameter("@id", productId));
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

        [Fact]
        public void GetSellableByCodeAndId_ReturnOnlyReleasedProducts()
        {
            int categoryId = CreateCategory();
            string code = SqlTestHelper.NewTag();
            int productId = 0;

            try
            {
                productId = Repository.Register(NewProduct(categoryId, code));

                // Not released yet -> not sellable, so the targeted lookups skip it.
                Assert.Null(Repository.GetSellableByCode(code));
                Assert.Null(Repository.GetSellableById(productId));

                Assert.True(Repository.SetSalePrice(productId, 10m, "alta", null));

                Assert.Equal(productId, Repository.GetSellableByCode(code).idProduct);
                Assert.Equal(code, Repository.GetSellableById(productId).code);
                Assert.Null(Repository.GetSellableByCode("no-such-code"));
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product_price_history WHERE product_id = @id", new SqlParameter("@id", productId));
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

        [Fact]
        public void ListPaged_SlicesTheResult_ReportsTheTotal_AndFiltersBySearch()
        {
            int categoryId = CreateCategory();
            string tag = SqlTestHelper.NewTag();
            var ids = new System.Collections.Generic.List<int>();

            try
            {
                // 7 products that share a searchable token in the name.
                for (int i = 0; i < 7; i++)
                {
                    var p = NewProduct(categoryId);
                    p.name = tag + " item " + i.ToString("D2");
                    ids.Add(Repository.Register(p));
                }

                PagedResult<Product> page1 = Repository.ListPaged(1, 3, tag);
                Assert.Equal(7, page1.TotalCount);
                Assert.Equal(3, page1.Items.Count);
                Assert.Equal(3, page1.TotalPages);
                Assert.All(page1.Items, p => Assert.NotNull(p.oCategory)); // multi-map still wires the category

                PagedResult<Product> page3 = Repository.ListPaged(3, 3, tag);
                Assert.Single(page3.Items);

                // Ordered by name, so paging does not repeat or skip rows.
                var seen = page1.Items.Concat(Repository.ListPaged(2, 3, tag).Items).Concat(page3.Items)
                    .Select(p => p.idProduct).ToList();
                Assert.Equal(7, seen.Distinct().Count());

                Assert.Equal(0, Repository.ListPaged(1, 10, "no-such-token-" + tag).TotalCount);
            }
            finally
            {
                foreach (int id in ids)
                {
                    SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", id));
                }
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
            }
        }

        [Fact]
        public void Report_StockCostValue_SumsEachLotAtItsOwnCost()
        {
            int categoryId = CreateCategory();
            string code = SqlTestHelper.NewTag();
            int productId = Repository.Register(NewProduct(categoryId, code));
            SqlTestHelper.ExecuteNonQuery("UPDATE product SET stock = 8 WHERE id = @id", new SqlParameter("@id", productId));
            SqlTestHelper.ExecuteNonQuery(
                "INSERT INTO product_lot(product_id, quantity, date_expired, unit_cost) VALUES (@p, 3, NULL, 10), (@p, 5, NULL, 4)",
                new SqlParameter("@p", productId));

            try
            {
                var row = Repository.Report(categoryId.ToString()).Single(r => r.Code == code);

                // 3*10 + 5*4 = 50, not stock * a single price.
                Assert.Equal(50m, row.StockCostValue);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product_lot WHERE product_id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
            }
        }
    }
}
