using System;
using System.Collections.Generic;

namespace PharmacySystem.Presentation
{
    // Passive view for the "arqueo de caja" dialog. The presenter owns the period, the expected
    // totals and the decision to save; the view only renders and reads plain values.
    public interface ICashCountView
    {
        // The period the count covers - shown read-only.
        void ShowPeriod(DateTime start, DateTime end);

        // Renders one row per payment method with its expected amount. Called once on load.
        void ShowLines(IReadOnlyList<CashCountRow> lines);

        // The counted amount the user typed for a method, as raw text ("" when blank). The
        // presenter parses it, so an unparseable value is a validation error, not a view concern.
        string GetCountedText(string paymentMethod);

        string Notes { get; }

        // Expected / counted / difference totals across all methods, recomputed on demand.
        void ShowTotals(decimal expected, decimal counted, decimal difference);

        void ShowMessage(string message);

        // The count was saved: the dialog can close.
        void CountRegistered();
    }
}
