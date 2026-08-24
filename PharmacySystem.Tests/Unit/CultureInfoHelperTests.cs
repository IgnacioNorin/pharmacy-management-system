using System.Globalization;
using System.Threading;
using PharmacySystem.Helpers;
using Xunit;

namespace PharmacySystem.Tests.Unit
{
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
            // "$1234,50" is what FormatAsEcuadorCurrency actually produces for small amounts:
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
        public void FormatAsEcuadorCurrency_ThenConverterStringToDecimal_RoundTripsExactly(double amount)
        {
            decimal original = (decimal)amount;

            string formatted = CultureInfoHelper.FormatAsEcuadorCurrency(original);
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

                    string formatted = CultureInfoHelper.FormatAsEcuadorCurrency(1234.50m);
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
        public void FormatAsEcuadorCurrency_IncludesDollarSign()
        {
            string formatted = CultureInfoHelper.FormatAsEcuadorCurrency(10m);

            Assert.Contains("$", formatted);
        }

        [Fact]
        public void CustomCultureInfo_ReturnsEcuadorCulture()
        {
            Assert.Equal("es-EC", CultureInfoHelper.CustomCultureInfo().Name);
        }
    }
}
