using System.Windows.Forms;
using PharmacySystem;
using PharmacySystem.Model;
using Xunit;

namespace PharmacySystem.UiTests
{
    // ViewParse guards the hidden index/id text boxes and the ComboBoxItem combos the ABM forms
    // read (DEF-20): a stray value or an empty combo must degrade to a harmless default, not throw.
    public class ViewParseTests
    {
        [Theory]
        [InlineData("7", 7)]
        [InlineData("  12  ", 12)]
        [InlineData("", 0)]
        [InlineData(null, 0)]
        [InlineData("not-a-number", 0)]
        [InlineData("3.5", 0)]
        public void Int_ParsesOrFallsBackToZero(string text, int expected)
        {
            Assert.Equal(expected, ViewParse.Int(text));
        }

        [Fact]
        public void ComboInt_NoSelection_ReturnsFallback()
        {
            StaThread.Run(() =>
            {
                using (var combo = new ComboBox())
                {
                    Assert.Equal(0, ViewParse.ComboInt(combo));
                    Assert.Equal(-1, ViewParse.ComboInt(combo, fallback: -1));
                }
            });
        }

        [Fact]
        public void ComboInt_SelectedItem_ReadsTheValue_WhetherBoxedIntOrString()
        {
            StaThread.Run(() =>
            {
                using (var combo = new ComboBox())
                {
                    var boxedInt = new ComboBoxItem { Text = "A", Value = 9 };
                    var stringValue = new ComboBoxItem { Text = "B", Value = "4" };
                    combo.Items.Add(boxedInt);
                    combo.Items.Add(stringValue);

                    combo.SelectedItem = boxedInt;
                    Assert.Equal(9, ViewParse.ComboInt(combo));

                    combo.SelectedItem = stringValue;
                    Assert.Equal(4, ViewParse.ComboInt(combo));
                }
            });
        }

        [Fact]
        public void ComboText_NoSelection_ReturnsFallback()
        {
            StaThread.Run(() =>
            {
                using (var combo = new ComboBox())
                {
                    Assert.Equal("", ViewParse.ComboText(combo));
                    Assert.Equal("Todos", ViewParse.ComboValueText(combo, "Todos"));
                }
            });
        }
    }
}
