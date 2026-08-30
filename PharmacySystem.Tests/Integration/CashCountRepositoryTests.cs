using System;
using System.Data.SqlClient;
using System.Linq;
using PharmacySystem.Data;
using PharmacySystem.Model;
using Xunit;

namespace PharmacySystem.Tests.Integration
{
    [Collection("Database")]
    public class CashCountRepositoryTests
    {
        private static readonly ICashCountRepository Repository =
            new CashCountRepository(SqlConnectionFactory.FromConfiguration());

        [Fact]
        public void GetExpectedTotals_SumsSaleTotalsPerPaymentMethodInThePeriod()
        {
            DateTime start = new DateTime(2099, 1, 1);
            DateTime end = new DateTime(2099, 1, 2);
            DateTime outside = new DateTime(2099, 2, 1);

            InsertSale(150m, "Efectivo", start.AddHours(9));
            InsertSale(50m, "Efectivo", start.AddHours(10));
            InsertSale(200m, "Tarjeta", start.AddHours(11));
            InsertSale(999m, "Efectivo", outside); // outside the window

            try
            {
                var totals = Repository.GetExpectedTotals(start, end);

                Assert.Equal(200m, totals.Single(t => t.paymentMethod == "Efectivo").expectedAmount);
                Assert.Equal(200m, totals.Single(t => t.paymentMethod == "Tarjeta").expectedAmount);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery(
                    "DELETE FROM sale WHERE date_registered >= @a AND date_registered < @b OR date_registered = @c",
                    new SqlParameter("@a", start), new SqlParameter("@b", end), new SqlParameter("@c", outside));
            }
        }

        [Fact]
        public void Register_PersistsHeaderAndLines_AndHistoryReadsThemBack()
        {
            var count = new CashCount
            {
                periodStart = new DateTime(2099, 3, 1, 8, 0, 0),
                periodEnd = new DateTime(2099, 3, 1, 20, 0, 0),
                userId = null,
                notes = "prueba de integración",
                lines =
                {
                    new CashCountLine { paymentMethod = "Efectivo", expectedAmount = 1000m, countedAmount = 990m },
                    new CashCountLine { paymentMethod = "Tarjeta", expectedAmount = 500m, countedAmount = 500m },
                    new CashCountLine { paymentMethod = "Transferencia", expectedAmount = 0m, countedAmount = 0m }
                }
            };

            int id = 0;
            try
            {
                id = Repository.Register(count);
                Assert.True(id > 0);

                Assert.True(Repository.GetLastPeriodEnd() >= new DateTime(2099, 3, 1, 20, 0, 0));

                CashCount saved = Repository.History().Single(c => c.id == id);
                Assert.Equal("prueba de integración", saved.notes);
                Assert.Equal(3, saved.lines.Count);
                Assert.Equal(-10m, saved.Difference); // 1490 counted - 1500 expected
                Assert.Equal(990m, saved.lines.Single(l => l.paymentMethod == "Efectivo").countedAmount);
            }
            finally
            {
                if (id > 0)
                {
                    SqlTestHelper.ExecuteNonQuery("DELETE FROM cash_count_line WHERE cash_count_id = @id", new SqlParameter("@id", id));
                    SqlTestHelper.ExecuteNonQuery("DELETE FROM cash_count WHERE id = @id", new SqlParameter("@id", id));
                }
            }
        }

        private static void InsertSale(decimal total, string method, DateTime when)
        {
            SqlTestHelper.ExecuteNonQuery(
                "INSERT INTO sale(total_amount, amount_received, payment_method, date_registered) " +
                "VALUES (@t, @t, @m, @d)",
                new SqlParameter("@t", total), new SqlParameter("@m", method), new SqlParameter("@d", when));
        }
    }
}
