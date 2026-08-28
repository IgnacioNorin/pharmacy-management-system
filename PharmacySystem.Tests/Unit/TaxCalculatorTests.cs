using PharmacySystem.Helpers;
using Xunit;

namespace PharmacySystem.Tests.Unit
{
    public class TaxCalculatorTests
    {
        [Fact]
        public void Compute_AllAffected_BacksOutTheNetFromVatIncludedPrices()
        {
            var result = TaxCalculator.Compute(
                new[] { (SubTotal: 1190m, TaxAffected: true) }, 19m);

            Assert.Equal(1000m, result.Net);
            Assert.Equal(190m, result.Tax);
            Assert.Equal(0m, result.Exempt);
            Assert.Equal(1190m, result.Total);
        }

        [Fact]
        public void Compute_AllExempt_LeavesEverythingAsExemptWithNoTax()
        {
            var result = TaxCalculator.Compute(
                new[] { (SubTotal: 5000m, TaxAffected: false), (SubTotal: 2500m, TaxAffected: false) }, 19m);

            Assert.Equal(0m, result.Net);
            Assert.Equal(0m, result.Tax);
            Assert.Equal(7500m, result.Exempt);
            Assert.Equal(7500m, result.Total);
        }

        [Fact]
        public void Compute_MixedCart_SplitsAffectedAndExempt()
        {
            var result = TaxCalculator.Compute(new[]
            {
                (SubTotal: 1190m, TaxAffected: true),
                (SubTotal: 3000m, TaxAffected: false)
            }, 19m);

            Assert.Equal(1000m, result.Net);
            Assert.Equal(190m, result.Tax);
            Assert.Equal(3000m, result.Exempt);
            Assert.Equal(4190m, result.Total);
        }

        [Fact]
        public void Compute_RoundsNetToTheWholeUnit_AndTaxAbsorbsTheRemainder()
        {
            // 999 / 1.19 = 839.4957...; net rounds to 839, tax = 999 - 839 = 160.
            var result = TaxCalculator.Compute(new[] { (SubTotal: 999m, TaxAffected: true) }, 19m);

            Assert.Equal(839m, result.Net);
            Assert.Equal(160m, result.Tax);
            Assert.Equal(999m, result.Total);
        }

        [Fact]
        public void Compute_ZeroRate_TreatsEverythingAsNet()
        {
            var result = TaxCalculator.Compute(new[] { (SubTotal: 1000m, TaxAffected: true) }, 0m);

            Assert.Equal(1000m, result.Net);
            Assert.Equal(0m, result.Tax);
            Assert.Equal(1000m, result.Total);
        }

        [Fact]
        public void Compute_DifferentRate_UsesTheGivenPercentage()
        {
            // Peru: 18%. 1180 / 1.18 = 1000.
            var result = TaxCalculator.Compute(new[] { (SubTotal: 1180m, TaxAffected: true) }, 18m);

            Assert.Equal(1000m, result.Net);
            Assert.Equal(180m, result.Tax);
        }
    }
}
