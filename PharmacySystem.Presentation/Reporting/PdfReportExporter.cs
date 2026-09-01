using System;
using System.Globalization;
using System.IO;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Fonts;
using PharmacySystem.Helpers;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // A4 landscape sheet: title, generation timestamp, a bordered table with a shaded header
    // row and a bold totals row, page numbers in the footer. Values are shown the way the grid
    // shows them (currency with the configured symbol, dates in presentation format).
    public class PdfReportExporter : IReportExporter
    {
        // PDFsharp 6.x does not read system fonts by default. The app is Windows-only and the
        // report uses Arial / Courier New, so let it pull those from C:\Windows\Fonts. The flag
        // is a safe no-op on non-Windows.
        static PdfReportExporter()
        {
            GlobalFontSettings.UseWindowsFontsUnderWindows = true;
        }

        public ReportExportFormat Format => ReportExportFormat.Pdf;
        public string Extension => "pdf";
        public string FilterLabel => "PDF (*.pdf)";

        public void Export<TRow>(ReportDefinition<TRow> definition, ReportResult<TRow> result, string title, Stream target)
        {
            int columnCount = definition.Columns.Count;

            var document = new Document();

            Style normal = document.Styles["Normal"]!;
            normal.Font.Name = "Arial";
            normal.Font.Size = 7;

            Section section = document.AddSection();

            // MigraDoc 6.x freezes Document.DefaultPageSetup - page settings go on the section's
            // own PageSetup (which starts as a clone of the default).
            PageSetup page = section.PageSetup;
            page.Orientation = Orientation.Landscape;
            page.PageFormat = PageFormat.A4;
            page.TopMargin = Unit.FromCentimeter(1.5);
            page.BottomMargin = Unit.FromCentimeter(1.5);
            page.LeftMargin = Unit.FromCentimeter(1.2);
            page.RightMargin = Unit.FromCentimeter(1.2);

            Paragraph heading = section.AddParagraph(string.IsNullOrWhiteSpace(title) ? "Reporte" : title);
            heading.Format.Font.Size = 13;
            heading.Format.Font.Bold = true;
            heading.Format.SpaceAfter = Unit.FromPoint(4);

            Paragraph meta = section.AddParagraph("Generado: " + DateTime.Now.ToString("dd-MM-yyyy HH:mm", CultureInfo.CurrentCulture));
            meta.Format.Font.Size = 7;
            meta.Format.Font.Color = Colors.Gray;
            meta.Format.SpaceAfter = Unit.FromPoint(10);

            Table table = section.AddTable();
            table.Borders.Width = 0.25;
            table.Borders.Color = Colors.Gray;

            double usableCm = 29.7 - page.LeftMargin.Centimeter - page.RightMargin.Centimeter;
            Unit columnWidth = Unit.FromCentimeter(usableCm / columnCount);
            for (int c = 0; c < columnCount; c++)
            {
                table.AddColumn(columnWidth);
            }

            Row headerRow = table.AddRow();
            headerRow.Shading.Color = Colors.LightGray;
            headerRow.Format.Font.Bold = true;
            for (int c = 0; c < columnCount; c++)
            {
                headerRow.Cells[c].AddParagraph(definition.Columns[c].Header);
            }

            foreach (TRow item in result.Rows)
            {
                Row dataRow = table.AddRow();
                for (int c = 0; c < columnCount; c++)
                {
                    ReportColumn<TRow> column = definition.Columns[c];
                    Paragraph cell = dataRow.Cells[c].AddParagraph(DisplayValue(column.Value(item), column.Type));
                    if (IsNumeric(column.Type))
                    {
                        cell.Format.Alignment = ParagraphAlignment.Right;
                    }
                }
            }

            if (result.HasTotals)
            {
                Row totalsRow = table.AddRow();
                totalsRow.Format.Font.Bold = true;
                totalsRow.Borders.Top.Width = 0.75;
                totalsRow.Cells[0].AddParagraph("Total:");
                for (int c = 1; c < columnCount; c++)
                {
                    ReportColumn<TRow> column = definition.Columns[c];
                    if (IsNumeric(column.Type))
                    {
                        Paragraph cell = totalsRow.Cells[c].AddParagraph(DisplayValue(column.Value(result.Totals!), column.Type));
                        cell.Format.Alignment = ParagraphAlignment.Right;
                    }
                }
            }

            Paragraph footer = section.Footers.Primary.AddParagraph();
            footer.Format.Alignment = ParagraphAlignment.Center;
            footer.Format.Font.Size = 7;
            footer.AddText("Página ");
            footer.AddPageField();
            footer.AddText(" de ");
            footer.AddNumPagesField();

            var renderer = new PdfDocumentRenderer { Document = document };
            renderer.RenderDocument();
            renderer.PdfDocument.Save(target, false);
        }

        private static bool IsNumeric(ReportValueType type) =>
            type == ReportValueType.Currency || type == ReportValueType.Integer;

        private static string DisplayValue(object? value, ReportValueType type)
        {
            if (value == null) return "";
            if (value is string s) return s;
            if (value is DateTime d) return DateHelper.FormatDatePresentation(d);

            switch (type)
            {
                case ReportValueType.Currency: return CultureInfoHelper.FormatAsCurrency(Convert.ToDecimal(value));
                case ReportValueType.Integer: return Convert.ToInt64(value).ToString(CultureInfo.CurrentCulture);
                default: return value.ToString() ?? "";
            }
        }
    }
}
