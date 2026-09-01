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
    // WPF port of ModalSalePayments. "Pago mixto": one amount box per method; the amounts must
    // sum to the sale total. Built entirely in code (no XAML). Result is the non-zero entries,
    // or null if cancelled.
    public class SalePaymentsWindow : Wpf.Ui.Controls.FluentWindow
    {
        public IReadOnlyList<SalePaymentEntry>? Result { get; private set; }

        private readonly decimal _total;
        private readonly Dictionary<string, TextBox> _amountByMethod =
            new Dictionary<string, TextBox>(StringComparer.OrdinalIgnoreCase);
        private readonly TextBlock _summary = new TextBlock { Margin = new Thickness(0, 8, 0, 0) };

        public SalePaymentsWindow(decimal total, IReadOnlyList<SalePaymentEntry>? current, IReadOnlyList<string> methods)
        {
            _total = total;

            Title = "Pago mixto";
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;
            SizeToContent = SizeToContent.WidthAndHeight;
            FontFamily = new FontFamily("Segoe UI");
            FontSize = 13;

            var root = new StackPanel { Margin = new Thickness(16) };

            root.Children.Add(new TextBlock
            {
                Text = "Total de la venta: " + CultureInfoHelper.FormatAsCurrency(total),
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 12)
            });

            foreach (string method in methods)
            {
                var row = new DockPanel { Margin = new Thickness(0, 3, 0, 3), LastChildFill = true };
                row.Children.Add(new TextBlock { Text = method, Width = 150, VerticalAlignment = VerticalAlignment.Center });

                decimal existing = current?.FirstOrDefault(e => string.Equals(e.Method, method, StringComparison.OrdinalIgnoreCase))?.Amount ?? 0m;
                var box = new TextBox
                {
                    Width = 160,
                    HorizontalContentAlignment = HorizontalAlignment.Right,
                    Text = existing > 0m ? existing.ToString("0.##", CultureInfo.InvariantCulture) : ""
                };
                box.TextChanged += (s, e) => RefreshSummary();
                _amountByMethod[method] = box;
                row.Children.Add(box);
                root.Children.Add(row);
            }

            root.Children.Add(_summary);

            var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 14, 0, 0) };
            var accept = new Button { Content = "Aceptar", Width = 90, Height = 28, Margin = new Thickness(0, 0, 8, 0), IsDefault = true };
            accept.Click += (s, e) => OnAccept();
            var cancel = new Button { Content = "Cancelar", Width = 90, Height = 28, IsCancel = true };
            buttons.Children.Add(accept);
            buttons.Children.Add(cancel);
            root.Children.Add(buttons);

            Content = root;
            RefreshSummary();
        }

        private void RefreshSummary()
        {
            decimal assigned = _amountByMethod.Values.Sum(b => ParseAmount(b.Text));
            decimal remaining = _total - assigned;
            _summary.Text = $"Asignado: {CultureInfoHelper.FormatAsCurrency(assigned)}   ·   Falta: {CultureInfoHelper.FormatAsCurrency(remaining)}";
            _summary.Foreground = remaining == 0m ? Brushes.SeaGreen : Brushes.Firebrick;
        }

        private void OnAccept()
        {
            var entries = new List<SalePaymentEntry>();
            foreach (KeyValuePair<string, TextBox> pair in _amountByMethod)
            {
                string text = pair.Value.Text.Trim();
                if (text.Length == 0)
                {
                    continue;
                }

                if (!TryParse(text, out decimal amount) || amount < 0m)
                {
                    MessageBox.Show(this, $"El monto de {pair.Key} no es válido.", "Pago mixto", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (amount > 0m)
                {
                    entries.Add(new SalePaymentEntry(pair.Key, amount));
                }
            }

            if (entries.Count == 0)
            {
                MessageBox.Show(this, "Ingrese al menos un monto.", "Pago mixto", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Math.Abs(entries.Sum(e => e.Amount) - _total) > 0.005m)
            {
                MessageBox.Show(this,
                    "La suma de los pagos debe ser igual al total de la venta (" + CultureInfoHelper.FormatAsCurrency(_total) + ").",
                    "Pago mixto", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Result = entries;
            DialogResult = true;
        }

        private static decimal ParseAmount(string text) => TryParse(text, out decimal a) ? a : 0m;

        private static bool TryParse(string text, out decimal amount)
        {
            text = (text ?? "").Trim();
            return decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out amount)
                   || decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out amount);
        }
    }
}
