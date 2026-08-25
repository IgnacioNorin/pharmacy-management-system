using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using PharmacySystem.Data;
using PharmacySystem.Logical;
using PharmacySystem.Model;
using Xunit;

namespace PharmacySystem.Tests.Integration
{
    // Was PurchaseServiceTests, calling PurchaseService.Instance. Now exercises
    // PurchaseRepository directly (ReportPurchase() has no repository equivalent yet, same
    // reason as ProductRepository's Report()). Person/Supplier/Category/Product setup keeps
    // using the already-migrated adapters, since that's just test fixture data.
    [Collection("Database")]
    public class PurchaseRepositoryTests
    {
        private static readonly IPurchaseRepository Repository = new PurchaseRepository(SqlConnectionFactory.FromConfiguration());

        private static int PersonTypeId()
        {
            return SqlTestHelper.ExecuteScalarInt("SELECT TOP 1 id FROM person_type");
        }

        [Fact]
        public void Register_ValidDetail_InsertsRowsAndUpdatesProductStock()
        {
            string document = SqlTestHelper.NewTag();
            PersonService.Instance.RegisterPerson(new Person
            {
                document = document,
                name = "Purchase tester",
                address = "Address",
                phone = "0999999999",
                password = "Passw0rd!",
                oPersonType = new TypePerson { idPersonType = PersonTypeId() }
            });
            Person person = PersonService.Instance.GetPersonByDocument(document);

            int supplierId = SupplierService.Instance.RegisterSupplier(new Supplier
            {
                document = SqlTestHelper.NewTag(),
                companyName = "Purchase supplier",
                email = "supplier@test.local",
                phone = "0999999999"
            });

            int categoryId = CategoryService.Instance.RegisterCategory(new Categories { description = SqlTestHelper.NewTag() });
            int productId = ProductService.Instance.RegisterProduct(new Product
            {
                code = SqlTestHelper.NewTag(),
                name = "Purchase product",
                description = "Purchase product",
                oCategory = new Categories { IdCategory = categoryId }
            });

            int purchaseId = 0;
            try
            {
                Purchase purchase = new Purchase
                {
                    oPerson = person,
                    oSupplier = new Supplier { idSupplier = supplierId },
                    totalAmount = 50m,
                    documentType = "Factura",
                    documentNumber = SqlTestHelper.NewTag(),
                    oPurchaseDetail = new List<PurchaseDetail>
                    {
                        new PurchaseDetail
                        {
                            oProduct = new Product { idProduct = productId },
                            quantity = 10,
                            expirationDate = DateTime.Today.AddYears(1),
                            purchasePrice = 3m,
                            salePrice = 5m,
                            total = 30m
                        }
                    }
                };

                bool result = Repository.Register(purchase);

                Assert.True(result);
                purchaseId = SqlTestHelper.ExecuteScalarInt("SELECT id FROM purchase WHERE document_number = @doc", new SqlParameter("@doc", purchase.documentNumber));
                Assert.True(purchaseId > 0);

                int stock = SqlTestHelper.ExecuteScalarInt("SELECT stock FROM product WHERE id = @id", new SqlParameter("@id", productId));
                Assert.Equal(10, stock);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM purchase_detail WHERE purchase_id = @id", new SqlParameter("@id", purchaseId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM purchase WHERE id = @id", new SqlParameter("@id", purchaseId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM supplier WHERE id = @id", new SqlParameter("@id", supplierId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM person WHERE document_number = @document", new SqlParameter("@document", document));
            }
        }

        [Fact]
        public void GetTotalAmount_SumsPurchasesInDateRangeForSupplier()
        {
            string document = SqlTestHelper.NewTag();
            PersonService.Instance.RegisterPerson(new Person
            {
                document = document,
                name = "Report tester",
                address = "Address",
                phone = "0999999999",
                password = "Passw0rd!",
                oPersonType = new TypePerson { idPersonType = PersonTypeId() }
            });
            Person person = PersonService.Instance.GetPersonByDocument(document);

            int supplierId = SupplierService.Instance.RegisterSupplier(new Supplier
            {
                document = SqlTestHelper.NewTag(),
                companyName = "Report supplier",
                email = "supplier@test.local",
                phone = "0999999999"
            });

            int categoryId = CategoryService.Instance.RegisterCategory(new Categories { description = SqlTestHelper.NewTag() });
            int productId = ProductService.Instance.RegisterProduct(new Product
            {
                code = SqlTestHelper.NewTag(),
                name = "Report product",
                description = "Report product",
                oCategory = new Categories { IdCategory = categoryId }
            });

            int purchaseId = 0;
            try
            {
                string documentNumber = SqlTestHelper.NewTag();
                Purchase purchase = new Purchase
                {
                    oPerson = person,
                    oSupplier = new Supplier { idSupplier = supplierId },
                    totalAmount = 42.50m,
                    documentType = "Factura",
                    documentNumber = documentNumber,
                    oPurchaseDetail = new List<PurchaseDetail>
                    {
                        new PurchaseDetail
                        {
                            oProduct = new Product { idProduct = productId },
                            quantity = 1,
                            expirationDate = DateTime.Today.AddYears(1),
                            purchasePrice = 42.50m,
                            salePrice = 60m,
                            total = 42.50m
                        }
                    }
                };
                Assert.True(Repository.Register(purchase));
                purchaseId = SqlTestHelper.ExecuteScalarInt("SELECT id FROM purchase WHERE document_number = @doc", new SqlParameter("@doc", documentNumber));

                // Pin the purchase date instead of relying on the getdate() default and a
                // DateTime.Today range: the application clock and the SQL Server clock can sit on
                // opposite sides of midnight when they run in different time zones, so the row is
                // stamped with one date while the range is built from another and matches nothing.
                const string purchaseDay = "2026-03-17";
                DateTime purchaseDate = new DateTime(2026, 3, 17);
                SqlTestHelper.ExecuteNonQuery(
                    "UPDATE purchase SET date_registered = @date WHERE id = @id",
                    new SqlParameter("@date", purchaseDate),
                    new SqlParameter("@id", purchaseId));

                decimal totalAmount = Repository.GetTotalAmount(supplierId.ToString(), purchaseDay, purchaseDay);

                Assert.Equal(42.50m, totalAmount);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM purchase_detail WHERE purchase_id = @id", new SqlParameter("@id", purchaseId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM purchase WHERE id = @id", new SqlParameter("@id", purchaseId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM supplier WHERE id = @id", new SqlParameter("@id", supplierId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM person WHERE document_number = @document", new SqlParameter("@document", document));
            }
        }
    }
}
