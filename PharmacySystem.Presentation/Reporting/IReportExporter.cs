using System.IO;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    public enum ReportExportFormat
    {
        Csv,
        Xlsx,
        Pdf
    }

    // One exporter per file format. All of them walk the same ReportDefinition / ReportResult
    // the grid uses, so a report is described once and every format stays in sync.
    public interface IReportExporter
    {
        ReportExportFormat Format { get; }

        // Lowercase, no dot ("csv" / "xlsx" / "pdf"). Used to match a chosen file name back to
        // its exporter.
        string Extension { get; }

        // SaveFileDialog filter label, e.g. "Excel (*.xlsx)".
        string FilterLabel { get; }

        // Writes the report to target. The caller owns the stream (it is not closed here).
        void Export<TRow>(ReportDefinition<TRow> definition, ReportResult<TRow> result, string title, Stream target);
    }
}
