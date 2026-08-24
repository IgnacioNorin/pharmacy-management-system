using System;
using System.Globalization;
using System.Threading;
using PharmacySystem.Helpers;
using Xunit;

namespace PharmacySystem.Tests.Unit
{
    // CultureInfoConverterStringToDecimal parses with Convert.ToDecimal(string), which
    // reads the current thread culture. Pin it so the test is deterministic regardless
    // of the machine's OS locale.
    public class CultureInfoHelperTests : IDisposable
    {
        private readonly CultureInfo _originalCulture;

        public CultureInfoHelperTests()
        {
            _originalCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        }

        public void Dispose()
        {
            Thread.CurrentThread.CurrentCulture = _originalCulture;
        }

        [Fact]
        public void CultureInfoConverterDecimal_UsesInvariantDotSeparator()
        {
            Assert.Equal("1234.50", CultureInfoHelper.CultureInfoConverterDecimal(1234.5m));
        }

        [Fact]
        public void CultureInfoConverterStringToDecimal_StripsDollarSign()
        {
            Assert.Equal(1234.50m, CultureInfoHelper.CultureInfoConverterStringToDecimal("$1234.50"));
        }

        [Fact]
        public void CultureInfoConverterStringToDecimal_ReplacesCommaWithDot()
        {
            Assert.Equal(1234.50m, CultureInfoHelper.CultureInfoConverterStringToDecimal("1234,50"));
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
