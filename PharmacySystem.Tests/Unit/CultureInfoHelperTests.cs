using System.Globalization;
using System.Linq;
using System.Threading;
using PharmacySystem.Helpers;
using Xunit;

namespace PharmacySystem.Tests.Unit
{
    // SetCurrency mutates a process-wide static field (CultureInfoHelper._cultureInfo), and
    // SaleServiceTests (in the "Database" collection) also calls FormatAsCurrency. Sharing that
    // collection here serializes both against each other so a test that temporarily switches
    // currency can never run concurrently with one that assumes the default is still active.
    [Collection("Database")]
    public class CultureInfoHelperTests
    {
        [Fact]
        public void CultureInfoConverterDecimal_UsesInvariantDotSeparator()
        {
            Assert.Equal("1234.50", CultureInfoHelper.CultureInfoConverterDecimal(1234.5m));
        }

        [Fact]
        public void CultureInfoConverterStringToDecimal_DollarPrefixedValue_ParsesEcuadorCommaDecimal()
        {
            // "$1234,50" is what FormatAsCurrency actually produces for small amounts:
            // "$" prefix, "," as the decimal separator (es-EC).
            Assert.Equal(1234.50m, CultureInfoHelper.CultureInfoConverterStringToDecimal("$1234,50"));
        }

        [Fact]
        public void CultureInfoConverterStringToDecimal_PlainCommaDecimal_ParsesAsDecimalPoint()
        {
            Assert.Equal(1234.50m, CultureInfoHelper.CultureInfoConverterStringToDecimal("1234,50"));
        }

        [Fact]
        public void CultureInfoConverterStringToDecimal_PlainDotDecimal_ParsesAsTyped()
        {
            // Manually typed input follows the "##.##" hint shown to the user (no "$", no grouping).
            Assert.Equal(12.50m, CultureInfoHelper.CultureInfoConverterStringToDecimal("12.50"));
        }

        [Theory]
        [InlineData(9.99)]
        [InlineData(999.99)]
        [InlineData(1234.50)]      // regression: used to corrupt to 123450 or throw once >= 1000
        [InlineData(12345.67)]
        [InlineData(1000000.01)]
        public void FormatAsCurrency_ThenConverterStringToDecimal_RoundTripsExactly(double amount)
        {
            decimal original = (decimal)amount;

            string formatted = CultureInfoHelper.FormatAsCurrency(original);
            decimal parsedBack = CultureInfoHelper.CultureInfoConverterStringToDecimal(formatted);

            Assert.Equal(original, parsedBack);
        }

        [Fact]
        public void CultureInfoConverterStringToDecimal_RoundTrip_IsIndependentOfThreadCulture()
        {
            // The old implementation used Convert.ToDecimal(string), which reads the ambient
            // thread culture; on an en-US machine it threw for totals >= 1000, and on an
            // es-EC-like machine it silently returned a value 100x too large. The fix parses
            // with explicit cultures, so the outcome must not depend on CurrentCulture.
            CultureInfo original = Thread.CurrentThread.CurrentCulture;
            try
            {
                foreach (string cultureName in new[] { "en-US", "es-EC", "fr-FR" })
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureName);

                    string formatted = CultureInfoHelper.FormatAsCurrency(1234.50m);
                    decimal parsedBack = CultureInfoHelper.CultureInfoConverterStringToDecimal(formatted);

                    Assert.Equal(1234.50m, parsedBack);
                }
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = original;
            }
        }

        [Fact]
        public void FormatAsCurrency_IncludesDollarSign()
        {
            string formatted = CultureInfoHelper.FormatAsCurrency(10m);

            Assert.Contains("$", formatted);
        }

        [Fact]
        public void CustomCultureInfo_ReturnsEcuadorCulture()
        {
            Assert.Equal("es-EC", CultureInfoHelper.CustomCultureInfo().Name);
        }

        [Fact]
        public void SupportedCurrencies_AllEntriesResolveToConstructibleCultures()
        {
            Assert.NotEmpty(CultureInfoHelper.SupportedCurrencies);

            foreach (var currency in CultureInfoHelper.SupportedCurrencies)
            {
                // Throws if the culture name is invalid; that alone is the assertion.
                new CultureInfo((string)currency.Value);
            }
        }

        [Fact]
        public void SetCurrency_SupportedCultureName_ChangesActiveCurrency()
        {
            try
            {
                CultureInfoHelper.SetCurrency("es-CL");

                Assert.Equal("es-CL", CultureInfoHelper.CustomCultureInfo().Name);
            }
            finally
            {
                CultureInfoHelper.SetCurrency("es-EC");
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("not-a-culture")]
        [InlineData("de-DE")]           // valid culture, but not in SupportedCurrencies
        public void SetCurrency_UnsupportedOrInvalidName_FallsBackToDefault(string cultureName)
        {
            try
            {
                CultureInfoHelper.SetCurrency("es-CL");
                CultureInfoHelper.SetCurrency(cultureName);

                Assert.Equal("es-EC", CultureInfoHelper.CustomCultureInfo().Name);
            }
            finally
            {
                CultureInfoHelper.SetCurrency("es-EC");
            }
        }

        [Fact]
        public void SetCurrency_ChileanPeso_FormatsWithZeroDecimalsAndStillRoundTrips()
        {
            try
            {
                CultureInfoHelper.SetCurrency("es-CL");

                // CLP has no minor currency unit, so FormatAsCurrency rounds to whole pesos -
                // the round trip is only exact for whole-peso amounts, which is correct for CLP.
                string formatted = CultureInfoHelper.FormatAsCurrency(1235m);
                decimal parsedBack = CultureInfoHelper.CultureInfoConverterStringToDecimal(formatted);

                Assert.DoesNotContain(",", formatted);
                Assert.Equal(1235m, parsedBack);
            }
            finally
            {
                CultureInfoHelper.SetCurrency("es-EC");
            }
        }
    }
}
