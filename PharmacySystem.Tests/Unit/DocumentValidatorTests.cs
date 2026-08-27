using PharmacySystem.Validators;
using Xunit;

namespace PharmacySystem.Tests.Unit
{
    // Country-agnostic replacement for the former Ecuadorian cedula/RUC check-digit validator.
    // It only checks shape (length and character set); uniqueness is enforced by the database.
    public class DocumentValidatorTests
    {
        [Theory]
        [InlineData("1712345675")]        // Ecuadorian cedula still passes - it is just digits
        [InlineData("12345678")]          // 8-digit national id
        [InlineData("20-12345678-9")]     // CUIT-style with separators
        [InlineData("X1234567L")]         // alphanumeric id / passport
        [InlineData("AB.123.456")]        // dots allowed
        public void IsValid_WellFormedDocuments_ReturnTrue(string document)
        {
            Assert.True(DocumentValidator.IsValid(document));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("ab")]                                  // shorter than MinLength
        [InlineData("123456789012345678901")]              // longer than MaxLength (21)
        [InlineData("12 34")]                               // space not allowed
        [InlineData("12/34")]                               // slash not allowed
        [InlineData("abc$")]                                // symbol not allowed
        public void IsValid_MalformedDocuments_ReturnFalse(string document)
        {
            Assert.False(DocumentValidator.IsValid(document));
        }

        [Fact]
        public void IsValid_TrimsSurroundingWhitespace()
        {
            Assert.True(DocumentValidator.IsValid("  12345678  "));
        }
    }
}
