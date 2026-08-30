using System.Linq;
using PharmacySystem.Helpers;
using PharmacySystem.Model;
using Xunit;

namespace PharmacySystem.Tests.Unit
{
    public class PasswordGeneratorTests
    {
        [Fact]
        public void Generate_ProducesEnoughSignificantCharacters()
        {
            string generated = PasswordGenerator.Generate();

            Assert.True(generated.Replace("-", "").Length >= PasswordRules.MinLength);
        }

        [Fact]
        public void Generate_HasNoVisuallyAmbiguousCharacters()
        {
            for (int i = 0; i < 200; i++)
            {
                string generated = PasswordGenerator.Generate().Replace("-", "");
                Assert.DoesNotContain(generated, c => "0O1lI".Contains(c));
                Assert.All(generated, c => Assert.True(char.IsLetterOrDigit(c)));
            }
        }

        [Fact]
        public void Generate_IsRandom_NotRepeatingAcrossCalls()
        {
            var seen = Enumerable.Range(0, 50).Select(_ => PasswordGenerator.Generate()).ToList();

            Assert.Equal(seen.Count, seen.Distinct().Count());
        }
    }
}
