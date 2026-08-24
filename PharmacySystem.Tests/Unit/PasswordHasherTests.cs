using PharmacySystem.Helpers;
using Xunit;

namespace PharmacySystem.Tests.Unit
{
    public class PasswordHasherTests
    {
        [Fact]
        public void Hash_ThenVerify_SamePassword_ReturnsTrue()
        {
            string hashed = PasswordHasher.Hash("MyS3cret!");

            Assert.True(PasswordHasher.Verify("MyS3cret!", hashed));
        }

        [Fact]
        public void Verify_WrongPassword_ReturnsFalse()
        {
            string hashed = PasswordHasher.Hash("MyS3cret!");

            Assert.False(PasswordHasher.Verify("WrongPassword", hashed));
        }

        [Fact]
        public void Hash_SamePasswordTwice_ProducesDifferentHashes()
        {
            string hashedFirst = PasswordHasher.Hash("MyS3cret!");
            string hashedSecond = PasswordHasher.Hash("MyS3cret!");

            Assert.NotEqual(hashedFirst, hashedSecond);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("plain-text-password")]
        [InlineData("v1$notanumber$salt$hash")]
        public void IsHashed_NonHashedValues_ReturnsFalse(string value)
        {
            Assert.False(PasswordHasher.IsHashed(value));
        }

        [Fact]
        public void IsHashed_HashedValue_ReturnsTrue()
        {
            string hashed = PasswordHasher.Hash("MyS3cret!");

            Assert.True(PasswordHasher.IsHashed(hashed));
        }

        [Fact]
        public void Verify_PlainTextLegacyValue_ReturnsFalseInsteadOfThrowing()
        {
            Assert.False(PasswordHasher.Verify("MyS3cret!", "MyS3cret!"));
        }
    }
}
