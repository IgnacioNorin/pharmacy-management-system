using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using PharmacySystem.Helpers;
using PharmacySystem.Presentation;

namespace PharmacySystem
{
    // "Pago mixto" dialog: one amount field per payment method; the amounts must sum to the sale
    // total. Built in code (no Designer file). Result is the non-zero entries, or null if the
    // cashier cancels.
    public class ModalSalePayments : Form
    {
        public IReadOnlyList<SalePaymentEntry> Result { get; private set; }

        private readonly decimal _total;
        private readonly Dictionary<string, TextBox> _amountByMethod = new Dictionary<string, TextBox>(StringComparer.OrdinalIgnoreCase);
        private readonly Label _lblSummary = new Label();

        public ModalSalePayments(decimal total, IReadOnlyList<SalePaymentEntry> current, IReadOnlyList<string> methods)
        {
            _total = total;

            Text = "Pago mixto";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9F);
            ClientSize = new Size(360, 80 + methods.Count * 34 + 90);

            var lblTotal = new Label
            {
                Location = new Point(16, 14),
                AutoSize = true,
                Font = new Font(Font, FontStyle.Bold),
                Text = "Total de la venta: " + CultureInfoHelper.FormatAsCurrency(total)
            };
            Controls.Add(lblTotal);

            int y = 48;
            foreach (string method in methods)
            {
                Controls.Add(new Label { Location = new Point(16, y + 3), AutoSize = true, Text = method });

                var box = new TextBox { Location = new Point(180, y), Size = new Size(150, 21), TextAlign = HorizontalAlignment.Right };
                decimal existing = current?.FirstOrDefault(e => string.Equals(e.Method, method, StringComparison.OrdinalIgnoreCase))?.Amount ?? 0m;
                if (existing > 0m)
                {
                    box.Text = existing.ToString("0.##", CultureInfo.InvariantCulture);
                }
                box.TextChanged += (s, e) => RefreshSummary();
                _amountByMethod[method] = box;
                Controls.Add(box);

                y += 34;
            }

            _lblSummary.Location = new Point(16, y + 6);
            _lblSummary.Size = new Size(328, 20);
            Controls.Add(_lblSummary);

            var btnAccept = new Button { Text = "Aceptar", Location = new Point(150, y + 34), Size = new Size(90, 28), DialogResult = DialogResult.None };
            btnAccept.Click += (s, e) => OnAccept();
            var btnCancel = new Button { Text = "Cancelar", Location = new Point(250, y + 34), Size = new Size(90, 28), DialogResult = DialogResult.Cancel };
            Controls.Add(btnAccept);
            Controls.Add(btnCancel);
            CancelButton = btnCancel;

            RefreshSummary();
        }

        private void RefreshSummary()
        {
            decimal assigned = _amountByMethod.Values.Sum(b => ParseAmount(b.Text));
            decimal remaining = _total - assigned;
            _lblSummary.Text = $"Asignado: {CultureInfoHelper.FormatAsCurrency(assigned)}   ·   Falta: {CultureInfoHelper.FormatAsCurrency(remaining)}";
            _lblSummary.ForeColor = remaining == 0m ? Color.SeaGreen : Color.Firebrick;
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
                    MessageBox.Show($"El monto de {pair.Key} no es válido.", "Pago mixto", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (amount > 0m)
                {
                    entries.Add(new SalePaymentEntry(pair.Key, amount));
                }
            }

            if (entries.Count == 0)
            {
                MessageBox.Show("Ingrese al menos un monto.", "Pago mixto", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal sum = entries.Sum(e => e.Amount);
            if (Math.Abs(sum - _total) > 0.005m)
            {
                MessageBox.Show(
                    "La suma de los pagos debe ser igual al total de la venta (" + CultureInfoHelper.FormatAsCurrency(_total) + ").",
                    "Pago mixto", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Result = entries;
            DialogResult = DialogResult.OK;
            Close();
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
