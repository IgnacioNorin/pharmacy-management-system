using PharmacySystem.Validators;
using Xunit;

namespace PharmacySystem.Tests.Unit
{
    // Test vectors were generated independently from the public SRI (Ecuador) module-10/11
    // check-digit algorithm, not copied from RuleValidation.cs, so they exercise it as an oracle.
    public class RUCValidatorTests
    {
        [Theory]
        [InlineData("1712345675")]        // valid cedula (provincia 17, tercer digito 1)
        [InlineData("1712345675001")]     // valid RUC persona natural (cedula + 001)
        [InlineData("1761234510001")]     // valid RUC sociedad publica (tercer digito 6)
        [InlineData("1791234561001")]     // valid RUC sociedad privada (tercer digito 9)
        public void ValidarIdentificacion_ValidNumbers_ReturnsTrue(string identificacion)
        {
            Assert.True(RUCValidator.ValidarIdentificacion(identificacion));
        }

        [Theory]
        [InlineData("1712345676")]        // wrong check digit
        [InlineData("123")]               // too short
        [InlineData("abcdefghij")]        // non numeric, length 10
        [InlineData("9912345678")]        // invalid province (99)
        [InlineData(null)]
        [InlineData("")]
        public void ValidarIdentificacion_InvalidNumbers_ReturnsFalse(string identificacion)
        {
            Assert.False(RUCValidator.ValidarIdentificacion(identificacion));
        }

        [Fact]
        public void EsRUCValido_CorruptedCheckDigitOnValidPublicRuc_ReturnsFalse()
        {
            // "1761234510001" is a valid sociedad publica RUC (see the vectors above);
            // flipping its check digit (index 8) must break validation.
            Assert.False(RUCValidator.EsRUCValido("1761234560001"));
        }

        [Fact]
        public void EsRUCValido_WrongLength_ReturnsFalse()
        {
            Assert.False(RUCValidator.EsRUCValido("123"));
        }
    }
}
