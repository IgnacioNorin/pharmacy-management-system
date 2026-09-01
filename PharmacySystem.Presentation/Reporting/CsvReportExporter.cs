using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using PharmacySystem.Helpers;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // Plain, re-importable CSV: the list separator of the current culture, UTF-8 with BOM (so
    // Excel on a Spanish locale keeps the accents), RFC 4180 quoting. Numbers are written plain
    // (no currency symbol, no thousands separator) and dates as yyyy-MM-dd so the file can be
    // parsed back or analysed elsewhere.
    public class CsvReportExporter : IReportExporter
    {
        public ReportExportFormat Format => ReportExportFormat.Csv;
        public string Extension => "csv";
        public string FilterLabel => "CSV (*.csv)";

        public void Export<TRow>(ReportDefinition<TRow> definition, ReportResult<TRow> result, string title, Stream target)
        {
            string separator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
            if (string.IsNullOrEmpty(separator))
            {
                separator = ";";
            }

            using (var writer = new StreamWriter(target, new UTF8Encoding(true), 1024, leaveOpen: true))
            {
                writer.WriteLine(string.Join(separator,
                    definition.Columns.Select(c => Escape(c.Header, separator))));

                foreach (TRow row in result.Rows)
                {
                    writer.WriteLine(string.Join(separator,
                        definition.Columns.Select(c => Escape(CellText(c.Value(row), c.Type), separator))));
                }

                if (result.HasTotals)
                {
                    string[] cells = new string[definition.Columns.Count];
                    cells[0] = "Total:";
                    for (int i = 1; i < definition.Columns.Count; i++)
                    {
                        ReportColumn<TRow> column = definition.Columns[i];
                        bool numeric = column.Type == ReportValueType.Currency || column.Type == ReportValueType.Integer;
                        cells[i] = numeric ? CellText(column.Value(result.Totals!), column.Type) : "";
                    }
                    writer.WriteLine(string.Join(separator, cells.Select(v => Escape(v, separator))));
                }

                writer.Flush();
            }
        }

        private static string CellText(object? value, ReportValueType type)
        {
            if (value == null) return "";
            if (value is string s) return s;
            if (value is DateTime d) return d.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);

            switch (type)
            {
                // CLP has no minor unit: currency amounts are whole pesos.
                case ReportValueType.Currency: return CultureInfoHelper.RoundMoney(Convert.ToDecimal(value)).ToString("0", CultureInfo.CurrentCulture);
                case ReportValueType.Integer: return Convert.ToInt64(value).ToString(CultureInfo.CurrentCulture);
                default: return value.ToString() ?? "";
            }
        }

        private static string Escape(string field, string separator)
        {
            if (string.IsNullOrEmpty(field)) return "";

            bool needsQuotes = field.Contains("\"") || field.Contains("\n") || field.Contains("\r") || field.Contains(separator);
            return needsQuotes ? "\"" + field.Replace("\"", "\"\"") + "\"" : field;
        }
    }
}
