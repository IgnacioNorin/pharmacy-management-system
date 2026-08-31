using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
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
        private static readonly IClientRepository ClientRepo = new ClientRepository(SqlConnectionFactory.FromConfiguration());
        private static readonly ICategoryRepository CategoryRepo = new CategoryRepository(SqlConnectionFactory.FromConfiguration());
        private static readonly IProductRepository ProductRepo = new ProductRepository(SqlConnectionFactory.FromConfiguration());

        private static int PersonTypeId()
        {
            return SqlTestHelper.ExecuteScalarInt("SELECT TOP 1 id FROM person_type");
        }

        // Credits every unit of every line still creditable on a sale - the pre-partial-credit
        // "anular la venta completa" behaviour, expressed through the new per-line API.
        private static CreditNoteResult CreditWholeSale(int saleId, int userId, string reason)
        {
            List<CreditNoteLineRequest> lines = Repository.GetCreditableLines(saleId)
                .Where(l => l.RemainingQuantity > 0)
                .Select(l => new CreditNoteLineRequest { SourceDetailId = l.SourceDetailId, Quantity = l.RemainingQuantity })
                .ToList();
            return Repository.CreateCreditNote(saleId, userId, reason, lines);
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

        private static Client CreateClient(out string document)
        {
            document = SqlTestHelper.NewTag();
            int id = ClientRepo.Register(new Client
            {
                document = document,
                name = "Sale client",
                address = "Address",
                phone = "0999999999"
            });
            return new Client { idClient = id, document = document, name = "Sale client" };
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
                    paymentMethod = "Transferencia",
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

                // Targeted single-sale lookups (used by the ticket printer instead of loading the
                // whole history - DEF-13).
                Sale one = Repository.GetById(saleId);
                Assert.NotNull(one);
                Assert.Equal("Walk-in client", one.nameClient);
                Assert.Equal("Transferencia", one.paymentMethod);
                Assert.Null(Repository.GetById(-1));

                var lines = Repository.GetDetailsBySaleId(saleId);
                Assert.Single(lines);
                Assert.Equal(15m, lines[0].subtotal);
                Assert.Empty(Repository.GetDetailsBySaleId(-1));
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_payment WHERE sale_id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_detail WHERE sale_id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale WHERE id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product_lot WHERE product_id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM person WHERE document_number = @document", new SqlParameter("@document", document));
            }
        }

        [Fact]
        public void RegisterSale_ConsumesLotsEarliestExpiryFirst_AndCreditNoteReturnsAnUndatedLot()
        {
            Person person = CreatePerson(out string document);
            int categoryId = CategoryRepo.Register(new Categories { description = SqlTestHelper.NewTag() });
            int productId = CreateProductWithStock(categoryId, 10);

            // Two lots: 4 units expiring soon, 6 units expiring later.
            SqlTestHelper.ExecuteNonQuery(
                "INSERT INTO product_lot(product_id, quantity, date_expired, unit_cost) VALUES (@p, 4, @soon, 2), (@p, 6, @later, 2)",
                new SqlParameter("@p", productId),
                new SqlParameter("@soon", DateTime.Today.AddDays(10)),
                new SqlParameter("@later", DateTime.Today.AddDays(200)));

            int saleId = 0;
            try
            {
                Sale sale = new Sale
                {
                    typeDocument = "Boleta",
                    oPerson = person,
                    documentClient = "9999999999",
                    nameClient = "Walk-in client",
                    totalPay = 25m,
                    payWith = 25m,
                    oSaleDetail = new List<SaleDetail>
                    {
                        new SaleDetail { oProduct = new Product { idProduct = productId }, amount = 5, salePrice = 5m, subtotal = 25m }
                    }
                };

                saleId = Repository.Register(sale);
                Assert.True(saleId > 0);

                // FEFO: the 4-unit soon lot is emptied, then 1 unit taken from the later lot.
                var lots = ProductRepo.GetLots(productId).OrderBy(l => l.dateExpired).ToList();
                Assert.Single(lots);
                Assert.Equal(5, lots[0].quantity);
                Assert.Equal(DateTime.Today.AddDays(200), lots[0].dateExpired);

                // A full credit note puts the 5 units back as one new undated lot.
                Assert.Equal(CreditNoteResult.Ok, CreditWholeSale(saleId, person.idPerson, "prueba"));
                var afterNc = ProductRepo.GetLots(productId);
                Assert.Contains(afterNc, l => l.dateExpired == null && l.quantity == 5);
                Assert.Equal(10, SqlTestHelper.ExecuteScalarInt("SELECT stock FROM product WHERE id = @id", new SqlParameter("@id", productId)));
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_payment WHERE sale_id = @a OR sale_id IN (SELECT id FROM sale WHERE reference_id = @a)", new SqlParameter("@a", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_detail WHERE sale_id = @a OR sale_id IN (SELECT id FROM sale WHERE reference_id = @a)", new SqlParameter("@a", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale WHERE reference_id = @a", new SqlParameter("@a", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale WHERE id = @a", new SqlParameter("@a", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product_lot WHERE product_id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM person WHERE document_number = @document", new SqlParameter("@document", document));
            }
        }

        [Fact]
        public void RegisterSale_FreezesTheProductAverageCostOnTheDetailLine()
        {
            Person person = CreatePerson(out string document);
            int categoryId = CategoryRepo.Register(new Categories { description = SqlTestHelper.NewTag() });
            int productId = CreateProductWithStock(categoryId, 10);
            SqlTestHelper.ExecuteNonQuery("UPDATE product SET average_cost = 7.50 WHERE id = @id", new SqlParameter("@id", productId));

            int saleId = 0;
            try
            {
                saleId = Repository.Register(new Sale
                {
                    typeDocument = "Boleta",
                    oPerson = person,
                    documentClient = "9999999999",
                    nameClient = "Walk-in client",
                    totalPay = 20m, payWith = 20m, change = 0m,
                    oSaleDetail = new List<SaleDetail>
                    {
                        new SaleDetail { oProduct = new Product { idProduct = productId }, amount = 2, salePrice = 10m, subtotal = 20m }
                    }
                });
                Assert.True(saleId > 0);

                decimal unitCost = (decimal)SqlTestHelper.ExecuteScalar(
                    "SELECT unit_cost FROM sale_detail WHERE sale_id = @id", new SqlParameter("@id", saleId));
                Assert.Equal(7.50m, unitCost);

                // Changing the cost afterwards does not touch the frozen line.
                SqlTestHelper.ExecuteNonQuery("UPDATE product SET average_cost = 99 WHERE id = @id", new SqlParameter("@id", productId));
                unitCost = (decimal)SqlTestHelper.ExecuteScalar(
                    "SELECT unit_cost FROM sale_detail WHERE sale_id = @id", new SqlParameter("@id", saleId));
                Assert.Equal(7.50m, unitCost);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_payment WHERE sale_id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_detail WHERE sale_id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale WHERE id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product_lot WHERE product_id = @id", new SqlParameter("@id", productId));
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
                    SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_payment WHERE sale_id = @id", new SqlParameter("@id", id));
                    SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_detail WHERE sale_id = @id", new SqlParameter("@id", id));
                    SqlTestHelper.ExecuteNonQuery("DELETE FROM sale WHERE id = @id", new SqlParameter("@id", id));
                }
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product_lot WHERE product_id = @id", new SqlParameter("@id", productId));
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
                        new SaleDetail { oProduct = new Product { idProduct = productId }, amount = 1, salePrice = 1190m, subtotal = 1190m, taxAffected = true }
                    }
                });
                Assert.True(originalId > 0);
                Assert.Equal(9, SqlTestHelper.ExecuteScalarInt("SELECT stock FROM product WHERE id = @id", new SqlParameter("@id", productId)));

                CreditNoteResult result = CreditWholeSale(originalId, person.idPerson, "Devolución del cliente");
                Assert.Equal(CreditNoteResult.Ok, result);

                // stock restored
                Assert.Equal(10, SqlTestHelper.ExecuteScalarInt("SELECT stock FROM product WHERE id = @id", new SqlParameter("@id", productId)));

                // a negative-amount NC row referencing the original, with the VAT split recomputed
                ncId = SqlTestHelper.ExecuteScalarInt("SELECT id FROM sale WHERE reference_id = @id", new SqlParameter("@id", originalId));
                Assert.Equal("Nota de Credito", (string)SqlTestHelper.ExecuteScalar("SELECT document_type FROM sale WHERE id = @id", new SqlParameter("@id", ncId)));
                Assert.Equal(-1190, SqlTestHelper.ExecuteScalarInt("SELECT total_amount FROM sale WHERE id = @id", new SqlParameter("@id", ncId)));
                Assert.Equal(-1000, SqlTestHelper.ExecuteScalarInt("SELECT net_amount FROM sale WHERE id = @id", new SqlParameter("@id", ncId)));
                Assert.Equal(-190, SqlTestHelper.ExecuteScalarInt("SELECT tax_amount FROM sale WHERE id = @id", new SqlParameter("@id", ncId)));

                // the NC line links back to the original line it credited
                Assert.Equal(1, SqlTestHelper.ExecuteScalarInt(
                    "SELECT COUNT(*) FROM sale_detail nc JOIN sale_detail o ON o.id = nc.source_detail_id " +
                    "WHERE nc.sale_id = @nc AND o.sale_id = @o", new SqlParameter("@nc", ncId), new SqlParameter("@o", originalId)));

                // nothing left to credit -> a second whole-sale credit is a no-op
                Assert.Equal(CreditNoteResult.NothingToCredit, CreditWholeSale(originalId, person.idPerson, "otra vez"));

                // and a NC cannot itself be credit-noted
                Assert.Equal(CreditNoteResult.NotAllowedOnCreditNote,
                    Repository.CreateCreditNote(ncId, person.idPerson, "no",
                        new List<CreditNoteLineRequest> { new CreditNoteLineRequest { SourceDetailId = 1, Quantity = 1 } }));
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_payment WHERE sale_id IN (@a, @b)", new SqlParameter("@a", originalId), new SqlParameter("@b", ncId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_detail WHERE sale_id IN (@a, @b)", new SqlParameter("@a", originalId), new SqlParameter("@b", ncId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale WHERE id = @id", new SqlParameter("@id", ncId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale WHERE id = @id", new SqlParameter("@id", originalId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product_lot WHERE product_id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM person WHERE document_number = @document", new SqlParameter("@document", document));
            }
        }

        [Fact]
        public void CreateCreditNote_UnknownSale_ReturnsNotFound()
        {
            Assert.Equal(CreditNoteResult.NotFound, Repository.CreateCreditNote(-999, 1, "x",
                new List<CreditNoteLineRequest> { new CreditNoteLineRequest { SourceDetailId = 1, Quantity = 1 } }));
        }

        [Fact]
        public void CreateCreditNote_NoLinesRequested_ReturnsNothingToCredit()
        {
            Assert.Equal(CreditNoteResult.NothingToCredit,
                Repository.CreateCreditNote(-999, 1, "x", new List<CreditNoteLineRequest>()));
        }

        [Fact]
        public void CreateCreditNote_PartialThenTheRest_TracksRemainingAndSplitsVat()
        {
            Person person = CreatePerson(out string document);
            int categoryId = CategoryRepo.Register(new Categories { description = SqlTestHelper.NewTag() });
            int productId = CreateProductWithStock(categoryId, 20);

            int originalId = 0;
            try
            {
                originalId = Repository.Register(new Sale
                {
                    typeDocument = "Boleta", oPerson = person,
                    documentClient = "9999999999", nameClient = "Walk-in",
                    totalPay = 2380m, payWith = 2380m, change = 0m,
                    netAmount = 2000m, taxAmount = 380m, exemptAmount = 0m,
                    oSaleDetail = new List<SaleDetail>
                    {
                        new SaleDetail { oProduct = new Product { idProduct = productId }, amount = 5, salePrice = 476m, subtotal = 2380m, taxAffected = true }
                    }
                });
                Assert.Equal(15, SqlTestHelper.ExecuteScalarInt("SELECT stock FROM product WHERE id = @id", new SqlParameter("@id", productId)));

                int sourceDetailId = Repository.GetCreditableLines(originalId).Single().SourceDetailId;

                // Credit 2 of the 5 units.
                Assert.Equal(CreditNoteResult.Ok, Repository.CreateCreditNote(originalId, person.idPerson, "devuelve 2",
                    new List<CreditNoteLineRequest> { new CreditNoteLineRequest { SourceDetailId = sourceDetailId, Quantity = 2 } }));

                SaleCreditDetail afterFirst = Repository.GetCreditableLines(originalId).Single();
                Assert.Equal(5, afterFirst.SoldQuantity);
                Assert.Equal(2, afterFirst.CreditedQuantity);
                Assert.Equal(3, afterFirst.RemainingQuantity);
                Assert.Equal(17, SqlTestHelper.ExecuteScalarInt("SELECT stock FROM product WHERE id = @id", new SqlParameter("@id", productId)));

                int nc1 = SqlTestHelper.ExecuteScalarInt("SELECT TOP 1 id FROM sale WHERE reference_id = @id ORDER BY id", new SqlParameter("@id", originalId));
                // 2 * 476 = 952 gross; net = round(952 / 1.19) = 800; tax = 152.
                Assert.Equal(-952, SqlTestHelper.ExecuteScalarInt("SELECT total_amount FROM sale WHERE id = @id", new SqlParameter("@id", nc1)));
                Assert.Equal(-800, SqlTestHelper.ExecuteScalarInt("SELECT net_amount FROM sale WHERE id = @id", new SqlParameter("@id", nc1)));
                Assert.Equal(-152, SqlTestHelper.ExecuteScalarInt("SELECT tax_amount FROM sale WHERE id = @id", new SqlParameter("@id", nc1)));

                Assert.True(Repository.FindByDocument("Boleta",
                    (string)SqlTestHelper.ExecuteScalar("SELECT document_number FROM sale WHERE id = @id", new SqlParameter("@id", originalId))).AlreadyCreditNoted);
                Assert.False(Repository.FindByDocument("Boleta",
                    (string)SqlTestHelper.ExecuteScalar("SELECT document_number FROM sale WHERE id = @id", new SqlParameter("@id", originalId))).FullyCreditNoted);

                // Asking for 4 more when only 3 remain is rejected, atomically.
                Assert.Equal(CreditNoteResult.QuantityExceedsRemaining, Repository.CreateCreditNote(originalId, person.idPerson, "de mas",
                    new List<CreditNoteLineRequest> { new CreditNoteLineRequest { SourceDetailId = sourceDetailId, Quantity = 4 } }));
                Assert.Equal(3, Repository.GetCreditableLines(originalId).Single().RemainingQuantity);

                // Credit the remaining 3.
                Assert.Equal(CreditNoteResult.Ok, CreditWholeSale(originalId, person.idPerson, "el resto"));
                Assert.Equal(0, Repository.GetCreditableLines(originalId).Single().RemainingQuantity);
                Assert.Equal(20, SqlTestHelper.ExecuteScalarInt("SELECT stock FROM product WHERE id = @id", new SqlParameter("@id", productId)));
                Assert.True(Repository.FindByDocument("Boleta",
                    (string)SqlTestHelper.ExecuteScalar("SELECT document_number FROM sale WHERE id = @id", new SqlParameter("@id", originalId))).FullyCreditNoted);
            }
            finally
            {
                // The original plus every Nota de Credito that references it.
                SqlTestHelper.ExecuteNonQuery(
                    "DELETE FROM sale_payment WHERE sale_id = @o OR sale_id IN (SELECT id FROM sale WHERE reference_id = @o)", new SqlParameter("@o", originalId));
                SqlTestHelper.ExecuteNonQuery(
                    "DELETE FROM sale_detail WHERE sale_id = @o OR sale_id IN (SELECT id FROM sale WHERE reference_id = @o)", new SqlParameter("@o", originalId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale WHERE reference_id = @o", new SqlParameter("@o", originalId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale WHERE id = @o", new SqlParameter("@o", originalId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product_lot WHERE product_id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM person WHERE document_number = @document", new SqlParameter("@document", document));
            }
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
                Assert.False(before.FullyCreditNoted);

                CreditWholeSale(originalId, person.idPerson, "test");
                ncId = SqlTestHelper.ExecuteScalarInt("SELECT id FROM sale WHERE reference_id = @id", new SqlParameter("@id", originalId));

                SaleLookup after = Repository.FindByDocument("Boleta", number);
                Assert.True(after.AlreadyCreditNoted);
                Assert.True(after.FullyCreditNoted);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_payment WHERE sale_id IN (@a, @b)", new SqlParameter("@a", originalId), new SqlParameter("@b", ncId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_detail WHERE sale_id IN (@a, @b)", new SqlParameter("@a", originalId), new SqlParameter("@b", ncId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale WHERE id = @id", new SqlParameter("@id", ncId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale WHERE id = @id", new SqlParameter("@id", originalId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product_lot WHERE product_id = @id", new SqlParameter("@id", productId));
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
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_payment WHERE sale_id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_detail WHERE sale_id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale WHERE id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product_lot WHERE product_id = @id", new SqlParameter("@id", productId));
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
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_payment WHERE sale_id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_detail WHERE sale_id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale WHERE id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product_lot WHERE product_id = @id", new SqlParameter("@id", productId));
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
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product_lot WHERE product_id IN (@a, @b)",
                    new SqlParameter("@a", okProductId), new SqlParameter("@b", shortProductId));
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
                    SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_payment WHERE sale_id = @id", new SqlParameter("@id", id));
                    SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_detail WHERE sale_id = @id", new SqlParameter("@id", id));
                    SqlTestHelper.ExecuteNonQuery("DELETE FROM sale WHERE id = @id", new SqlParameter("@id", id));
                }
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product_lot WHERE product_id = @id", new SqlParameter("@id", productId));
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
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_payment WHERE sale_id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_detail WHERE sale_id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale WHERE id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product_lot WHERE product_id = @id", new SqlParameter("@id", productId));
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
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_payment WHERE sale_id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_detail WHERE sale_id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale WHERE id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product_lot WHERE product_id = @id", new SqlParameter("@id", productId));
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
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_payment WHERE sale_id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_detail WHERE sale_id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale WHERE id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product_lot WHERE product_id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM person WHERE document_number = @document", new SqlParameter("@document", document));
            }
        }

        // DEF-18: RIGHT('000000' + folio, 6) truncated the number past 999.999, colliding with
        // UX_sale_document_number. Registering a boleta with the sequence already past a million
        // must keep every digit.
        [Fact]
        public void RegisterSale_FolioPastAMillion_KeepsEveryDigit()
        {
            Person person = CreatePerson(out string document);
            int categoryId = CategoryRepo.Register(new Categories { description = SqlTestHelper.NewTag() });
            int productId = CreateProductWithStock(categoryId, 10);

            long originalNext = (long)SqlTestHelper.ExecuteScalar(
                "SELECT CAST(current_value AS bigint) + 1 FROM sys.sequences WHERE name = 'seq_folio_boleta'");
            SqlTestHelper.ExecuteNonQuery("ALTER SEQUENCE dbo.seq_folio_boleta RESTART WITH 1000000");

            int saleId = 0;
            try
            {
                saleId = Repository.Register(new Sale
                {
                    typeDocument = "Boleta",
                    oPerson = person,
                    documentClient = "9999999999",
                    nameClient = "Walk-in",
                    totalPay = 5m, payWith = 5m, change = 0m,
                    oSaleDetail = new List<SaleDetail>
                    {
                        new SaleDetail { oProduct = new Product { idProduct = productId }, amount = 1, salePrice = 5m, subtotal = 5m }
                    }
                });

                Assert.True(saleId > 0);
                Assert.Equal("1000000", (string)SqlTestHelper.ExecuteScalar(
                    "SELECT document_number FROM sale WHERE id = @id", new SqlParameter("@id", saleId)));
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("ALTER SEQUENCE dbo.seq_folio_boleta RESTART WITH " + originalNext);
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_payment WHERE sale_id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_detail WHERE sale_id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale WHERE id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product_lot WHERE product_id = @id", new SqlParameter("@id", productId));
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
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_payment WHERE sale_id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_detail WHERE sale_id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale WHERE id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product_lot WHERE product_id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM person WHERE document_number = @document", new SqlParameter("@document", document));
            }
        }

        [Fact]
        public void RegisterSale_WithClientId_PersistsTheLinkAndListSaleReadsItBack()
        {
            Person seller = CreatePerson(out string sellerDoc);
            Client client = CreateClient(out string clientDoc);
            int categoryId = CategoryRepo.Register(new Categories { description = SqlTestHelper.NewTag() });
            int productId = CreateProductWithStock(categoryId, 10);

            int saleId = 0;
            try
            {
                saleId = Repository.Register(new Sale
                {
                    typeDocument = "Boleta",
                    oPerson = seller,
                    clientId = client.idClient,
                    documentClient = client.document,
                    nameClient = client.name,
                    totalPay = 5m, payWith = 5m, change = 0m,
                    oSaleDetail = new List<SaleDetail>
                    {
                        new SaleDetail { oProduct = new Product { idProduct = productId }, amount = 1, salePrice = 5m, subtotal = 5m }
                    }
                });
                Assert.True(saleId > 0);

                Assert.Equal(client.idClient,
                    SqlTestHelper.ExecuteScalarInt("SELECT client_id FROM sale WHERE id = @id", new SqlParameter("@id", saleId)));

                Sale listed = Repository.ListSale().Find(s => s.idSale == saleId);
                Assert.NotNull(listed);
                Assert.Equal(client.idClient, listed.clientId);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_payment WHERE sale_id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_detail WHERE sale_id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale WHERE id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product_lot WHERE product_id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM client WHERE document_number = @d", new SqlParameter("@d", clientDoc));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM person WHERE document_number = @document", new SqlParameter("@document", sellerDoc));
            }
        }

        [Fact]
        public void ReportSale_Factura_MergesTheReceptorIntoTheClientColumns()
        {
            Person seller = CreatePerson(out string document);
            int categoryId = CategoryRepo.Register(new Categories { description = SqlTestHelper.NewTag() });
            int productId = CreateProductWithStock(categoryId, 10);

            int saleId = 0;
            try
            {
                saleId = Repository.Register(new Sale
                {
                    typeDocument = "Factura",
                    oPerson = seller,
                    documentClient = "",           // not duplicated on a Factura
                    nameClient = "",
                    recipientTaxId = "76.222.333-4",
                    recipientBusinessName = "Comercial Test SpA",
                    recipientActivity = "Comercio", recipientAddress = "Calle 1", recipientCommune = "Centro",
                    totalPay = 5m, payWith = 5m, change = 0m,
                    oSaleDetail = new System.Collections.Generic.List<SaleDetail>
                    {
                        new SaleDetail { oProduct = new Product { idProduct = productId }, amount = 1, salePrice = 5m, subtotal = 5m }
                    }
                });
                Assert.True(saleId > 0);

                SaleReportRow row = Repository
                    .ReportSale(System.DateTime.Today.AddDays(-1), System.DateTime.Today.AddDays(1), 0)
                    .Find(r => r.DocumentNumber != null && r.DocumentType == "Factura" && r.ClientDocument == "76.222.333-4");

                Assert.NotNull(row);
                Assert.Equal("Comercial Test SpA", row.ClientName);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_payment WHERE sale_id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_detail WHERE sale_id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale WHERE id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product_lot WHERE product_id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM person WHERE document_number = @document", new SqlParameter("@document", document));
            }
        }

        [Fact]
        public void ReportSale_WithAClientId_ReturnsOnlyThatClientSales()
        {
            Person seller = CreatePerson(out string sellerDoc);
            Client client = CreateClient(out string clientDoc);
            int categoryId = CategoryRepo.Register(new Categories { description = SqlTestHelper.NewTag() });
            int productId = CreateProductWithStock(categoryId, 20);

            var saleIds = new System.Collections.Generic.List<int>();
            try
            {
                Sale Make(int? cid) => new Sale
                {
                    typeDocument = "Boleta",
                    oPerson = seller,
                    clientId = cid,
                    documentClient = "x", nameClient = "x",
                    totalPay = 5m, payWith = 5m, change = 0m,
                    oSaleDetail = new System.Collections.Generic.List<SaleDetail>
                    {
                        new SaleDetail { oProduct = new Product { idProduct = productId }, amount = 1, salePrice = 5m, subtotal = 5m }
                    }
                };

                saleIds.Add(Repository.Register(Make(client.idClient)));
                saleIds.Add(Repository.Register(Make(null)));

                var start = System.DateTime.Today.AddDays(-1);
                var end = System.DateTime.Today.AddDays(1);

                var all = Repository.ReportSale(start, end, 0);
                var forClient = Repository.ReportSale(start, end, client.idClient);

                Assert.True(all.Count >= 2);
                Assert.Single(forClient);
            }
            finally
            {
                foreach (int id in saleIds)
                {
                    SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_payment WHERE sale_id = @id", new SqlParameter("@id", id));
                    SqlTestHelper.ExecuteNonQuery("DELETE FROM sale_detail WHERE sale_id = @id", new SqlParameter("@id", id));
                    SqlTestHelper.ExecuteNonQuery("DELETE FROM sale WHERE id = @id", new SqlParameter("@id", id));
                }
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product_lot WHERE product_id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM product WHERE id = @id", new SqlParameter("@id", productId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM category WHERE id = @id", new SqlParameter("@id", categoryId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM client WHERE document_number = @d", new SqlParameter("@d", clientDoc));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM person WHERE document_number = @document", new SqlParameter("@document", sellerDoc));
            }
        }
    }
}
