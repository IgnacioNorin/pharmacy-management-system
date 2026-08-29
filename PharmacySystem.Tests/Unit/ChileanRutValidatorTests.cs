using PharmacySystem.Validators;
using Xunit;

namespace PharmacySystem.Tests.Unit
{
    public class ChileanRutValidatorTests
    {
        [Theory]
        [InlineData("11.111.111-1")]
        [InlineData("22222222-2")]
        [InlineData("12.345.678-5")]
        [InlineData("1-9")]
        [InlineData("12345678-5")]      // without dots
        public void IsValid_WellFormedRut_ReturnsTrue(string rut)
        {
            Assert.True(ChileanRutValidator.IsValid(rut));
        }

        [Theory]
        [InlineData("11.111.111-2")]    // wrong check digit
        [InlineData("12.345.678-K")]    // wrong check digit
        [InlineData("abc")]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("1")]               // too short
        [InlineData("12.34X.678-5")]    // non-digit in the body
        public void IsValid_MalformedOrWrongRut_ReturnsFalse(string rut)
        {
            Assert.False(ChileanRutValidator.IsValid(rut));
        }

        [Fact]
        public void IsValid_AcceptsLowercaseK()
        {
            // body 5000001 -> module-11 check digit is K
            Assert.True(ChileanRutValidator.IsValid("5.000.001-K"));
            Assert.True(ChileanRutValidator.IsValid("5000001-k"));
        }
    }
}
