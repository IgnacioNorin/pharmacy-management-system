using System.Collections.Generic;
using System.Data.SqlClient;
using PharmacySystem.Helpers;
using PharmacySystem.Data;
using PharmacySystem.Model;
using Xunit;

namespace PharmacySystem.Tests.Integration
{
    [Collection("Database")]
    public class SaleRepositoryTests
    {
        private static readonly ISaleRepository Repository = new SaleRepository(SqlConnectionFactory.FromConfiguration());
        private static readonly IPersonRepository PersonRepo = new PersonRepository(SqlConnectionFactory.FromConfiguration());
        private static readonly ICategoryRepository CategoryRepo = new CategoryRepository(SqlConnectionFactory.FromConfiguration());
        private static readonly IProductRepository ProductRepo = new ProductRepository(SqlConnectionFactory.FromConfiguration());

        private static int PersonTypeId()
        {
            return SqlTestHelper.ExecuteScalarInt("SELECT TOP 1 id FROM person_type");
        }

        private static Person CreatePerson(out string document)
        {
            document = SqlTestHelper.NewTag();
            PersonRepo.Register(new Person
            {
                document = document,
                name = "Sale tester",
                address = "Address",
                phone = "0999999999",
                password = "Passw0rd!",
                oPersonType = new TypePerson { idPersonType = PersonTypeId() }
            });
            return PersonRepo.GetByDocument(document);
        }

        [Fact]
        public void RegisterSale_ValidDetail_InsertsHeaderAndDetail()
        {
            Person person = CreatePerson(out string document);
            int categoryId = CategoryRepo.Register(new Categories { description = SqlTestHelper.NewTag() });
            int productId = ProductRepo.Register(new Product
            {
                code = SqlTestHelper.NewTag(),
                name = "Sale product",
                description = "Sale product",
                oCategory = new Categories { IdCategory = categoryId }
            });

            int saleId = 0;
            try
            {
                Sale sale = new Sale
                {
                    typeDocument = "Boleta",
                    oPerson = person,
                    documentClient = "9999999999",
                    nameClient = "Walk-in client",
                    totalPay = 15m,
                    payWith = 20m,
                    change = 5m,
                    oSaleDetail = new List<SaleDetail>
                    {
                        new SaleDetail
                        {
                            oProduct = new Product { idProduct = productId },
                            amount = 3,
                            salePrice = 5m,
                            subtotal = 15m
                        }
                    }
                };

                saleId = Repository.Register(sale);

                Assert.True(saleId > 0);
                Assert.Contains(Repository.ListSale(), s => s.idSale == saleId && s.nameClient == "Walk-in client");
                Assert.Contains(Repository.ListSaleDetail(), d => d.idSale == saleId && d.subtotal == 15m);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_detail WHERE sale_id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale WHERE id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM person WHERE document_number = @document", new SqlParameter("@document", document));
            }
        }

        [Fact]
        public void ControlStock_Subtract_DecreasesStock()
        {
            int categoryId = CategoryRepo.Register(new Categories { description = SqlTestHelper.NewTag() });
            int productId = ProductRepo.Register(new Product
            {
                code = SqlTestHelper.NewTag(),
                name = "Stock product",
                description = "Stock product",
                oCategory = new Categories { IdCategory = categoryId }
            });
            SqlTestHelper.ExecuteNonQuery("UPDATE product SET stock = 10 WHERE id = @id", new SqlParameter("@id", productId));

            try
            {
                bool result = Repository.ControlStock(productId, 4, subtract: true);

                Assert.True(result);
                Assert.Equal(6, SqlTestHelper.ExecuteScalarInt("SELECT stock FROM product WHERE id = @id", new SqlParameter("@id", productId)));
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
            }
        }

        [Fact]
        public void ControlStock_Add_IncreasesStock()
        {
            int categoryId = CategoryRepo.Register(new Categories { description = SqlTestHelper.NewTag() });
            int productId = ProductRepo.Register(new Product
            {
                code = SqlTestHelper.NewTag(),
                name = "Stock product",
                description = "Stock product",
                oCategory = new Categories { IdCategory = categoryId }
            });
            SqlTestHelper.ExecuteNonQuery("UPDATE product SET stock = 10 WHERE id = @id", new SqlParameter("@id", productId));

            try
            {
                bool result = Repository.ControlStock(productId, 4, subtract: false);

                Assert.True(result);
                Assert.Equal(14, SqlTestHelper.ExecuteScalarInt("SELECT stock FROM product WHERE id = @id", new SqlParameter("@id", productId)));
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
            }
        }

        // End-to-end reproduction of the real frmSale.cs cart flow (not just the CultureInfoHelper
        // unit): each line's subtotal is formatted for display and immediately parsed back, exactly
        // like CalculateTotal() does, to build the running total before the sale is persisted. Two
        // lines (100 x $12.50 and 1 x $5.00) push the total to $1,255.00, past the $1,000 threshold
        // that used to corrupt or throw. If this test passes, a real cashier ringing up a big sale
        // gets the correct total end to end: cart math -> DB persistence -> read-back.
        [Fact]
        public void RegisterSale_CartTotalCrossesThousandThreshold_PersistsExactAmount()
        {
            Person person = CreatePerson(out string document);
            int categoryId = CategoryRepo.Register(new Categories { description = SqlTestHelper.NewTag() });
            int productId = ProductRepo.Register(new Product
            {
                code = SqlTestHelper.NewTag(),
                name = "Bulk product",
                description = "Bulk product",
                oCategory = new Categories { IdCategory = categoryId }
            });

            int saleId = 0;
            try
            {
                var cartLines = new (int quantity, decimal unitPrice)[]
                {
                    (100, 12.50m),
                    (1, 5.00m)
                };

                decimal runningTotal = 0m;
                var saleDetails = new List<SaleDetail>();

                foreach (var line in cartLines)
                {
                    decimal lineSubtotal = line.quantity * line.unitPrice;

                    // Mirrors frmSale.cs: the grid stores the formatted string, not the decimal.
                    string formattedSubtotal = CultureInfoHelper.FormatAsCurrency(lineSubtotal);

                    // Mirrors CalculateTotal(): the running total is rebuilt by re-parsing every
                    // formatted cell, which is exactly where the old bug corrupted amounts >= 1000.
                    runningTotal += CultureInfoHelper.CultureInfoConverterStringToDecimal(formattedSubtotal);

                    saleDetails.Add(new SaleDetail
                    {
                        oProduct = new Product { idProduct = productId },
                        amount = line.quantity,
                        salePrice = line.unitPrice,
                        subtotal = CultureInfoHelper.CultureInfoConverterStringToDecimal(formattedSubtotal)
                    });
                }

                Assert.Equal(1255.00m, runningTotal);

                string formattedTotal = CultureInfoHelper.FormatAsCurrency(runningTotal);
                decimal totalToPay = CultureInfoHelper.CultureInfoConverterStringToDecimal(formattedTotal);
                decimal moneyToPay = CultureInfoHelper.CultureInfoConverterStringToDecimal(
                    CultureInfoHelper.FormatAsCurrency(1300.00m));
                decimal change = moneyToPay - totalToPay;

                Sale sale = new Sale
                {
                    typeDocument = "Factura",
                    oPerson = person,
                    documentClient = "9999999999",
                    nameClient = "Bulk buyer",
                    totalPay = totalToPay,
                    payWith = moneyToPay,
                    change = change,
                    oSaleDetail = saleDetails
                };

                saleId = Repository.Register(sale);

                Assert.True(saleId > 0);

                decimal persistedTotal = SqlTestHelper.ExecuteScalar(
                    "SELECT total_amount FROM sale WHERE id = @id", new SqlParameter("@id", saleId)) is decimal d ? d : default;

                Assert.Equal(1255.00m, persistedTotal);
                Assert.Equal(45.00m, change);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_detail WHERE sale_id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale WHERE id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM person WHERE document_number = @document", new SqlParameter("@document", document));
            }
        }
    }
}
