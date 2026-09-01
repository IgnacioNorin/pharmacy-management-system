using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using PharmacySystem.Helpers;
using PharmacySystem.Model;

namespace PharmacySystem.Ui
{
    // WPF port of ModalProductLots. Read-only list of a product's lots: units on hand, expiry and
    // purchase cost per batch. Pure display - the lots are handed in by the caller (the WinForms
    // shell resolves them from the shared ProductService).
    public class ProductLotsWindow : Wpf.Ui.Controls.FluentWindow
    {
        private sealed class LotRowVm
        {
            public string Cantidad { get; set; } = string.Empty;
            public string Vencimiento { get; set; } = string.Empty;
            public string CostoUnitario { get; set; } = string.Empty;
            public string Valor { get; set; } = string.Empty;
        }

        public ProductLotsWindow(string productName, IReadOnlyList<ProductLot> lots)
        {
            Title = "Lotes de " + productName;
            Width = 560;
            Height = 360;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ShowInTaskbar = false;
            Background = System.Windows.Media.Brushes.White;
            FontFamily = new System.Windows.Media.FontFamily("Segoe UI");
            FontSize = 13;

            var grid = new DataGrid
            {
                AutoGenerateColumns = false,
                IsReadOnly = true,
                CanUserAddRows = false,
                CanUserDeleteRows = false,
                HeadersVisibility = DataGridHeadersVisibility.Column,
                GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
                RowHeaderWidth = 0
            };
            grid.Columns.Add(new DataGridTextColumn { Header = "Cantidad", Width = 90, Binding = new System.Windows.Data.Binding("Cantidad") });
            grid.Columns.Add(new DataGridTextColumn { Header = "Vencimiento", Width = 130, Binding = new System.Windows.Data.Binding("Vencimiento") });
            grid.Columns.Add(new DataGridTextColumn { Header = "Costo unitario", Width = 130, Binding = new System.Windows.Data.Binding("CostoUnitario") });
            grid.Columns.Add(new DataGridTextColumn { Header = "Valor (cantidad x costo)", Width = new DataGridLength(1, DataGridLengthUnitType.Star), Binding = new System.Windows.Data.Binding("Valor") });

            var footer = new TextBlock
            {
                Margin = new Thickness(0, 6, 12, 8),
                HorizontalAlignment = HorizontalAlignment.Right,
                FontWeight = FontWeights.Bold
            };

            var rows = new List<LotRowVm>();
            decimal totalValue = 0m;
            int totalUnits = 0;
            foreach (ProductLot lot in lots ?? new List<ProductLot>())
            {
                decimal cost = lot.unitCost ?? 0m;
                decimal value = lot.quantity * cost;
                totalValue += value;
                totalUnits += lot.quantity;

                rows.Add(new LotRowVm
                {
                    Cantidad = lot.quantity.ToString(),
                    Vencimiento = lot.dateExpired.HasValue ? lot.dateExpired.Value.ToShortDateString() : "sin fecha",
                    CostoUnitario = CultureInfoHelper.FormatAsCurrency(cost),
                    Valor = CultureInfoHelper.FormatAsCurrency(value)
                });
            }

            grid.ItemsSource = rows;
            footer.Text = rows.Count == 0
                ? "Este producto no tiene lotes con stock."
                : $"{rows.Count} lote(s)  ·  {totalUnits} u.  ·  valor {CultureInfoHelper.FormatAsCurrency(totalValue)}";

            var root = new DockPanel { Margin = new Thickness(10) };
            DockPanel.SetDock(footer, Dock.Bottom);
            root.Children.Add(footer);
            root.Children.Add(grid);
            Content = root;
        }
    }
}
