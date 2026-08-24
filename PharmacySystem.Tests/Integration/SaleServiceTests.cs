using System.Collections.Generic;
using System.Data.SqlClient;
using PharmacySystem.Logical;
using PharmacySystem.Model;
using Xunit;

namespace PharmacySystem.Tests.Integration
{
    [Collection("Database")]
    public class SaleServiceTests
    {
        private static int PersonTypeId()
        {
            return SqlTestHelper.ExecuteScalarInt("SELECT TOP 1 id FROM person_type");
        }

        private static Person CreatePerson(out string document)
        {
            document = SqlTestHelper.NewTag();
            PersonService.Instance.RegisterPerson(new Person
            {
                document = document,
                name = "Sale tester",
                address = "Address",
                phone = "0999999999",
                password = "Passw0rd!",
                oPersonType = new TypePerson { idPersonType = PersonTypeId() }
            });
            return PersonService.Instance.GetPersonByDocument(document);
        }

        [Fact]
        public void RegisterSale_ValidDetail_InsertsHeaderAndDetail()
        {
            Person person = CreatePerson(out string document);
            int categoryId = CategoryService.Instance.RegisterCategory(new Categories { description = SqlTestHelper.NewTag() });
            int productId = ProductService.Instance.RegisterProduct(new Product
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

                saleId = SaleService.Instance.RegisterSale(sale);

                Assert.True(saleId > 0);
                Assert.Contains(SaleService.Instance.ListSale(), s => s.idSale == saleId && s.nameClient == "Walk-in client");
                Assert.Contains(SaleService.Instance.ListSaleDetail(), d => d.idSale == saleId && d.subtotal == 15m);
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
            int categoryId = CategoryService.Instance.RegisterCategory(new Categories { description = SqlTestHelper.NewTag() });
            int productId = ProductService.Instance.RegisterProduct(new Product
            {
                code = SqlTestHelper.NewTag(),
                name = "Stock product",
                description = "Stock product",
                oCategory = new Categories { IdCategory = categoryId }
            });
            SqlTestHelper.ExecuteNonQuery("UPDATE product SET stock = 10 WHERE id = @id", new SqlParameter("@id", productId));

            try
            {
                bool result = SaleService.Instance.ControlStock(productId, 4, subtract: true);

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
            int categoryId = CategoryService.Instance.RegisterCategory(new Categories { description = SqlTestHelper.NewTag() });
            int productId = ProductService.Instance.RegisterProduct(new Product
            {
                code = SqlTestHelper.NewTag(),
                name = "Stock product",
                description = "Stock product",
                oCategory = new Categories { IdCategory = categoryId }
            });
            SqlTestHelper.ExecuteNonQuery("UPDATE product SET stock = 10 WHERE id = @id", new SqlParameter("@id", productId));

            try
            {
                bool result = SaleService.Instance.ControlStock(productId, 4, subtract: false);

                Assert.True(result);
                Assert.Equal(14, SqlTestHelper.ExecuteScalarInt("SELECT stock FROM product WHERE id = @id", new SqlParameter("@id", productId)));
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
            }
        }
    }
}
