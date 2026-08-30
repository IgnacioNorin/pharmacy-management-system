using System;
using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Business;
using PharmacySystem.Model;
using Xunit;

namespace PharmacySystem.Tests.Business
{
    public class CashCountServiceTests
    {
        [Fact]
        public void PrepareCurrent_PeriodStart_IsTheLastCountEndWhenThereIsOne()
        {
            var lastEnd = new DateTime(2026, 8, 29, 20, 0, 0);
            var repo = new FakeCashCountRepository { LastPeriodEnd = lastEnd, EarliestSaleDate = new DateTime(2020, 1, 1) };

            CashCount prepared = new CashCountService(repo).PrepareCurrent();

            Assert.Equal(lastEnd, prepared.periodStart);
            Assert.Equal(lastEnd, repo.ExpectedTotalsRequestedFor?.Start);
        }

        [Fact]
        public void PrepareCurrent_NoPriorCount_FallsBackToTheFirstSaleDate()
        {
            var firstSale = new DateTime(2026, 8, 1, 9, 0, 0);
            var repo = new FakeCashCountRepository { LastPeriodEnd = null, EarliestSaleDate = firstSale };

            CashCount prepared = new CashCountService(repo).PrepareCurrent();

            Assert.Equal(firstSale, prepared.periodStart);
        }

        [Fact]
        public void PrepareCurrent_NoPriorCountAndNoSales_FallsBackToToday()
        {
            var repo = new FakeCashCountRepository { LastPeriodEnd = null, EarliestSaleDate = null };

            CashCount prepared = new CashCountService(repo).PrepareCurrent();

            Assert.Equal(DateTime.Today, prepared.periodStart);
        }

        [Fact]
        public void PrepareCurrent_HasOneLinePerSelectableMethod_WithExpectedFilledAndCountedZero()
        {
            var repo = new FakeCashCountRepository
            {
                LastPeriodEnd = new DateTime(2026, 8, 30, 8, 0, 0),
                ExpectedTotals = new List<CashCountLine>
                {
                    new CashCountLine { paymentMethod = "Efectivo", expectedAmount = 1234.50m },
                    new CashCountLine { paymentMethod = "Tarjeta", expectedAmount = 800m }
                    // no Transferencia row from the query
                }
            };

            CashCount prepared = new CashCountService(repo).PrepareCurrent();

            Assert.Equal(PaymentMethods.Selectable.Length, prepared.lines.Count);
            Assert.Equal(1234.50m, prepared.lines.Single(l => l.paymentMethod == "Efectivo").expectedAmount);
            Assert.Equal(800m, prepared.lines.Single(l => l.paymentMethod == "Tarjeta").expectedAmount);
            // A method with no sales in the period still gets a row, at 0.
            Assert.Equal(0m, prepared.lines.Single(l => l.paymentMethod == "Transferencia").expectedAmount);
            Assert.All(prepared.lines, l => Assert.Equal(0m, l.countedAmount));
        }

        [Fact]
        public void PrepareCurrent_ClampsStartToNowIfALaterCountEndIsRecorded()
        {
            var repo = new FakeCashCountRepository { LastPeriodEnd = DateTime.Now.AddDays(1) };

            CashCount prepared = new CashCountService(repo).PrepareCurrent();

            Assert.True(prepared.periodStart <= prepared.periodEnd);
        }
    }
}
