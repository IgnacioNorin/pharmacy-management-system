using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ClosedXML.Excel;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using Xunit;

namespace PharmacySystem.Tests.Presentation
{
    public class ReportExporterTests
    {
        public class Row
        {
            public string Name { get; set; }
            public decimal Amount { get; set; }
            public int Quantity { get; set; }
            public DateTime When { get; set; }
        }

        private static ReportDefinition<Row> Definition() => new ReportDefinition<Row>(new[]
        {
            new ReportColumn<Row>("Nombre", ReportValueType.Text, r => r.Name),
            new ReportColumn<Row>("Monto", ReportValueType.Currency, r => r.Amount),
            new ReportColumn<Row>("Cantidad", ReportValueType.Integer, r => r.Quantity),
            new ReportColumn<Row>("Fecha", ReportValueType.Date, r => r.When)
        });

        private static ReportResult<Row> WithTotals() => new ReportResult<Row>(
            new List<Row>
            {
                new Row { Name = "Uno", Amount = 10.5m, Quantity = 3, When = new DateTime(2026, 3, 17) },
                new Row { Name = "Dos", Amount = 4.25m, Quantity = 2, When = new DateTime(2026, 3, 18) }
            },
            new Row { Amount = 14.75m, Quantity = 5 });

        private static byte[] Run(IReportExporter exporter, ReportResult<Row> result)
        {
            using (var stream = new MemoryStream())
            {
                exporter.Export(Definition(), result, "Prueba", stream);
                return stream.ToArray();
            }
        }

        // ---- CSV ----------------------------------------------------------------

        [Fact]
        public void Csv_StartsWithUtf8Bom()
        {
            byte[] bytes = Run(new CsvReportExporter(), WithTotals());
            Assert.True(bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF);
        }

        [Fact]
        public void Csv_UsesTheCurrentCultureListSeparatorAndWritesHeaderThenRowsThenTotals()
        {
            string sep = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
            string[] lines = CsvLines(Run(new CsvReportExporter(), WithTotals()));

            Assert.Equal("Nombre" + sep + "Monto" + sep + "Cantidad" + sep + "Fecha", lines[0]);
            Assert.Equal("Uno" + sep + 10.5m.ToString("0.00", CultureInfo.CurrentCulture) + sep + "3" + sep + "2026-03-17", lines[1]);

            string totals = lines.Last(l => l.Length > 0);
            Assert.StartsWith("Total:" + sep, totals);
            Assert.Contains(14.75m.ToString("0.00", CultureInfo.CurrentCulture), totals);
        }

        [Fact]
        public void Csv_QuotesFieldsThatContainTheSeparator()
        {
            string sep = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
            var result = new ReportResult<Row>(new List<Row>
            {
                new Row { Name = "a" + sep + "b", Amount = 1m, Quantity = 1, When = new DateTime(2026, 1, 1) }
            });

            string[] lines = CsvLines(Run(new CsvReportExporter(), result));

            Assert.StartsWith("\"a" + sep + "b\"" + sep, lines[1]);
        }

        [Fact]
        public void Csv_WithoutTotals_HasNoTotalLine()
        {
            var result = new ReportResult<Row>(new List<Row>
            {
                new Row { Name = "Uno", Amount = 1m, Quantity = 1, When = new DateTime(2026, 1, 1) }
            });

            string[] lines = CsvLines(Run(new CsvReportExporter(), result));

            Assert.DoesNotContain(lines, l => l.StartsWith("Total:"));
        }

        // ---- XLSX ---------------------------------------------------------------

        [Fact]
        public void Xlsx_WritesTypedCells_HeaderNumbersDatesAndABoldTotalsRow()
        {
            byte[] bytes = Run(new XlsxReportExporter(), WithTotals());

            using (var wb = new XLWorkbook(new MemoryStream(bytes)))
            {
                IXLWorksheet ws = wb.Worksheet(1);

                Assert.Equal("Nombre", ws.Cell(1, 1).GetString());

                Assert.Equal(XLDataType.Number, ws.Cell(2, 2).DataType);
                Assert.Equal(10.5, ws.Cell(2, 2).GetDouble(), 3);

                Assert.Equal(XLDataType.Number, ws.Cell(2, 3).DataType);
                Assert.Equal(3, (int)ws.Cell(2, 3).GetDouble());

                Assert.Equal(XLDataType.DateTime, ws.Cell(2, 4).DataType);
                Assert.Equal(new DateTime(2026, 3, 17), ws.Cell(2, 4).GetDateTime());

                IXLCell totalLabel = ws.Cell(4, 1);
                Assert.Equal("Total:", totalLabel.GetString());
                Assert.True(totalLabel.Style.Font.Bold);
                Assert.Equal(14.75, ws.Cell(4, 2).GetDouble(), 3);
            }
        }

        [Fact]
        public void Xlsx_EmptyResult_StillWritesTheHeader()
        {
            byte[] bytes = Run(new XlsxReportExporter(), new ReportResult<Row>(new List<Row>()));

            using (var wb = new XLWorkbook(new MemoryStream(bytes)))
            {
                Assert.Equal("Cantidad", wb.Worksheet(1).Cell(1, 3).GetString());
            }
        }

        // ---- PDF ---------------------------------------------------------------

        [Theory]
        [MemberData(nameof(PdfCases))]
        public void Pdf_ProducesValidPdfBytes(ReportResult<Row> result)
        {
            byte[] bytes = Run(new PdfReportExporter(), result);

            Assert.True(bytes.Length > 100);
            Assert.Equal("%PDF-", Encoding.ASCII.GetString(bytes, 0, 5));
        }

        public static IEnumerable<object[]> PdfCases()
        {
            yield return new object[] { new ReportResult<Row>(new List<Row>()) };
            yield return new object[]
            {
                new ReportResult<Row>(new List<Row>
                {
                    new Row { Name = "Uno", Amount = 1m, Quantity = 1, When = new DateTime(2026, 1, 1) }
                })
            };
            yield return new object[] { WithTotals() };
        }

        // ---------------------------------------------------------------------

        private static string[] CsvLines(byte[] bytes)
        {
            using (var reader = new StreamReader(new MemoryStream(bytes), Encoding.UTF8, true))
            {
                return reader.ReadToEnd().Replace("\r\n", "\n").Split('\n');
            }
        }
    }
}
