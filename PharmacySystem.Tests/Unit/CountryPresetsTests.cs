using PharmacySystem.Model;
using Xunit;

namespace PharmacySystem.Tests.Unit
{
    public class CountryPresetsTests
    {
        [Theory]
        [InlineData("CL")]
        [InlineData("cl")]
        [InlineData(" CL ")]
        public void ForCode_KnownCode_ReturnsChile(string code)
        {
            Assert.Same(CountryPresets.Chile, CountryPresets.ForCode(code));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("XX")]
        public void ForCode_MissingOrUnknownCode_ReturnsGeneric(string code)
        {
            Assert.Same(CountryPresets.Generic, CountryPresets.ForCode(code));
        }

        [Fact]
        public void Chile_HasTheChileanDefaults()
        {
            Assert.Equal("CL", CountryPresets.Chile.Code);
            Assert.Equal(19m, CountryPresets.Chile.DefaultTaxRate);
            Assert.Equal("es-CL", CountryPresets.Chile.CurrencyCulture);
            Assert.Equal(CountryPresets.ChileanRutScheme, CountryPresets.Chile.RecipientDocumentScheme);
            Assert.False(CountryPresets.Chile.IsGeneric);
        }

        [Fact]
        public void Generic_MakesNoNationalAssumptions()
        {
            Assert.Equal("", CountryPresets.Generic.Code);
            Assert.True(CountryPresets.Generic.IsGeneric);
            Assert.Equal(CountryPresets.GenericScheme, CountryPresets.Generic.RecipientDocumentScheme);
        }

        [Fact]
        public void All_ListsGenericFirstThenChile()
        {
            Assert.Equal(new[] { CountryPresets.Generic, CountryPresets.Chile }, CountryPresets.All);
        }

        [Fact]
        public void EveryPreset_ExposesItsSaleDocumentTypes()
        {
            foreach (var preset in CountryPresets.All)
            {
                Assert.NotNull(preset.SaleDocumentTypes);
                Assert.NotEmpty(preset.SaleDocumentTypes);
            }

            // Both presets offer Boleta / Factura today; a real second country supplies its own.
            Assert.Equal(new[] { "Boleta", "Factura" }, CountryPresets.Generic.SaleDocumentTypes);
            Assert.Equal(new[] { "Boleta", "Factura" }, CountryPresets.Chile.SaleDocumentTypes);
        }
    }
}
