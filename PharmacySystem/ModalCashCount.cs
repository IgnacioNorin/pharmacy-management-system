using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using PharmacySystem.Helpers;
using PharmacySystem.Presentation;

namespace PharmacySystem
{
    // "Arqueo de caja" dialog. Built entirely in code (no Designer file): one row per payment
    // method with its expected total and an editable "counted" field, a live totals line and a
    // notes box. Every decision (period, expected amounts, whether to save) is the presenter's.
    public class ModalCashCount : Form, ICashCountView
    {
        private readonly CashCountPresenter _presenter;

        private readonly Label _lblPeriod = new Label();
        private readonly Label _lblExpectedTotal = new Label();
        private readonly Label _lblCountedTotal = new Label();
        private readonly Label _lblDifferenceTotal = new Label();
        private readonly TextBox _txtNotes = new TextBox();

        private readonly Dictionary<string, TextBox> _countedByMethod =
            new Dictionary<string, TextBox>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Label> _diffByMethod =
            new Dictionary<string, Label>(StringComparer.OrdinalIgnoreCase);

        private TableLayoutPanel _rows;

        public ModalCashCount()
        {
            BuildLayout();
            _presenter = CompositionRoot.CreateCashCountPresenter(this);
            Load += (s, e) => _presenter.OnLoad();
        }

        private void BuildLayout()
        {
            Text = "Arqueo de caja";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(520, 420);
            Font = new Font("Segoe UI", 9F);

            _lblPeriod.Location = new Point(16, 14);
            _lblPeriod.AutoSize = true;

            var header = new Label
            {
                Location = new Point(16, 44),
                AutoSize = true,
                Font = new Font(Font, FontStyle.Bold),
                Text = "Forma de pago        Esperado          Contado          Diferencia"
            };

            _rows = new TableLayoutPanel
            {
                Location = new Point(16, 68),
                Size = new Size(488, 140),
                ColumnCount = 4,
                AutoSize = false
            };
            _rows.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            _rows.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24));
            _rows.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 23));
            _rows.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 23));

            var totalsCaption = new Label { Location = new Point(16, 216), AutoSize = true, Font = new Font(Font, FontStyle.Bold), Text = "Totales" };
            _lblExpectedTotal.Location = new Point(160, 216);
            _lblExpectedTotal.AutoSize = true;
            _lblCountedTotal.Location = new Point(270, 216);
            _lblCountedTotal.AutoSize = true;
            _lblDifferenceTotal.Location = new Point(380, 216);
            _lblDifferenceTotal.AutoSize = true;
            _lblDifferenceTotal.Font = new Font(Font, FontStyle.Bold);

            var lblNotes = new Label { Location = new Point(16, 246), AutoSize = true, Text = "Observaciones:" };
            _txtNotes.Location = new Point(16, 266);
            _txtNotes.Size = new Size(488, 60);
            _txtNotes.Multiline = true;

            var btnCalc = new Button { Location = new Point(16, 340), Size = new Size(120, 30), Text = "Calcular" };
            btnCalc.Click += (s, e) => _presenter.OnCountedChanged();

            var btnRegister = new Button { Location = new Point(270, 340), Size = new Size(140, 30), Text = "Registrar arqueo" };
            btnRegister.Click += (s, e) => _presenter.OnRegister();

            var btnClose = new Button { Location = new Point(416, 340), Size = new Size(88, 30), Text = "Cerrar", DialogResult = DialogResult.Cancel };
            btnClose.Click += (s, e) => Close();

            Controls.AddRange(new Control[]
            {
                _lblPeriod, header, _rows, totalsCaption,
                _lblExpectedTotal, _lblCountedTotal, _lblDifferenceTotal,
                lblNotes, _txtNotes, btnCalc, btnRegister, btnClose
            });
        }

        #region ICashCountView

        public string Notes => _txtNotes.Text;

        public void ShowPeriod(DateTime start, DateTime end)
        {
            _lblPeriod.Text = $"Período: {start:dd/MM/yyyy HH:mm}  —  {end:dd/MM/yyyy HH:mm}";
        }

        public void ShowLines(IReadOnlyList<CashCountRow> lines)
        {
            _rows.Controls.Clear();
            _rows.RowStyles.Clear();
            _countedByMethod.Clear();
            _diffByMethod.Clear();
            RememberExpected(lines);
            _rows.RowCount = lines.Count;

            for (int i = 0; i < lines.Count; i++)
            {
                CashCountRow line = lines[i];
                _rows.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));

                _rows.Controls.Add(new Label { Text = line.PaymentMethod, AutoSize = true, Anchor = AnchorStyles.Left, TextAlign = ContentAlignment.MiddleLeft }, 0, i);
                _rows.Controls.Add(new Label { Text = CultureInfoHelper.FormatAsCurrency(line.Expected), AutoSize = true, Anchor = AnchorStyles.Left, TextAlign = ContentAlignment.MiddleLeft }, 1, i);

                var counted = new TextBox { Width = 90, Anchor = AnchorStyles.Left };
                counted.TextChanged += (s, e) => _presenter.OnCountedChanged();
                _countedByMethod[line.PaymentMethod] = counted;
                _rows.Controls.Add(counted, 2, i);

                var diff = new Label { Text = CultureInfoHelper.FormatAsCurrency(-line.Expected), AutoSize = true, Anchor = AnchorStyles.Left, TextAlign = ContentAlignment.MiddleLeft };
                _diffByMethod[line.PaymentMethod] = diff;
                _rows.Controls.Add(diff, 3, i);
            }
        }

        public string GetCountedText(string paymentMethod) =>
            _countedByMethod.TryGetValue(paymentMethod, out TextBox box) ? box.Text : "";

        public void ShowTotals(decimal expected, decimal counted, decimal difference)
        {
            _lblExpectedTotal.Text = "Esp: " + CultureInfoHelper.FormatAsCurrency(expected);
            _lblCountedTotal.Text = "Cont: " + CultureInfoHelper.FormatAsCurrency(counted);
            _lblDifferenceTotal.Text = "Dif: " + CultureInfoHelper.FormatAsCurrency(difference);
            _lblDifferenceTotal.ForeColor = DifferenceColor(difference);

            foreach (KeyValuePair<string, Label> entry in _diffByMethod)
            {
                decimal exp = _expectedByMethod.TryGetValue(entry.Key, out decimal e) ? e : 0m;
                decimal cnt = ParseAmount(GetCountedText(entry.Key));
                decimal diff = cnt - exp;
                entry.Value.Text = CultureInfoHelper.FormatAsCurrency(diff);
                entry.Value.ForeColor = DifferenceColor(diff);
            }
        }

        private static Color DifferenceColor(decimal difference) =>
            difference == 0m ? Color.Black : (difference < 0m ? Color.Firebrick : Color.SeaGreen);

        private static decimal ParseAmount(string text)
        {
            if (decimal.TryParse(text, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out decimal a) ||
                decimal.TryParse(text, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.CurrentCulture, out a))
            {
                return a;
            }
            return 0m;
        }

        public void ShowMessage(string message) =>
            MessageBox.Show(message, "Arqueo de caja", MessageBoxButtons.OK, MessageBoxIcon.Information);

        public void CountRegistered()
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        #endregion

        // Per-method expected amount, cached so ShowTotals can refresh the per-row difference
        // without re-parsing a formatted currency label.
        private readonly Dictionary<string, decimal> _expectedByMethod =
            new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        private void RememberExpected(IReadOnlyList<CashCountRow> lines)
        {
            _expectedByMethod.Clear();
            foreach (CashCountRow line in lines)
            {
                _expectedByMethod[line.PaymentMethod] = line.Expected;
            }
        }
    }
}
