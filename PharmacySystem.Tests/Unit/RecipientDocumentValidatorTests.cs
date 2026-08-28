using PharmacySystem.Model;
using PharmacySystem.Validators;
using Xunit;

namespace PharmacySystem.Tests.Unit
{
    public class RecipientDocumentValidatorTests
    {
        [Theory]
        [InlineData("12.345.678-5", true)]   // valid modulo-11 check digit
        [InlineData("12.345.678-9", false)]  // wrong check digit
        [InlineData("AB-1234.5", false)]     // valid generic format, but not a RUT
        public void ChileanScheme_UsesModulo11(string document, bool expected)
        {
            Assert.Equal(expected, RecipientDocumentValidator.IsValid(document, CountryPresets.ChileanRutScheme));
        }

        [Theory]
        [InlineData("12.345.678-9", true)]   // format only: 3-20 chars of letters/digits/./-
        [InlineData("AB-1234.5", true)]
        [InlineData("@@", false)]
        [InlineData("", false)]
        public void GenericScheme_ChecksFormatOnly(string document, bool expected)
        {
            Assert.Equal(expected, RecipientDocumentValidator.IsValid(document, CountryPresets.GenericScheme));
        }

        [Fact]
        public void UnknownScheme_FallsBackToTheGenericCheck()
        {
            Assert.True(RecipientDocumentValidator.IsValid("12.345.678-9", "something-else"));
        }
    }
}
