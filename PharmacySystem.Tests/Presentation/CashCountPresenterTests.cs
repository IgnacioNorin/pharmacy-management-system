using System;
using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using Xunit;

namespace PharmacySystem.Tests.Presentation
{
    public class CashCountPresenterTests
    {
        private static CashCountPresenter CreatePresenter(FakeCashCountView view, FakeCashCountService service, bool withPermission = true)
            => new CashCountPresenter(view, service, withPermission ? TestUser.With("caja.acceso") : TestUser.With());

        private static CashCount PreparedCount(params (string method, decimal expected)[] lines) => new CashCount
        {
            periodStart = new DateTime(2026, 8, 30, 8, 0, 0),
            periodEnd = new DateTime(2026, 8, 30, 20, 0, 0),
            lines = lines.Select(l => new CashCountLine { paymentMethod = l.method, expectedAmount = l.expected, countedAmount = 0m }).ToList()
        };

        [Fact]
        public void OnLoad_ShowsPeriodExpectedLinesAndInitialTotals()
        {
            var view = new FakeCashCountView();
            var service = new FakeCashCountService
            {
                PrepareResult = PreparedCount(("Efectivo", 1000m), ("Tarjeta", 500m), ("Transferencia", 0m))
            };

            CreatePresenter(view, service).OnLoad();

            Assert.Equal(new DateTime(2026, 8, 30, 8, 0, 0), view.ShownPeriod?.Start);
            Assert.Equal(3, view.ShownLines.Count);
            Assert.Equal(1000m, view.ShownLines.Single(r => r.PaymentMethod == "Efectivo").Expected);
            // Nothing counted yet: counted 0, difference = -expected.
            Assert.Equal((1500m, 0m, -1500m), view.ShownTotals);
        }

        [Fact]
        public void OnCountedChanged_RecomputesTotalsFromTheViewFields()
        {
            var view = new FakeCashCountView();
            var service = new FakeCashCountService { PrepareResult = PreparedCount(("Efectivo", 1000m), ("Tarjeta", 500m)) };
            var presenter = CreatePresenter(view, service);
            presenter.OnLoad();

            view.CountedTexts["Efectivo"] = "980";
            view.CountedTexts["Tarjeta"] = "500";
            presenter.OnCountedChanged();

            Assert.Equal((1500m, 1480m, -20m), view.ShownTotals);
        }

        [Fact]
        public void OnRegister_WithoutPermission_ShowsDeniedAndDoesNotSave()
        {
            var view = new FakeCashCountView();
            var service = new FakeCashCountService { PrepareResult = PreparedCount(("Efectivo", 100m)) };
            var presenter = CreatePresenter(view, service, withPermission: false);
            presenter.OnLoad();

            presenter.OnRegister();

            Assert.Contains(view.ShownMessages, m => m.Contains("No tiene permiso"));
            Assert.Null(service.RegisteredWith);
        }

        [Fact]
        public void OnRegister_InvalidCountedAmount_ShowsValidationAndDoesNotSave()
        {
            var view = new FakeCashCountView();
            var service = new FakeCashCountService { PrepareResult = PreparedCount(("Efectivo", 100m)) };
            var presenter = CreatePresenter(view, service);
            presenter.OnLoad();

            view.CountedTexts["Efectivo"] = "abc";
            presenter.OnRegister();

            Assert.Contains(view.ShownMessages, m => m.Contains("no es un número válido"));
            Assert.Null(service.RegisteredWith);
        }

        [Fact]
        public void OnRegister_NegativeCountedAmount_ShowsValidationAndDoesNotSave()
        {
            var view = new FakeCashCountView();
            var service = new FakeCashCountService { PrepareResult = PreparedCount(("Efectivo", 100m)) };
            var presenter = CreatePresenter(view, service);
            presenter.OnLoad();

            view.CountedTexts["Efectivo"] = "-5";
            presenter.OnRegister();

            Assert.Contains(view.ShownMessages, m => m.Contains("no puede ser negativo"));
            Assert.Null(service.RegisteredWith);
        }

        [Fact]
        public void OnRegister_Valid_SavesTheCountWithPeriodUserBlankFieldsAsZeroAndNotes()
        {
            var view = new FakeCashCountView { Notes = "  cierre de turno  " };
            var service = new FakeCashCountService
            {
                PrepareResult = PreparedCount(("Efectivo", 1000m), ("Tarjeta", 500m), ("Transferencia", 0m)),
                RegisterResult = 42
            };
            var presenter = CreatePresenter(view, service);
            presenter.OnLoad();

            view.CountedTexts["Efectivo"] = "995";
            // Tarjeta left blank -> counts as 0. Transferencia left blank too.
            presenter.OnRegister();

            CashCount saved = service.RegisteredWith;
            Assert.NotNull(saved);
            Assert.Equal(new DateTime(2026, 8, 30, 8, 0, 0), saved.periodStart);
            Assert.Equal(new DateTime(2026, 8, 30, 20, 0, 0), saved.periodEnd);
            Assert.Equal("cierre de turno", saved.notes);
            Assert.Equal(3, saved.lines.Count);
            Assert.Equal(995m, saved.lines.Single(l => l.paymentMethod == "Efectivo").countedAmount);
            Assert.Equal(0m, saved.lines.Single(l => l.paymentMethod == "Tarjeta").countedAmount);
            Assert.Equal(500m, saved.lines.Single(l => l.paymentMethod == "Tarjeta").expectedAmount);
            Assert.True(view.RegisteredCalled);
            Assert.Contains(view.ShownMessages, m => m.Contains("registrado"));
        }

        [Fact]
        public void OnRegister_ServiceReturnsZero_ShowsErrorAndDoesNotClose()
        {
            var view = new FakeCashCountView();
            var service = new FakeCashCountService { PrepareResult = PreparedCount(("Efectivo", 100m)), RegisterResult = 0 };
            var presenter = CreatePresenter(view, service);
            presenter.OnLoad();

            view.CountedTexts["Efectivo"] = "100";
            presenter.OnRegister();

            Assert.Contains(view.ShownMessages, m => m.Contains("No se pudo registrar"));
            Assert.False(view.RegisteredCalled);
        }

        [Fact]
        public void OnRegister_BeforeOnLoad_DoesNothing()
        {
            var view = new FakeCashCountView();
            var service = new FakeCashCountService();

            CreatePresenter(view, service).OnRegister();

            Assert.Empty(view.ShownMessages);
            Assert.Null(service.RegisteredWith);
        }
    }
}
