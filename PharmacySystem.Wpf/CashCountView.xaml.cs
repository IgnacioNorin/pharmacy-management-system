using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PharmacySystem.Helpers;
using PharmacySystem.Presentation;

namespace PharmacySystem.Ui
{
    // "Arqueo de caja": one row per payment method with its expected total and an editable
    // "counted" field, a live totals line and a notes box. Built in code, hosted inline in
    // MainWindow's content area.
    public class CashCountView : System.Windows.Controls.UserControl, ICashCountView
    {
        private class RowCtl
        {
            public string Method = string.Empty;
            public TextBox Counted = null!;
            public TextBlock Diff = null!;
        }

        private readonly CashCountPresenter _presenter;
        private readonly List<RowCtl> _rows = new List<RowCtl>();
        private readonly Grid _rowsGrid = new Grid();
        private readonly TextBox _notes = new TextBox { Height = 56, AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        private readonly TextBlock _lblPeriod = new TextBlock { Margin = new Thickness(0, 0, 0, 10) };
        private readonly TextBlock _lblTotals = new TextBlock { FontWeight = FontWeights.Bold, Margin = new Thickness(0, 8, 0, 0) };

        public CashCountView(Func<ICashCountView, CashCountPresenter> presenterFactory)
        {
            _presenter = presenterFactory(this);

            var root = new StackPanel { Margin = new Thickness(24), Width = 460, HorizontalAlignment = HorizontalAlignment.Left };
            root.Children.Add(new TextBlock { Text = "Arqueo de caja", FontSize = 22, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 16) });
            root.Children.Add(_lblPeriod);

            var header = new Grid();
            foreach (var w in new[] { 2.0, 1.0, 1.0, 1.0 }) header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(w, GridUnitType.Star) });
            AddCell(header, 0, 0, "Forma de pago", true);
            AddCell(header, 1, 0, "Esperado", true);
            AddCell(header, 2, 0, "Contado", true);
            AddCell(header, 3, 0, "Diferencia", true);
            root.Children.Add(header);

            foreach (var w in new[] { 2.0, 1.0, 1.0, 1.0 }) _rowsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(w, GridUnitType.Star) });
            root.Children.Add(_rowsGrid);

            root.Children.Add(_lblTotals);
            root.Children.Add(new TextBlock { Text = "Observaciones:", Margin = new Thickness(0, 10, 0, 2) });
            root.Children.Add(_notes);

            var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 12, 0, 0) };
            var register = new Button { Content = "Registrar arqueo", Margin = new Thickness(0, 0, 8, 0) };
            register.Click += (s, e) => _presenter.OnRegister();
            buttons.Children.Add(register);
            root.Children.Add(buttons);

            Content = root;

            Loaded += (s, e) => _presenter.OnLoad();
        }

        // The hosting window, for owning message boxes.
        private Window Host => Window.GetWindow(this)!;

        private static void AddCell(Grid grid, int col, int row, string text, bool bold = false)
        {
            var tb = new TextBlock { Text = text, Margin = new Thickness(2), FontWeight = bold ? FontWeights.Bold : FontWeights.Normal };
            Grid.SetColumn(tb, col);
            Grid.SetRow(tb, row);
            grid.Children.Add(tb);
        }

        #region ICashCountView

        public string Notes => _notes.Text;

        public void ShowPeriod(DateTime start, DateTime end) =>
            _lblPeriod.Text = $"Período: {start:dd/MM/yyyy HH:mm}  —  {end:dd/MM/yyyy HH:mm}";

        public void ShowLines(IReadOnlyList<CashCountRow> lines)
        {
            _rowsGrid.Children.Clear();
            _rowsGrid.RowDefinitions.Clear();
            _rows.Clear();
            _expected.Clear();

            for (int i = 0; i < lines.Count; i++)
            {
                _rowsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                CashCountRow line = lines[i];
                _expected[line.PaymentMethod] = line.Expected;

                AddCell(_rowsGrid, 0, i, line.PaymentMethod);
                AddCell(_rowsGrid, 1, i, CultureInfoHelper.FormatAsCurrency(line.Expected));

                var counted = new TextBox { Margin = new Thickness(2), HorizontalContentAlignment = HorizontalAlignment.Right };
                counted.TextChanged += (s, e) => _presenter.OnCountedChanged();
                Grid.SetColumn(counted, 2);
                Grid.SetRow(counted, i);
                _rowsGrid.Children.Add(counted);

                var diff = new TextBlock { Margin = new Thickness(2), Text = CultureInfoHelper.FormatAsCurrency(-line.Expected) };
                Grid.SetColumn(diff, 3);
                Grid.SetRow(diff, i);
                _rowsGrid.Children.Add(diff);

                _rows.Add(new RowCtl { Method = line.PaymentMethod, Counted = counted, Diff = diff });
            }
        }

        public string GetCountedText(string paymentMethod) =>
            _rows.FirstOrDefault(r => string.Equals(r.Method, paymentMethod, StringComparison.OrdinalIgnoreCase))?.Counted.Text ?? "";

        public void ShowTotals(decimal expected, decimal counted, decimal difference)
        {
            _lblTotals.Text = $"Esperado: {CultureInfoHelper.FormatAsCurrency(expected)}   ·   " +
                              $"Contado: {CultureInfoHelper.FormatAsCurrency(counted)}   ·   " +
                              $"Diferencia: {CultureInfoHelper.FormatAsCurrency(difference)}";
            _lblTotals.Foreground = difference == 0m ? Brushes.SeaGreen : (difference < 0m ? Brushes.Firebrick : Brushes.DarkGoldenrod);

            foreach (RowCtl row in _rows)
            {
                decimal exp = GetExpectedFor(row.Method);
                decimal cnt = ParseAmount(row.Counted.Text);
                decimal d = cnt - exp;
                row.Diff.Text = CultureInfoHelper.FormatAsCurrency(d);
                row.Diff.Foreground = d == 0m ? Brushes.SeaGreen : (d < 0m ? Brushes.Firebrick : Brushes.DarkGoldenrod);
            }
        }

        public void ShowMessage(string message) =>
            MessageBox.Show(Host, message, "Arqueo de caja", MessageBoxButton.OK, MessageBoxImage.Information);

        // Hosted inline: no dialog to close. Reload so the screen reflects the just-registered
        // state (fresh period, expected totals, cleared counted fields).
        public void CountRegistered() => _presenter.OnLoad();

        #endregion

        // The expected amount is only handed in via ShowLines; cache it per method for the
        // per-row difference refresh without re-parsing a formatted currency label.
        private readonly Dictionary<string, decimal> _expected = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        private decimal GetExpectedFor(string method) => _expected.TryGetValue(method, out decimal v) ? v : 0m;

        private static decimal ParseAmount(string text)
        {
            text = (text ?? "").Trim();
            return decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal a)
                   || decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out a)
                ? a : 0m;
        }
    }
}
