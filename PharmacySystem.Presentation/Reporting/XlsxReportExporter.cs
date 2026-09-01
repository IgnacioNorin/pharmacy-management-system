using System;
using System.Linq;
using System.IO;
using ClosedXML.Excel;
using PharmacySystem.Helpers;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // Real typed cells: currency and integers as numbers with a display format, dates as dates.
    // The totals row is bold with a top border; the header row is bold, frozen and has an
    // AutoFilter; columns are auto-sized.
    public class XlsxReportExporter : IReportExporter
    {
        public ReportExportFormat Format => ReportExportFormat.Xlsx;
        public string Extension => "xlsx";
        public string FilterLabel => "Excel (*.xlsx)";

        public void Export<TRow>(ReportDefinition<TRow> definition, ReportResult<TRow> result, string title, Stream target)
        {
            int columnCount = definition.Columns.Count;

            using (var workbook = new XLWorkbook())
            {
                IXLWorksheet sheet = workbook.Worksheets.Add(SafeSheetName(title));

                for (int c = 0; c < columnCount; c++)
                {
                    IXLCell cell = sheet.Cell(1, c + 1);
                    cell.Value = definition.Columns[c].Header;
                    cell.Style.Font.Bold = true;
                }

                int row = 2;
                foreach (TRow item in result.Rows)
                {
                    for (int c = 0; c < columnCount; c++)
                    {
                        SetCell(sheet.Cell(row, c + 1), definition.Columns[c].Value(item), definition.Columns[c].Type);
                    }
                    row++;
                }

                if (result.HasTotals)
                {
                    sheet.Cell(row, 1).Value = "Total:";
                    for (int c = 1; c < columnCount; c++)
                    {
                        ReportColumn<TRow> column = definition.Columns[c];
                        if (column.Type == ReportValueType.Currency || column.Type == ReportValueType.Integer)
                        {
                            SetCell(sheet.Cell(row, c + 1), column.Value(result.Totals!), column.Type);
                        }
                    }

                    IXLRange totalsRange = sheet.Range(row, 1, row, columnCount);
                    totalsRange.Style.Font.Bold = true;
                    totalsRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    row++;
                }

                int lastRow = Math.Max(row - 1, 1);
                sheet.Range(1, 1, lastRow, columnCount).SetAutoFilter();
                sheet.SheetView.FreezeRows(1);
                sheet.Columns().AdjustToContents();

                workbook.SaveAs(target);
            }
        }

        private static void SetCell(IXLCell cell, object value, ReportValueType type)
        {
            if (value == null)
            {
                cell.Value = string.Empty;
                return;
            }

            switch (type)
            {
                case ReportValueType.Currency:
                    // CLP has no minor unit: currency amounts are whole pesos.
                    cell.Value = CultureInfoHelper.RoundMoney(Convert.ToDecimal(value));
                    cell.Style.NumberFormat.Format = "$ #,##0";
                    break;
                case ReportValueType.Integer:
                    cell.Value = Convert.ToInt64(value);
                    cell.Style.NumberFormat.Format = "#,##0";
                    break;
                case ReportValueType.Date:
                    if (value is DateTime date)
                    {
                        cell.Value = date;
                        cell.Style.NumberFormat.Format = "dd-mm-yyyy";
                    }
                    else
                    {
                        cell.Value = value.ToString();
                    }
                    break;
                default:
                    cell.Value = value.ToString();
                    break;
            }
        }

        private static string SafeSheetName(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return "Informe";
            }

            char[] invalid = { '\\', '/', '?', '*', '[', ']', ':' };
            string clean = new string(title.Where(ch => Array.IndexOf(invalid, ch) < 0).ToArray());
            if (clean.Length == 0)
            {
                return "Informe";
            }
            return clean.Length > 31 ? clean.Substring(0, 31) : clean;
        }
    }
}
