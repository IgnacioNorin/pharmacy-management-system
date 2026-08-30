using System;
using System.Collections.Generic;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using Xunit;

namespace PharmacySystem.Tests.Presentation
{
    public class HtmlTicketBuilderTests
    {
        // Minimal stand-in for Resources/Ticket.html: only the tokens the builder fills.
        private const string Template =
            "N=¡nombreempresa! D=¡documentoempresa! T=¡tipodocumento! NRO=¡numerodocumento! F=¡fechaventa! " +
            "{{receptor}} DET[¡detalleventa!] {{desgloseiva}} TOT=¡totalpagar! FP=¡formapago! PC=¡pagocon! CB=¡cambio!";

        private static Store MakeStore() => new Store
        {
            companyName = "Farmacia Central",
            document = "76.111.222-3",
            email = "a@b.cl",
            phone = "123",
            defaultTaxRate = 19m
        };

        private static Sale MakeSale() => new Sale
        {
            typeDocument = "Boleta",
            numberDocument = "000123",
            registrationDate = new DateTime(2026, 3, 17, 14, 30, 0),
            netAmount = 840m,
            taxAmount = 160m,
            exemptAmount = 0m,
            totalPay = 1000m,
            payWith = 2000m,
            change = 1000m,
            paymentMethod = "Efectivo"
        };

        private static List<SaleDetail> OneLine(string productName) => new List<SaleDetail>
        {
            new SaleDetail { amount = 2, salePrice = 500m, subtotal = 1000m, oProduct = new Product { name = productName } }
        };

        [Fact]
        public void Build_HtmlEncodesProductAndCompanyNames()
        {
            var store = MakeStore();
            store.companyName = "Botica <A&B>";

            string html = HtmlTicketBuilder.Build(Template, store, MakeSale(), OneLine("Vitamina C & D <forte>"));

            Assert.Contains("Vitamina C &amp; D &lt;forte&gt;", html);
            Assert.Contains("BOTICA &lt;A&amp;B&gt;", html); // company name is upper-cased, then encoded
            Assert.DoesNotContain("<forte>", html);
        }

        [Fact]
        public void Build_WithoutRecipient_ShowsPublicoGeneralAndNoRecipientBlock()
        {
            string html = HtmlTicketBuilder.Build(Template, MakeStore(), MakeSale(), OneLine("Aspirina"));

            Assert.Contains("Público General", html);
            Assert.DoesNotContain("RECEPTOR", html);
        }

        [Fact]
        public void Build_WithRecipient_RendersTheReceptorBlock()
        {
            Sale sale = MakeSale();
            sale.typeDocument = "Factura";
            sale.recipientTaxId = "77.888.999-0";
            sale.recipientBusinessName = "Clínica <Andes>";
            sale.recipientActivity = "Salud";
            sale.recipientAddress = "Calle 1";
            sale.recipientCommune = "Santiago";

            string html = HtmlTicketBuilder.Build(Template, MakeStore(), sale, OneLine("Aspirina"));

            Assert.Contains("RECEPTOR", html);
            Assert.Contains("77.888.999-0", html);
            Assert.Contains("Cl&#237;nica &lt;Andes&gt;", html);
            Assert.Contains("Salud", html);
            Assert.DoesNotContain("Público General", html);
        }

        [Fact]
        public void Build_RendersTheNetIvaExemptBreakdown()
        {
            Sale sale = MakeSale();
            sale.exemptAmount = 50m;

            string html = HtmlTicketBuilder.Build(Template, MakeStore(), sale, OneLine("Aspirina"));

            Assert.Contains("Neto:", html);
            Assert.Contains("IVA (19%):", html);
            Assert.Contains("Exento:", html);
        }

        [Fact]
        public void Build_NoExemptAmount_OmitsTheExemptRow()
        {
            string html = HtmlTicketBuilder.Build(Template, MakeStore(), MakeSale(), OneLine("Aspirina"));

            Assert.Contains("Neto:", html);
            Assert.DoesNotContain("Exento:", html);
        }

        [Fact]
        public void Build_CreditNote_ShowsTheVoidedSaleReference()
        {
            Sale sale = MakeSale();
            sale.referenceId = 42;
            sale.referenceReason = "Devolución <cliente>";

            string html = HtmlTicketBuilder.Build(Template, MakeStore(), sale, OneLine("Aspirina"));

            Assert.Contains("Anula venta N", html);
            Assert.Contains("42", html);
            Assert.Contains("Devoluci&#243;n &lt;cliente&gt;", html);
        }

        [Fact]
        public void Build_FillsThePlainTokens()
        {
            string html = HtmlTicketBuilder.Build(Template, MakeStore(), MakeSale(), OneLine("Aspirina"));

            Assert.DoesNotContain("¡", html);       // every legacy token replaced
            Assert.DoesNotContain("{{", html);      // every block token replaced
            Assert.Contains("Boleta", html);
            Assert.Contains("Efectivo", html);
        }
    }
}
