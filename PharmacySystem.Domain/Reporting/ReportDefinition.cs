using System;
using System.Collections.Generic;
using System.Linq;

namespace PharmacySystem.Model
{
    // How a column's raw value should be rendered by whoever consumes the definition
    // (the grid, a CSV writer, an XLSX writer). Keeps formatting out of the presenter:
    // it emits typed values, the consumer decides how they look.
    public enum ReportValueType
    {
        Text,
        Currency,
        Integer,
        Date
    }

    // One column of a report: a header, the value type, and a selector that pulls the
    // raw (unformatted) value out of a row.
    public class ReportColumn<TRow>
    {
        public string Header { get; }
        public ReportValueType Type { get; }
        public Func<TRow, object> Value { get; }

        public ReportColumn(string header, ReportValueType type, Func<TRow, object> value)
        {
            Header = header;
            Type = type;
            Value = value;
        }
    }

    // The shape of a report: an ordered list of columns. One definition per report type,
    // shared by the grid and every exporter so a column is declared exactly once.
    public class ReportDefinition<TRow>
    {
        public IReadOnlyList<ReportColumn<TRow>> Columns { get; }

        public ReportDefinition(IEnumerable<ReportColumn<TRow>> columns)
        {
            Columns = columns.ToList();
        }
    }

    // The data of a report: the rows, plus an optional totals row of the same shape.
    // Totals is a TRow with only its numeric fields populated; text/date fields stay
    // at their defaults and the consumer renders a spreadsheet-style totals line.
    public class ReportResult<TRow>
    {
        public IReadOnlyList<TRow> Rows { get; }
        public TRow? Totals { get; }
        public bool HasTotals { get; }

        public ReportResult(IReadOnlyList<TRow> rows)
        {
            Rows = rows ?? new List<TRow>();
            Totals = default;
            HasTotals = false;
        }

        public ReportResult(IReadOnlyList<TRow> rows, TRow totals)
        {
            Rows = rows ?? new List<TRow>();
            Totals = totals;
            HasTotals = true;
        }
    }
}
