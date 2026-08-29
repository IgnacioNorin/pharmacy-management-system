using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using Xunit;

namespace PharmacySystem.Tests.Presentation
{
    // PharmacyTicketBuilder.Build calls CultureInfoHelper.FormatAsCurrency, which reads the same
    // process-wide static field CultureInfoHelperTests mutates via SetCurrency - shares the
    // "Database" collection for the same reason as ReportPresenterTests.
    [Collection("Database")]
    public class PharmacyTicketBuilderTests
    {
        private static Store MakeStore() => new Store
        {
            companyName = "Farmacia Central",
            document = "0102030405",
            address = "Av. Siempre Viva",
            phone = "0999999999",
            email = "contacto@farmacia.com",
            defaultTaxRate = 19m
        };

        private static Sale MakeSale() => new Sale
        {
            typeDocument = "Factura",
            numberDocument = "001-001-000001",
            registrationDate = new DateTime(2026, 3, 17, 14, 30, 0),
            netAmount = 840m,
            taxAmount = 160m,
            exemptAmount = 0m,
            totalPay = 1000m,
            payWith = 2000m,
            change = 1000m,
            paymentMethod = "Tarjeta"
        };

        private static List<SaleDetail> MakeDetails() => new List<SaleDetail>
        {
            new SaleDetail { oProduct = new Product { name = "Paracetamol" }, amount = 2, salePrice = 5m, subtotal = 10m }
        };

        [Fact]
        public void Build_NullSale_ReturnsErrorText()
        {
            string result = PharmacyTicketBuilder.Build(MakeStore(), null, MakeDetails());

            Assert.Equal("Error: Sale not found or no details available.", result);
        }

        [Fact]
        public void Build_EmptyDetails_ReturnsErrorText()
        {
            string result = PharmacyTicketBuilder.Build(MakeStore(), MakeSale(), new List<SaleDetail>());

            Assert.Equal("Error: Sale not found or no details available.", result);
        }

        [Fact]
        public void Build_ValidSale_IncludesStoreAndSaleInfo()
        {
            // registrationDate.ToString("dd/MM/yyyy") renders its separator via the thread's
            // current culture, same as the original code - pin Invariant so the assertion doesn't
            // depend on whatever culture the machine running the tests defaults to.
            var original = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            string ticket;
            try
            {
                ticket = PharmacyTicketBuilder.Build(MakeStore(), MakeSale(), MakeDetails());
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = original;
            }

            Assert.Contains("FARMACIA CENTRAL", ticket);
            Assert.Contains("Documento: 0102030405", ticket);
            Assert.Contains("Paracetamol", ticket);
            Assert.Contains("17/03/2026", ticket);
            Assert.Contains("NETO:", ticket);
            Assert.Contains("IVA (19%):", ticket);
            Assert.Contains("FORMA DE PAGO:", ticket);
            Assert.Contains("Tarjeta", ticket);
            Assert.Contains("Cliente: Público General", ticket); // no recipient block for a plain sale
        }

        [Fact]
        public void Build_Factura_IncludesRecipientBlock()
        {
            Sale sale = MakeSale();
            sale.typeDocument = "Factura";
            sale.recipientTaxId = "76.111.111-1";
            sale.recipientBusinessName = "Acme SpA";
            sale.recipientActivity = "Comercio";
            sale.recipientAddress = "Av. Principal 123";
            sale.recipientCommune = "Santiago";

            string ticket = PharmacyTicketBuilder.Build(MakeStore(), sale, MakeDetails());

            Assert.Contains("RECEPTOR", ticket);
            Assert.Contains("76.111.111-1", ticket);
            Assert.Contains("Acme SpA", ticket);
            Assert.Contains("Santiago", ticket);
            Assert.DoesNotContain("Público General", ticket);
        }

        [Fact]
        public void Build_ProductNameLongerThan20Chars_IsTruncatedWithEllipsis()
        {
            var details = new List<SaleDetail>
            {
                new SaleDetail { oProduct = new Product { name = "Nombre de producto extremadamente largo" }, amount = 1, salePrice = 1m, subtotal = 1m }
            };

            string ticket = PharmacyTicketBuilder.Build(MakeStore(), MakeSale(), details);

            Assert.Contains("Nombre de product...", ticket);
        }

        [Fact]
        public void Build_StoreWithoutPhoneOrEmail_OmitsThoseLines()
        {
            var store = MakeStore();
            store.phone = "";
            store.email = "";

            string ticket = PharmacyTicketBuilder.Build(store, MakeSale(), MakeDetails());

            Assert.DoesNotContain("Tel:", ticket);
        }
    }

    public class PlainTextTicketFormatterTests
    {
        [Fact]
        public void AddCharacter_RepeatsCharacterToFillWidth()
        {
            var formatter = new PlainTextTicketFormatter(width: 5);

            formatter.AddCharacter("-");

            Assert.Equal("-----" + Environment.NewLine, formatter.ToString());
        }

        [Fact]
        public void AddCenteredText_ShorterThanWidth_IsPadded()
        {
            var formatter = new PlainTextTicketFormatter(width: 10);

            formatter.AddCenteredText("Hi");

            Assert.Equal("    Hi" + Environment.NewLine, formatter.ToString());
        }

        [Fact]
        public void AddCenteredText_LongerThanWidth_IsTruncated()
        {
            var formatter = new PlainTextTicketFormatter(width: 5);

            formatter.AddCenteredText("Hello World");

            Assert.Equal("Hello" + Environment.NewLine, formatter.ToString());
        }

        [Fact]
        public void AddTwoColumns_FitsWidth_PadsBetween()
        {
            var formatter = new PlainTextTicketFormatter(width: 10);

            formatter.AddTwoColumns("AB", "CD");

            Assert.Equal("AB      CD" + Environment.NewLine, formatter.ToString());
        }

        [Fact]
        public void AddTwoColumns_TooLongButLeftCanBeTruncated_TruncatesLeft()
        {
            var formatter = new PlainTextTicketFormatter(width: 6);

            formatter.AddTwoColumns("ABCDEF", "XY");

            Assert.Equal("ABC XY" + Environment.NewLine, formatter.ToString());
        }

        [Fact]
        public void AddTwoColumns_RightAloneExceedsWidth_PutsEachOnOwnLine()
        {
            var formatter = new PlainTextTicketFormatter(width: 3);

            formatter.AddTwoColumns("AB", "WXYZ");

            Assert.Equal("AB" + Environment.NewLine + "WXY" + Environment.NewLine, formatter.ToString());
        }
    }
}
