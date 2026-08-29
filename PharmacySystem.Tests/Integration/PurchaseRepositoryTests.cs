using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using PharmacySystem.Data;
using PharmacySystem.Model;
using Xunit;

namespace PharmacySystem.Tests.Integration
{
    // Was PurchaseServiceTests, calling PurchaseService.Instance. Now exercises
    // PurchaseRepository directly (ReportPurchase() has no repository equivalent yet, same
    // reason as ProductRepository's Report()). Person/Supplier/Category/Product setup goes
    // through their repositories directly too, since that's just test fixture data.
    [Collection("Database")]
    public class PurchaseRepositoryTests
    {
        private static readonly IPurchaseRepository Repository = new PurchaseRepository(SqlConnectionFactory.FromConfiguration());
        private static readonly IPersonRepository PersonRepo = new PersonRepository(SqlConnectionFactory.FromConfiguration());
        private static readonly ISupplierRepository SupplierRepo = new SupplierRepository(SqlConnectionFactory.FromConfiguration());
        private static readonly ICategoryRepository CategoryRepo = new CategoryRepository(SqlConnectionFactory.FromConfiguration());
        private static readonly IProductRepository ProductRepo = new ProductRepository(SqlConnectionFactory.FromConfiguration());

        private static int PersonTypeId()
        {
            return SqlTestHelper.ExecuteScalarInt("SELECT TOP 1 id FROM person_type");
        }

        [Fact]
        public void Register_ValidDetail_InsertsRowsAndUpdatesProductStock()
        {
            string document = SqlTestHelper.NewTag();
            PersonRepo.Register(new Person
            {
                document = document,
                name = "Purchase tester",
                address = "Address",
                phone = "0999999999",
                password = "Passw0rd!",
                oPersonType = new TypePerson { idPersonType = PersonTypeId() }
            });
            Person person = PersonRepo.GetByDocument(document);

            int supplierId = SupplierRepo.Register(new Supplier
            {
                document = SqlTestHelper.NewTag(),
                companyName = "Purchase supplier",
                email = "supplier@test.local",
                phone = "0999999999"
            });

            int categoryId = CategoryRepo.Register(new Categories { description = SqlTestHelper.NewTag() });
            int productId = ProductRepo.Register(new Product
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
            PersonRepo.Register(new Person
            {
                document = document,
                name = "Report tester",
                address = "Address",
                phone = "0999999999",
                password = "Passw0rd!",
                oPersonType = new TypePerson { idPersonType = PersonTypeId() }
            });
            Person person = PersonRepo.GetByDocument(document);

            int supplierId = SupplierRepo.Register(new Supplier
            {
                document = SqlTestHelper.NewTag(),
                companyName = "Report supplier",
                email = "supplier@test.local",
                phone = "0999999999"
            });

            int categoryId = CategoryRepo.Register(new Categories { description = SqlTestHelper.NewTag() });
            int productId = ProductRepo.Register(new Product
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
                DateTime purchaseDate = new DateTime(2026, 3, 17);
                SqlTestHelper.ExecuteNonQuery(
                    "UPDATE purchase SET date_registered = @date WHERE id = @id",
                    new SqlParameter("@date", purchaseDate),
                    new SqlParameter("@id", purchaseId));

                decimal totalAmount = Repository.GetTotalAmount(supplierId.ToString(), purchaseDate, purchaseDate);

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

        // DEF-02 (option B): each purchase line keeps its own expiry in purchase_detail, and the
        // product master's date_expired only moves EARLIER - a newer lot with a later expiry must
        // not push it forward and switch off the alert for older stock still on the shelf.
        [Fact]
        public void Register_ProductExpiryOnlyMovesEarlier_AndLotExpiryIsKeptOnTheDetailLine()
        {
            string document = SqlTestHelper.NewTag();
            PersonRepo.Register(new Person
            {
                document = document,
                name = "Expiry tester",
                address = "Address",
                phone = "0999999999",
                password = "Passw0rd!",
                oPersonType = new TypePerson { idPersonType = PersonTypeId() }
            });
            Person person = PersonRepo.GetByDocument(document);

            int supplierId = SupplierRepo.Register(new Supplier
            {
                document = SqlTestHelper.NewTag(),
                companyName = "Expiry supplier",
                email = "supplier@test.local",
                phone = "0999999999"
            });

            int categoryId = CategoryRepo.Register(new Categories { description = SqlTestHelper.NewTag() });
            int productId = ProductRepo.Register(new Product
            {
                code = SqlTestHelper.NewTag(),
                name = "Expiry product",
                description = "Expiry product",
                oCategory = new Categories { IdCategory = categoryId }
            });

            var purchaseIds = new List<int>();
            DateTime near = DateTime.Today.AddDays(30);
            DateTime far = DateTime.Today.AddDays(365);
            DateTime nearest = DateTime.Today.AddDays(10);
            try
            {
                int RegisterLot(DateTime expiry)
                {
                    string doc = SqlTestHelper.NewTag();
                    Assert.True(Repository.Register(new Purchase
                    {
                        oPerson = person,
                        oSupplier = new Supplier { idSupplier = supplierId },
                        totalAmount = 10m,
                        documentType = "Factura",
                        documentNumber = doc,
                        oPurchaseDetail = new List<PurchaseDetail>
                        {
                            new PurchaseDetail { oProduct = new Product { idProduct = productId }, quantity = 1, expirationDate = expiry, purchasePrice = 3m, total = 3m }
                        }
                    }));
                    int id = SqlTestHelper.ExecuteScalarInt("SELECT id FROM purchase WHERE document_number = @doc", new SqlParameter("@doc", doc));
                    purchaseIds.Add(id);
                    return id;
                }

                int firstId = RegisterLot(near);
                int daysAfterFirst = SqlTestHelper.ExecuteScalarInt(
                    "SELECT DATEDIFF(DAY, CAST(@e AS date), CAST(date_expired AS date)) FROM product WHERE id = @id",
                    new SqlParameter("@e", near), new SqlParameter("@id", productId));
                Assert.Equal(0, daysAfterFirst);

                // Later expiry: the product date must stay on the nearer 'near' lot.
                RegisterLot(far);
                int daysAfterFar = SqlTestHelper.ExecuteScalarInt(
                    "SELECT DATEDIFF(DAY, CAST(@e AS date), CAST(date_expired AS date)) FROM product WHERE id = @id",
                    new SqlParameter("@e", near), new SqlParameter("@id", productId));
                Assert.Equal(0, daysAfterFar);

                // Nearer expiry: the product date does move earlier.
                RegisterLot(nearest);
                int daysAfterNearest = SqlTestHelper.ExecuteScalarInt(
                    "SELECT DATEDIFF(DAY, CAST(@e AS date), CAST(date_expired AS date)) FROM product WHERE id = @id",
                    new SqlParameter("@e", nearest), new SqlParameter("@id", productId));
                Assert.Equal(0, daysAfterNearest);

                // Every lot kept its own expiry on the detail line.
                int distinctLotExpiries = SqlTestHelper.ExecuteScalarInt(
                    "SELECT COUNT(DISTINCT CAST(date_expired AS date)) FROM purchase_detail WHERE product_id = @id AND purchase_id IN (@a, @b, @c)",
                    new SqlParameter("@id", productId),
                    new SqlParameter("@a", firstId), new SqlParameter("@b", purchaseIds[1]), new SqlParameter("@c", purchaseIds[2]));
                Assert.Equal(3, distinctLotExpiries);
            }
            finally
            {
                foreach (int id in purchaseIds)
                {
                    SqlTestHelper.ExecuteNonQuery("DELETE FROM purchase_detail WHERE purchase_id = @id", new SqlParameter("@id", id));
                    SqlTestHelper.ExecuteNonQuery("DELETE FROM purchase WHERE id = @id", new SqlParameter("@id", id));
                }
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM supplier WHERE id = @id", new SqlParameter("@id", supplierId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM person WHERE document_number = @document", new SqlParameter("@document", document));
            }
        }

        // Regression test: GetTotalAmount used to join purchase_detail, so a purchase with N
        // detail lines had its header total_amount summed N times. Here one purchase with a
        // header total of 100 and two detail lines must still total 100, not 200.
        [Fact]
        public void GetTotalAmount_PurchaseWithMultipleDetailLines_CountsHeaderTotalOnce()
        {
            string document = SqlTestHelper.NewTag();
            PersonRepo.Register(new Person
            {
                document = document,
                name = "Multi-line tester",
                address = "Address",
                phone = "0999999999",
                password = "Passw0rd!",
                oPersonType = new TypePerson { idPersonType = PersonTypeId() }
            });
            Person person = PersonRepo.GetByDocument(document);

            int supplierId = SupplierRepo.Register(new Supplier
            {
                document = SqlTestHelper.NewTag(),
                companyName = "Multi-line supplier",
                email = "supplier@test.local",
                phone = "0999999999"
            });

            int categoryId = CategoryRepo.Register(new Categories { description = SqlTestHelper.NewTag() });
            int productA = ProductRepo.Register(new Product { code = SqlTestHelper.NewTag(), name = "A", description = "A", oCategory = new Categories { IdCategory = categoryId } });
            int productB = ProductRepo.Register(new Product { code = SqlTestHelper.NewTag(), name = "B", description = "B", oCategory = new Categories { IdCategory = categoryId } });

            int purchaseId = 0;
            try
            {
                string documentNumber = SqlTestHelper.NewTag();
                Assert.True(Repository.Register(new Purchase
                {
                    oPerson = person,
                    oSupplier = new Supplier { idSupplier = supplierId },
                    totalAmount = 100m,
                    documentType = "Factura",
                    documentNumber = documentNumber,
                    oPurchaseDetail = new List<PurchaseDetail>
                    {
                        new PurchaseDetail { oProduct = new Product { idProduct = productA }, quantity = 1, expirationDate = DateTime.Today.AddYears(1), purchasePrice = 40m, total = 40m },
                        new PurchaseDetail { oProduct = new Product { idProduct = productB }, quantity = 1, expirationDate = DateTime.Today.AddYears(1), purchasePrice = 60m, total = 60m }
                    }
                }));
                purchaseId = SqlTestHelper.ExecuteScalarInt("SELECT id FROM purchase WHERE document_number = @doc", new SqlParameter("@doc", documentNumber));

                DateTime purchaseDate = new DateTime(2026, 3, 18);
                SqlTestHelper.ExecuteNonQuery("UPDATE purchase SET date_registered = @date WHERE id = @id",
                    new SqlParameter("@date", purchaseDate), new SqlParameter("@id", purchaseId));

                decimal totalAmount = Repository.GetTotalAmount(supplierId.ToString(), purchaseDate, purchaseDate);

                Assert.Equal(100m, totalAmount);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM purchase_detail WHERE purchase_id = @id", new SqlParameter("@id", purchaseId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM purchase WHERE id = @id", new SqlParameter("@id", purchaseId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id IN (@a, @b)", new SqlParameter("@a", productA), new SqlParameter("@b", productB));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM supplier WHERE id = @id", new SqlParameter("@id", supplierId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM person WHERE document_number = @document", new SqlParameter("@document", document));
            }
        }
    }
}
