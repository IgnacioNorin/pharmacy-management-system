using System.Collections.Generic;
using System.Data.SqlClient;
using PharmacySystem.Helpers;
using PharmacySystem.Data;
using PharmacySystem.Fiscal;
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

        private static int CreateProductWithStock(int categoryId, int stock)
        {
            int productId = ProductRepo.Register(new Product
            {
                code = SqlTestHelper.NewTag(),
                name = "Sale product",
                description = "Sale product",
                oCategory = new Categories { IdCategory = categoryId }
            });
            SqlTestHelper.ExecuteNonQuery("UPDATE product SET stock = @stock WHERE id = @id",
                new SqlParameter("@stock", stock), new SqlParameter("@id", productId));
            return productId;
        }

        [Fact]
        public void RegisterSale_ValidDetail_InsertsHeaderAndDetailAndDiscountsStock()
        {
            Person person = CreatePerson(out string document);
            int categoryId = CategoryRepo.Register(new Categories { description = SqlTestHelper.NewTag() });
            int productId = CreateProductWithStock(categoryId, 10);

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
                Assert.Equal(7, SqlTestHelper.ExecuteScalarInt("SELECT stock FROM product WHERE id = @id", new SqlParameter("@id", productId)));
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
        public void RegisterSale_BoletaAndFactura_NumberedByIndependentSequences()
        {
            Person person = CreatePerson(out string document);
            int categoryId = CategoryRepo.Register(new Categories { description = SqlTestHelper.NewTag() });
            int productId = CreateProductWithStock(categoryId, 100);

            var saleIds = new List<int>();
            try
            {
                Sale Make(string type) => new Sale
                {
                    typeDocument = type,
                    oPerson = person,
                    documentClient = "9999999999",
                    nameClient = "Walk-in client",
                    totalPay = 5m, payWith = 5m, change = 0m,
                    oSaleDetail = new List<SaleDetail>
                    {
                        new SaleDetail { oProduct = new Product { idProduct = productId }, amount = 1, salePrice = 5m, subtotal = 5m }
                    }
                };

                int boleta1 = Repository.Register(Make("Boleta"));
                int factura1 = Repository.Register(Make("Factura"));
                int boleta2 = Repository.Register(Make("Boleta"));
                saleIds.AddRange(new[] { boleta1, factura1, boleta2 });

                string Number(int id) => (string)SqlTestHelper.ExecuteScalar(
                    "SELECT document_number FROM sale WHERE id = @id", new SqlParameter("@id", id));

                // The two boletas are consecutive; the factura in between does not consume a boleta number.
                Assert.Equal(int.Parse(Number(boleta1)) + 1, int.Parse(Number(boleta2)));
                // A factura and a boleta can share a number - the unique index is per type.
                Assert.NotEqual(0, factura1);
            }
            finally
            {
                foreach (int id in saleIds)
                {
                    SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_detail WHERE sale_id = @id", new SqlParameter("@id", id));
                    SqlTestHelper.ExecuteNonQuery("DELETE FROM sale WHERE id = @id", new SqlParameter("@id", id));
                }
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM person WHERE document_number = @document", new SqlParameter("@document", document));
            }
        }

        [Fact]
        public void CreateCreditNote_RestoresStockInsertsNegativeHeaderAndBlocksASecondOne()
        {
            Person person = CreatePerson(out string document);
            int categoryId = CategoryRepo.Register(new Categories { description = SqlTestHelper.NewTag() });
            int productId = CreateProductWithStock(categoryId, 10);

            int originalId = 0, ncId = 0;
            try
            {
                originalId = Repository.Register(new Sale
                {
                    typeDocument = "Boleta", oPerson = person,
                    documentClient = "9999999999", nameClient = "Walk-in",
                    totalPay = 1190m, payWith = 1190m, change = 0m,
                    netAmount = 1000m, taxAmount = 190m, exemptAmount = 0m,
                    oSaleDetail = new List<SaleDetail>
                    {
                        new SaleDetail { oProduct = new Product { idProduct = productId }, amount = 3, salePrice = 5m, subtotal = 15m }
                    }
                });
                Assert.True(originalId > 0);
                Assert.Equal(7, SqlTestHelper.ExecuteScalarInt("SELECT stock FROM product WHERE id = @id", new SqlParameter("@id", productId)));

                CreditNoteResult result = Repository.CreateCreditNote(originalId, person.idPerson, "Devolución del cliente");
                Assert.Equal(CreditNoteResult.Ok, result);

                // stock restored
                Assert.Equal(10, SqlTestHelper.ExecuteScalarInt("SELECT stock FROM product WHERE id = @id", new SqlParameter("@id", productId)));

                // a negative-amount NC row referencing the original
                ncId = SqlTestHelper.ExecuteScalarInt("SELECT id FROM sale WHERE reference_id = @id", new SqlParameter("@id", originalId));
                Assert.Equal("Nota de Credito", (string)SqlTestHelper.ExecuteScalar("SELECT document_type FROM sale WHERE id = @id", new SqlParameter("@id", ncId)));
                Assert.Equal(-1190, SqlTestHelper.ExecuteScalarInt("SELECT total_amount FROM sale WHERE id = @id", new SqlParameter("@id", ncId)));
                Assert.Equal(-1000, SqlTestHelper.ExecuteScalarInt("SELECT net_amount FROM sale WHERE id = @id", new SqlParameter("@id", ncId)));

                // second attempt is rejected
                Assert.Equal(CreditNoteResult.AlreadyCreditNoted, Repository.CreateCreditNote(originalId, person.idPerson, "otra vez"));

                // and a NC cannot itself be credit-noted
                Assert.Equal(CreditNoteResult.NotAllowedOnCreditNote, Repository.CreateCreditNote(ncId, person.idPerson, "no"));
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_detail WHERE sale_id IN (@a, @b)", new SqlParameter("@a", originalId), new SqlParameter("@b", ncId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale WHERE id = @id", new SqlParameter("@id", ncId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale WHERE id = @id", new SqlParameter("@id", originalId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM person WHERE document_number = @document", new SqlParameter("@document", document));
            }
        }

        [Fact]
        public void CreateCreditNote_UnknownSale_ReturnsNotFound()
        {
            Assert.Equal(CreditNoteResult.NotFound, Repository.CreateCreditNote(-999, 1, "x"));
        }

        [Fact]
        public void FindByDocument_ReturnsTheSaleWithItsFlags()
        {
            Person person = CreatePerson(out string document);
            int categoryId = CategoryRepo.Register(new Categories { description = SqlTestHelper.NewTag() });
            int productId = CreateProductWithStock(categoryId, 10);

            int originalId = 0, ncId = 0;
            try
            {
                originalId = Repository.Register(new Sale
                {
                    typeDocument = "Boleta", oPerson = person,
                    documentClient = "1", nameClient = "N",
                    totalPay = 5m, payWith = 5m, change = 0m,
                    oSaleDetail = new List<SaleDetail>
                    {
                        new SaleDetail { oProduct = new Product { idProduct = productId }, amount = 1, salePrice = 5m, subtotal = 5m }
                    }
                });
                string number = (string)SqlTestHelper.ExecuteScalar("SELECT document_number FROM sale WHERE id = @id", new SqlParameter("@id", originalId));

                SaleLookup before = Repository.FindByDocument("Boleta", number);
                Assert.NotNull(before);
                Assert.Equal(originalId, before.Id);
                Assert.False(before.IsCreditNote);
                Assert.False(before.AlreadyCreditNoted);

                Repository.CreateCreditNote(originalId, person.idPerson, "test");
                ncId = SqlTestHelper.ExecuteScalarInt("SELECT id FROM sale WHERE reference_id = @id", new SqlParameter("@id", originalId));

                Assert.True(Repository.FindByDocument("Boleta", number).AlreadyCreditNoted);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_detail WHERE sale_id IN (@a, @b)", new SqlParameter("@a", originalId), new SqlParameter("@b", ncId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale WHERE id = @id", new SqlParameter("@id", ncId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale WHERE id = @id", new SqlParameter("@id", originalId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM person WHERE document_number = @document", new SqlParameter("@document", document));
            }
        }

        [Fact]
        public void RegisterSale_Factura_PersistsRecipientFiscalData()
        {
            Person person = CreatePerson(out string document);
            int categoryId = CategoryRepo.Register(new Categories { description = SqlTestHelper.NewTag() });
            int productId = CreateProductWithStock(categoryId, 10);

            int saleId = 0;
            try
            {
                saleId = Repository.Register(new Sale
                {
                    typeDocument = "Factura",
                    oPerson = person,
                    documentClient = "76.111.111-1",
                    nameClient = "Acme SpA",
                    recipientTaxId = "76.111.111-1",
                    recipientBusinessName = "Acme SpA",
                    recipientActivity = "Comercio al por menor",
                    recipientAddress = "Av. Principal 123",
                    recipientCommune = "Santiago",
                    totalPay = 5m, payWith = 5m, change = 0m,
                    oSaleDetail = new List<SaleDetail>
                    {
                        new SaleDetail { oProduct = new Product { idProduct = productId }, amount = 1, salePrice = 5m, subtotal = 5m }
                    }
                });

                Assert.True(saleId > 0);
                Assert.Equal("Acme SpA", (string)SqlTestHelper.ExecuteScalar("SELECT recipient_business_name FROM sale WHERE id = @id", new SqlParameter("@id", saleId)));
                Assert.Equal("Santiago", (string)SqlTestHelper.ExecuteScalar("SELECT recipient_commune FROM sale WHERE id = @id", new SqlParameter("@id", saleId)));
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
        public void RegisterSale_PersistsVatBreakdownAndPerLineTaxAffectedFlag()
        {
            Person person = CreatePerson(out string document);
            int categoryId = CategoryRepo.Register(new Categories { description = SqlTestHelper.NewTag() });
            int productId = CreateProductWithStock(categoryId, 10);

            int saleId = 0;
            try
            {
                Sale sale = new Sale
                {
                    typeDocument = "Boleta",
                    oPerson = person,
                    documentClient = "9999999999",
                    nameClient = "Walk-in client",
                    totalPay = 1190m,
                    payWith = 1190m,
                    change = 0m,
                    netAmount = 1000m,
                    taxAmount = 190m,
                    exemptAmount = 0m,
                    oSaleDetail = new List<SaleDetail>
                    {
                        new SaleDetail
                        {
                            oProduct = new Product { idProduct = productId },
                            amount = 1,
                            salePrice = 1190m,
                            subtotal = 1190m,
                            taxAffected = false
                        }
                    }
                };

                saleId = Repository.Register(sale);
                Assert.True(saleId > 0);

                Assert.Equal(1000, SqlTestHelper.ExecuteScalarInt("SELECT net_amount FROM sale WHERE id = @id", new SqlParameter("@id", saleId)));
                Assert.Equal(190, SqlTestHelper.ExecuteScalarInt("SELECT tax_amount FROM sale WHERE id = @id", new SqlParameter("@id", saleId)));
                Assert.Equal(0, SqlTestHelper.ExecuteScalarInt("SELECT CAST(tax_affected AS INT) FROM sale_detail WHERE sale_id = @id", new SqlParameter("@id", saleId)));
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
        public void RegisterSale_InsufficientStockOnOneLine_RollsBackWholeSale()
        {
            Person person = CreatePerson(out string document);
            int categoryId = CategoryRepo.Register(new Categories { description = SqlTestHelper.NewTag() });
            int okProductId = CreateProductWithStock(categoryId, 10);
            int shortProductId = CreateProductWithStock(categoryId, 2);

            try
            {
                Sale sale = new Sale
                {
                    typeDocument = "Boleta",
                    oPerson = person,
                    documentClient = "9999999999",
                    nameClient = "Walk-in client",
                    totalPay = 30m,
                    payWith = 30m,
                    change = 0m,
                    oSaleDetail = new List<SaleDetail>
                    {
                        new SaleDetail { oProduct = new Product { idProduct = okProductId }, amount = 1, salePrice = 5m, subtotal = 5m },
                        new SaleDetail { oProduct = new Product { idProduct = shortProductId }, amount = 5, salePrice = 5m, subtotal = 25m }
                    }
                };

                int saleId = Repository.Register(sale);

                Assert.Equal(0, saleId);
                // Nothing persisted, and the first line's stock was not left decremented.
                Assert.Equal(0, SqlTestHelper.ExecuteScalarInt(
                    "SELECT COUNT(*) FROM sale WHERE document_client = '9999999999' AND name_client = 'Walk-in client' AND total_amount = 30"));
                Assert.Equal(10, SqlTestHelper.ExecuteScalarInt("SELECT stock FROM product WHERE id = @id", new SqlParameter("@id", okProductId)));
                Assert.Equal(2, SqlTestHelper.ExecuteScalarInt("SELECT stock FROM product WHERE id = @id", new SqlParameter("@id", shortProductId)));
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id IN (@a, @b)",
                    new SqlParameter("@a", okProductId), new SqlParameter("@b", shortProductId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM person WHERE document_number = @document", new SqlParameter("@document", document));
            }
        }

        [Fact]
        public void RegisterSale_AssignsSequentialReceiptNumbers()
        {
            Person person = CreatePerson(out string document);
            int categoryId = CategoryRepo.Register(new Categories { description = SqlTestHelper.NewTag() });
            int productId = CreateProductWithStock(categoryId, 100);

            var saleIds = new List<int>();
            try
            {
                for (int i = 0; i < 2; i++)
                {
                    saleIds.Add(Repository.Register(new Sale
                    {
                        typeDocument = "Boleta",
                        oPerson = person,
                        documentClient = "9999999999",
                        nameClient = "Walk-in client",
                        totalPay = 5m,
                        payWith = 5m,
                        change = 0m,
                        oSaleDetail = new List<SaleDetail>
                        {
                            new SaleDetail { oProduct = new Product { idProduct = productId }, amount = 1, salePrice = 5m, subtotal = 5m }
                        }
                    }));
                }

                var numbers = new List<string>();
                foreach (int id in saleIds)
                {
                    numbers.Add((string)SqlTestHelper.ExecuteScalar("SELECT document_number FROM sale WHERE id = @id", new SqlParameter("@id", id)));
                }

                Assert.All(numbers, n => Assert.False(string.IsNullOrWhiteSpace(n)));
                Assert.Equal(numbers.Count, new HashSet<string>(numbers).Count);
            }
            finally
            {
                foreach (int id in saleIds)
                {
                    SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_detail WHERE sale_id = @id", new SqlParameter("@id", id));
                    SqlTestHelper.ExecuteNonQuery("DELETE FROM sale WHERE id = @id", new SqlParameter("@id", id));
                }
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM person WHERE document_number = @document", new SqlParameter("@document", document));
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
            int productId = CreateProductWithStock(categoryId, 500);

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

        [Fact]
        public void RegisterSale_DefaultsFiscalStatusToInterno()
        {
            Person person = CreatePerson(out string document);
            int categoryId = CategoryRepo.Register(new Categories { description = SqlTestHelper.NewTag() });
            int productId = CreateProductWithStock(categoryId, 10);

            int saleId = 0;
            try
            {
                saleId = Repository.Register(new Sale
                {
                    typeDocument = "Boleta",
                    oPerson = person,
                    documentClient = "9999999999",
                    nameClient = "Walk-in client",
                    totalPay = 5m, payWith = 5m, change = 0m,
                    oSaleDetail = new List<SaleDetail>
                    {
                        new SaleDetail { oProduct = new Product { idProduct = productId }, amount = 1, salePrice = 5m, subtotal = 5m }
                    }
                });

                Assert.True(saleId > 0);
                Assert.Equal("interno", (string)SqlTestHelper.ExecuteScalar("SELECT fiscal_status FROM sale WHERE id = @id", new SqlParameter("@id", saleId)));
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
        public void SaveFiscalResult_StoresStatusTrackAndBarcode_AndKeepsFolioWhenNumberIsNull()
        {
            Person person = CreatePerson(out string document);
            int categoryId = CategoryRepo.Register(new Categories { description = SqlTestHelper.NewTag() });
            int productId = CreateProductWithStock(categoryId, 10);

            int saleId = 0;
            try
            {
                saleId = Repository.Register(new Sale
                {
                    typeDocument = "Boleta",
                    oPerson = person,
                    documentClient = "9999999999",
                    nameClient = "Walk-in client",
                    totalPay = 5m, payWith = 5m, change = 0m,
                    oSaleDetail = new List<SaleDetail>
                    {
                        new SaleDetail { oProduct = new Product { idProduct = productId }, amount = 1, salePrice = 5m, subtotal = 5m }
                    }
                });
                Assert.True(saleId > 0);

                string originalFolio = (string)SqlTestHelper.ExecuteScalar("SELECT document_number FROM sale WHERE id = @id", new SqlParameter("@id", saleId));

                Repository.SaveFiscalResult(saleId, new FiscalDocumentResult
                {
                    Status = FiscalStatuses.Aceptado,
                    TrackId = "TRK-123",
                    Barcode = "<TED>stamp</TED>"
                });

                Assert.Equal("aceptado", (string)SqlTestHelper.ExecuteScalar("SELECT fiscal_status FROM sale WHERE id = @id", new SqlParameter("@id", saleId)));
                Assert.Equal("TRK-123", (string)SqlTestHelper.ExecuteScalar("SELECT fiscal_track_id FROM sale WHERE id = @id", new SqlParameter("@id", saleId)));
                Assert.Equal("<TED>stamp</TED>", (string)SqlTestHelper.ExecuteScalar("SELECT fiscal_barcode FROM sale WHERE id = @id", new SqlParameter("@id", saleId)));
                Assert.Equal(originalFolio, (string)SqlTestHelper.ExecuteScalar("SELECT document_number FROM sale WHERE id = @id", new SqlParameter("@id", saleId)));
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
        public void SaveFiscalResult_WithDocumentNumber_OverridesTheFolio()
        {
            Person person = CreatePerson(out string document);
            int categoryId = CategoryRepo.Register(new Categories { description = SqlTestHelper.NewTag() });
            int productId = CreateProductWithStock(categoryId, 10);

            int saleId = 0;
            try
            {
                saleId = Repository.Register(new Sale
                {
                    typeDocument = "Factura",
                    oPerson = person,
                    documentClient = "76.111.111-1",
                    nameClient = "Acme SpA",
                    totalPay = 5m, payWith = 5m, change = 0m,
                    oSaleDetail = new List<SaleDetail>
                    {
                        new SaleDetail { oProduct = new Product { idProduct = productId }, amount = 1, salePrice = 5m, subtotal = 5m }
                    }
                });
                Assert.True(saleId > 0);

                Repository.SaveFiscalResult(saleId, new FiscalDocumentResult
                {
                    DocumentNumber = "990001",
                    Status = FiscalStatuses.Aceptado
                });

                Assert.Equal("990001", (string)SqlTestHelper.ExecuteScalar("SELECT document_number FROM sale WHERE id = @id", new SqlParameter("@id", saleId)));
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
        public void RegisterSale_WithClientId_PersistsTheLinkAndListSaleReadsItBack()
        {
            Person client = CreatePerson(out string document);
            int categoryId = CategoryRepo.Register(new Categories { description = SqlTestHelper.NewTag() });
            int productId = CreateProductWithStock(categoryId, 10);

            int saleId = 0;
            try
            {
                saleId = Repository.Register(new Sale
                {
                    typeDocument = "Boleta",
                    oPerson = client,
                    clientId = client.idPerson,
                    documentClient = client.document,
                    nameClient = client.name,
                    totalPay = 5m, payWith = 5m, change = 0m,
                    oSaleDetail = new List<SaleDetail>
                    {
                        new SaleDetail { oProduct = new Product { idProduct = productId }, amount = 1, salePrice = 5m, subtotal = 5m }
                    }
                });
                Assert.True(saleId > 0);

                Assert.Equal(client.idPerson,
                    SqlTestHelper.ExecuteScalarInt("SELECT client_id FROM sale WHERE id = @id", new SqlParameter("@id", saleId)));

                Sale listed = Repository.ListSale().Find(s => s.idSale == saleId);
                Assert.NotNull(listed);
                Assert.Equal(client.idPerson, listed.clientId);
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

        // Migration 014: a client that appears on a sale via client_id must be soft-deleted
        // (status = 0), not fail with an FK error the way it did before the SP knew about the link.
        [Fact]
        public void DeleteClient_ReferencedByASaleClientId_SoftDeletesInsteadOfFailing()
        {
            Person client = CreatePerson(out string document);
            int categoryId = CategoryRepo.Register(new Categories { description = SqlTestHelper.NewTag() });
            int productId = CreateProductWithStock(categoryId, 10);

            int saleId = 0;
            try
            {
                saleId = Repository.Register(new Sale
                {
                    typeDocument = "Boleta",
                    oPerson = client,
                    clientId = client.idPerson,
                    documentClient = client.document,
                    nameClient = client.name,
                    totalPay = 5m, payWith = 5m, change = 0m,
                    oSaleDetail = new List<SaleDetail>
                    {
                        new SaleDetail { oProduct = new Product { idProduct = productId }, amount = 1, salePrice = 5m, subtotal = 5m }
                    }
                });
                Assert.True(saleId > 0);

                bool deleted = PersonRepo.Delete(client.idPerson);

                Assert.True(deleted);
                Assert.Equal(0, SqlTestHelper.ExecuteScalarInt(
                    "SELECT CAST(status AS INT) FROM person WHERE id = @id", new SqlParameter("@id", client.idPerson)));
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
        public void ReportSale_WithAClientId_ReturnsOnlyThatClientSales()
        {
            Person client = CreatePerson(out string document);
            int categoryId = CategoryRepo.Register(new Categories { description = SqlTestHelper.NewTag() });
            int productId = CreateProductWithStock(categoryId, 20);

            var saleIds = new System.Collections.Generic.List<int>();
            try
            {
                Sale Make(int? cid, string number) => new Sale
                {
                    typeDocument = "Boleta",
                    oPerson = client,
                    clientId = cid,
                    documentClient = "x", nameClient = "x",
                    totalPay = 5m, payWith = 5m, change = 0m,
                    oSaleDetail = new System.Collections.Generic.List<SaleDetail>
                    {
                        new SaleDetail { oProduct = new Product { idProduct = productId }, amount = 1, salePrice = 5m, subtotal = 5m }
                    }
                };

                int linked = Repository.Register(Make(client.idPerson, "linked"));
                int walkIn = Repository.Register(Make(null, "walkin"));
                saleIds.Add(linked);
                saleIds.Add(walkIn);

                var start = System.DateTime.Today.AddDays(-1);
                var end = System.DateTime.Today.AddDays(1);

                var all = Repository.ReportSale(start, end, 0);
                var forClient = Repository.ReportSale(start, end, client.idPerson);

                Assert.True(all.Count >= 2);
                Assert.Single(forClient);
            }
            finally
            {
                foreach (int id in saleIds)
                {
                    SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_detail WHERE sale_id = @id", new SqlParameter("@id", id));
                    SqlTestHelper.ExecuteNonQuery("DELETE FROM sale WHERE id = @id", new SqlParameter("@id", id));
                }
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM person WHERE document_number = @document", new SqlParameter("@document", document));
            }
        }
    }
}
