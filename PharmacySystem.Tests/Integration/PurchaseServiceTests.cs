using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using PharmacySystem.Logical;
using PharmacySystem.Model;
using Xunit;

namespace PharmacySystem.Tests.Integration
{
    [Collection("Database")]
    public class PurchaseServiceTests
    {
        private static int PersonTypeId()
        {
            return SqlTestHelper.ExecuteScalarInt("SELECT TOP 1 id FROM person_type");
        }

        [Fact]
        public void RegisterPurchase_ValidDetail_InsertsRowsAndUpdatesProductStock()
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

                bool result = PurchaseService.Instance.RegisterPurchase(purchase);

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
                Assert.True(PurchaseService.Instance.RegisterPurchase(purchase));
                purchaseId = SqlTestHelper.ExecuteScalarInt("SELECT id FROM purchase WHERE document_number = @doc", new SqlParameter("@doc", documentNumber));

                string today = DateTime.Today.ToString("yyyy-MM-dd");
                decimal totalAmount = PurchaseService.Instance.GetTotalAmount(supplierId.ToString(), today, today);

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
