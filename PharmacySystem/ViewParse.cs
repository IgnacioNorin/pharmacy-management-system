using System.Windows.Forms;
using PharmacySystem.Model;

namespace PharmacySystem
{
    // Safe reads for the hidden index / id text boxes and the ComboBoxItem combos the ABM forms
    // use. These fields are set by the forms themselves, so in the normal flow they always hold
    // a valid value - but a stray non-numeric value or an empty combo must not throw and (with
    // no crash-stopping global handler that also shows a friendly message) close the operation
    // abruptly. DEF-20.
    internal static class ViewParse
    {
        public static int Int(string text) =>
            int.TryParse((text ?? string.Empty).Trim(), out int value) ? value : 0;

        public static int ComboInt(ComboBox combo, int fallback = 0) =>
            combo?.SelectedItem is ComboBoxItem item && int.TryParse((item.Value ?? string.Empty).ToString().Trim(), out int value)
                ? value
                : fallback;

        public static string ComboText(ComboBox combo, string fallback = "") =>
            combo?.SelectedItem is ComboBoxItem item ? (item.Text ?? fallback) : fallback;

        public static string ComboValueText(ComboBox combo, string fallback = "") =>
            combo?.SelectedItem is ComboBoxItem item ? (item.Value ?? fallback).ToString() : fallback;
    }
}
