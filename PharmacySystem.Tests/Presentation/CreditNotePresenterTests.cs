using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using Xunit;

namespace PharmacySystem.Tests.Presentation
{
    public class CreditNotePresenterTests
    {
        private static CreditNotePresenter Create(FakeCreditNoteView view, FakeSaleService service, params string[] permissions)
            => new CreditNotePresenter(view, service, TestUser.With(permissions), currentPersonId: 7);

        private static SaleLookup Vigente(int id = 5) => new SaleLookup
        {
            Id = id, DocumentType = "Boleta", DocumentNumber = "000005",
            ClientName = "Cliente", TotalAmount = 1000m, IsCreditNote = false,
            AlreadyCreditNoted = false, FullyCreditNoted = false
        };

        private static List<CreditNoteLineRequest> OneUnit(int sourceDetailId = 1) =>
            new List<CreditNoteLineRequest> { new CreditNoteLineRequest { SourceDetailId = sourceDetailId, Quantity = 1 } };

        [Fact]
        public void OnLoad_LoadsDocumentTypesAndDisablesGenerate()
        {
            var view = new FakeCreditNoteView();
            Create(view, new FakeSaleService(), "ventas.nota_credito").OnLoad();

            Assert.Equal(new[] { "Boleta", "Factura" }, view.DocumentTypeOptions);
            Assert.False(view.GenerateEnabled);
        }

        [Fact]
        public void OnSearch_EmptyNumber_ShowsMessage()
        {
            var view = new FakeCreditNoteView { DocumentNumberInput = "  " };
            Create(view, new FakeSaleService(), "ventas.nota_credito").OnSearch();

            Assert.Contains(view.ShownMessages, m => m.Contains("número de comprobante"));
            Assert.False(view.GenerateEnabled);
        }

        [Fact]
        public void OnSearch_NotFound_ShowsMessageAndClears()
        {
            var view = new FakeCreditNoteView { DocumentNumberInput = "999" };
            var service = new FakeSaleService { FindByDocumentResult = null };
            Create(view, service, "ventas.nota_credito").OnSearch();

            Assert.Contains(view.ShownMessages, m => m.Contains("No se encontró"));
            Assert.Equal(1, view.ClearSaleCount);
        }

        [Fact]
        public void OnSearch_VigenteSale_ShowsItWithItsLinesAndEnablesGenerate()
        {
            var view = new FakeCreditNoteView { DocumentNumberInput = "000005", DocumentTypeInput = "Boleta" };
            var lines = new List<SaleCreditDetail> { new SaleCreditDetail { SourceDetailId = 11, ProductName = "P", SoldQuantity = 3 } };
            var service = new FakeSaleService { FindByDocumentResult = Vigente(id: 5), CreditableLinesResult = lines };
            Create(view, service, "ventas.nota_credito").OnSearch();

            Assert.Equal(("Boleta", "000005"), service.FindByDocumentArgs);
            Assert.NotNull(view.ShownSale);
            Assert.Equal(5, service.CreditableLinesArg);
            Assert.Same(lines, view.ShownLines);
            Assert.True(view.GenerateEnabled);
        }

        [Fact]
        public void OnSearch_PartiallyCreditNoted_StillShowsLinesAndAllowsCreditingTheRest()
        {
            var lookup = Vigente();
            lookup.AlreadyCreditNoted = true; // partial: some credited, not all
            var view = new FakeCreditNoteView { DocumentNumberInput = "000005" };
            Create(view, new FakeSaleService { FindByDocumentResult = lookup }, "ventas.nota_credito").OnSearch();

            Assert.Contains(view.ShownMessages, m => m.Contains("parcial"));
            Assert.NotNull(view.ShownLines);
            Assert.True(view.GenerateEnabled);
        }

        [Fact]
        public void OnSearch_FullyCreditNoted_ShowsItButDoesNotEnableGenerate()
        {
            var lookup = Vigente();
            lookup.AlreadyCreditNoted = true;
            lookup.FullyCreditNoted = true;
            var view = new FakeCreditNoteView { DocumentNumberInput = "000005" };
            Create(view, new FakeSaleService { FindByDocumentResult = lookup }, "ventas.nota_credito").OnSearch();

            Assert.Contains(view.ShownMessages, m => m.Contains("por completo"));
            Assert.False(view.GenerateEnabled);
        }

        [Fact]
        public void OnSearch_IsItselfACreditNote_DoesNotEnableGenerate()
        {
            var lookup = Vigente();
            lookup.IsCreditNote = true;
            var view = new FakeCreditNoteView { DocumentNumberInput = "000005" };
            Create(view, new FakeSaleService { FindByDocumentResult = lookup }, "ventas.nota_credito").OnSearch();

            Assert.Contains(view.ShownMessages, m => m.Contains("es una nota de crédito"));
            Assert.False(view.GenerateEnabled);
        }

        [Fact]
        public void OnGenerate_WithoutPermission_IsRejected()
        {
            var view = new FakeCreditNoteView { DocumentNumberInput = "000005", ReasonInput = "Error de tipeo" };
            var service = new FakeSaleService { FindByDocumentResult = Vigente() };
            var presenter = Create(view, service); // no permission
            presenter.OnSearch();

            presenter.OnGenerate();

            Assert.Contains(view.ShownMessages, m => m.Contains("No tiene permiso"));
            Assert.Null(service.CreditNoteArgs);
        }

        [Fact]
        public void OnGenerate_MissingReason_IsRejected()
        {
            var view = new FakeCreditNoteView { DocumentNumberInput = "000005", ReasonInput = "  " };
            var service = new FakeSaleService { FindByDocumentResult = Vigente() };
            var presenter = Create(view, service, "ventas.nota_credito");
            presenter.OnSearch();

            presenter.OnGenerate();

            Assert.Contains(view.ShownMessages, m => m.Contains("motivo"));
            Assert.Null(service.CreditNoteArgs);
        }

        [Fact]
        public void OnGenerate_NoUnitsRequested_ShowsMessageAndDoesNotCallService()
        {
            var view = new FakeCreditNoteView
            {
                DocumentNumberInput = "000005", ReasonInput = "x",
                RequestedQuantities = new List<CreditNoteLineRequest>
                {
                    new CreditNoteLineRequest { SourceDetailId = 1, Quantity = 0 }
                }
            };
            var service = new FakeSaleService { FindByDocumentResult = Vigente() };
            var presenter = Create(view, service, "ventas.nota_credito");
            presenter.OnSearch();

            presenter.OnGenerate();

            Assert.Contains(view.ShownMessages, m => m.Contains("al menos una línea"));
            Assert.Null(service.CreditNoteArgs);
        }

        [Fact]
        public void OnGenerate_NotConfirmed_DoesNothing()
        {
            var view = new FakeCreditNoteView
            {
                DocumentNumberInput = "000005", ReasonInput = "x", ConfirmGenerateResult = false,
                RequestedQuantities = OneUnit()
            };
            var service = new FakeSaleService { FindByDocumentResult = Vigente() };
            var presenter = Create(view, service, "ventas.nota_credito");
            presenter.OnSearch();

            presenter.OnGenerate();

            Assert.Null(service.CreditNoteArgs);
        }

        [Fact]
        public void OnGenerate_Succeeds_CallsServiceWithTheRequestedLinesAndCompletes()
        {
            var view = new FakeCreditNoteView
            {
                DocumentNumberInput = "000005", ReasonInput = "Devolución",
                RequestedQuantities = new List<CreditNoteLineRequest>
                {
                    new CreditNoteLineRequest { SourceDetailId = 11, Quantity = 2 },
                    new CreditNoteLineRequest { SourceDetailId = 12, Quantity = 0 } // dropped by the presenter
                }
            };
            var service = new FakeSaleService { FindByDocumentResult = Vigente(id: 42), CreateCreditNoteResult = CreditNoteResult.Ok };
            var presenter = Create(view, service, "ventas.nota_credito");
            presenter.OnSearch();

            presenter.OnGenerate();

            Assert.Equal((42, 7, "Devolución"), service.CreditNoteArgs);
            Assert.Equal(new[] { (11, 2) }, service.CreditNoteLines.Select(l => (l.SourceDetailId, l.Quantity)).ToArray());
            Assert.Contains(view.ShownMessages, m => m.Contains("Nota de crédito emitida"));
            Assert.Equal(1, view.CreditNoteCompletedCount);
        }

        [Fact]
        public void OnGenerate_ServiceReportsQuantityExceedsRemaining_ShowsMessageAndDoesNotComplete()
        {
            var view = new FakeCreditNoteView
            {
                DocumentNumberInput = "000005", ReasonInput = "x", RequestedQuantities = OneUnit()
            };
            var service = new FakeSaleService { FindByDocumentResult = Vigente(), CreateCreditNoteResult = CreditNoteResult.QuantityExceedsRemaining };
            var presenter = Create(view, service, "ventas.nota_credito");
            presenter.OnSearch();

            presenter.OnGenerate();

            Assert.Contains(view.ShownMessages, m => m.Contains("queda por acreditar"));
            Assert.Equal(0, view.CreditNoteCompletedCount);
        }
    }
}
