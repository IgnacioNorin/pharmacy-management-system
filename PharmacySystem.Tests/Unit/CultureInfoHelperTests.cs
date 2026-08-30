using System.Globalization;
using System.Threading;
using PharmacySystem.Helpers;
using Xunit;

namespace PharmacySystem.Tests.Unit
{
    // The system is CLP-only: CultureInfoHelper has a fixed es-CL culture, no SetCurrency.
    public class CultureInfoHelperTests
    {
        [Fact]
        public void CultureInfoConverterDecimal_UsesInvariantDotSeparator()
        {
            Assert.Equal("1234.5", CultureInfoHelper.CultureInfoConverterDecimal(1234.5m));
        }

        [Fact]
        public void FormatAsCurrency_FormatsAsChileanPesos_NoDecimals_DotThousands()
        {
            Assert.Equal("$2.000.000", CultureInfoHelper.FormatAsCurrency(2000000m));
            Assert.Equal("$1.235", CultureInfoHelper.FormatAsCurrency(1235m));
            Assert.Equal("$10", CultureInfoHelper.FormatAsCurrency(10m));
        }

        [Fact]
        public void FormatAsCurrency_RoundsToWholePesos()
        {
            Assert.Equal("$1.235", CultureInfoHelper.FormatAsCurrency(1234.50m));
            Assert.Equal("$1.234", CultureInfoHelper.FormatAsCurrency(1234.49m));
        }

        [Fact]
        public void RoundMoney_RoundsHalfAwayFromZero()
        {
            Assert.Equal(1235m, CultureInfoHelper.RoundMoney(1234.5m));
            Assert.Equal(-1235m, CultureInfoHelper.RoundMoney(-1234.5m));
            Assert.Equal(1234m, CultureInfoHelper.RoundMoney(1234.4m));
        }

        [Theory]
        [InlineData("$2.000.000", 2000000)]
        [InlineData("2.000.000", 2000000)]
        [InlineData("2000000", 2000000)]
        [InlineData("1.235", 1235)]
        [InlineData("1235", 1235)]
        [InlineData("1234,50", 1235)]   // a lone comma = decimal point, then rounded to whole pesos
        public void CultureInfoConverterStringToDecimal_ParsesPesoInput(string input, int expected)
        {
            Assert.Equal(expected, CultureInfoHelper.CultureInfoConverterStringToDecimal(input));
        }

        [Theory]
        [InlineData(2000000)]
        [InlineData(999)]
        [InlineData(1235)]
        [InlineData(12345678)]
        public void FormatAsCurrency_ThenParse_RoundTripsForWholePesos(int amount)
        {
            string formatted = CultureInfoHelper.FormatAsCurrency(amount);
            decimal parsedBack = CultureInfoHelper.CultureInfoConverterStringToDecimal(formatted);

            Assert.Equal((decimal)amount, parsedBack);
        }

        [Fact]
        public void RoundTrip_IsIndependentOfThreadCulture()
        {
            CultureInfo original = Thread.CurrentThread.CurrentCulture;
            try
            {
                foreach (string cultureName in new[] { "en-US", "es-CL", "fr-FR" })
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureName);

                    string formatted = CultureInfoHelper.FormatAsCurrency(1234m);
                    decimal parsedBack = CultureInfoHelper.CultureInfoConverterStringToDecimal(formatted);

                    Assert.Equal(1234m, parsedBack);
                }
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = original;
            }
        }

        [Fact]
        public void CustomCultureInfo_IsChileanPeso()
        {
            Assert.Equal("es-CL", CultureInfoHelper.CustomCultureInfo().Name);
        }
    }
}
