using System;
using System.Drawing;
using System.Windows.Forms;
using PharmacySystem.Business;
using PharmacySystem.Data;
using PharmacySystem.Helpers;
using PharmacySystem.Model;

namespace PharmacySystem
{
    // Read-only list of a product's lots: units on hand, expiry and purchase cost per batch.
    // Pure display, no presenter (same as ModalAlerts / PrintSale) - it builds its own service
    // from the shared connection factory.
    public class ModalProductLots : Form
    {
        public ModalProductLots(int productId, string productName)
        {
            var service = new ProductService(new ProductRepository(CompositionRoot.ConnectionFactory));

            Text = "Lotes de " + productName;
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9F);
            ClientSize = new Size(560, 340);

            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            grid.Columns.Add("Cantidad", "Cantidad");
            grid.Columns.Add("Vencimiento", "Vencimiento");
            grid.Columns.Add("CostoUnitario", "Costo unitario");
            grid.Columns.Add("Valor", "Valor (cantidad x costo)");

            var footer = new Label { Dock = DockStyle.Bottom, Height = 26, TextAlign = ContentAlignment.MiddleRight, Padding = new Padding(0, 0, 12, 0) };

            Controls.Add(grid);
            Controls.Add(footer);

            decimal totalValue = 0m;
            int totalUnits = 0;
            foreach (ProductLot lot in service.GetLots(productId))
            {
                decimal cost = lot.unitCost ?? 0m;
                decimal value = lot.quantity * cost;
                totalValue += value;
                totalUnits += lot.quantity;

                int row = grid.Rows.Add();
                grid.Rows[row].Cells["Cantidad"].Value = lot.quantity;
                grid.Rows[row].Cells["Vencimiento"].Value = lot.dateExpired.HasValue ? lot.dateExpired.Value.ToShortDateString() : "sin fecha";
                grid.Rows[row].Cells["CostoUnitario"].Value = CultureInfoHelper.FormatAsCurrency(cost);
                grid.Rows[row].Cells["Valor"].Value = CultureInfoHelper.FormatAsCurrency(value);
            }

            footer.Text = grid.Rows.Count == 0
                ? "Este producto no tiene lotes con stock."
                : $"{grid.Rows.Count} lote(s)  ·  {totalUnits} u.  ·  valor {CultureInfoHelper.FormatAsCurrency(totalValue)}";
        }
    }
}
