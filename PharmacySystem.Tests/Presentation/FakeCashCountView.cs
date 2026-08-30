using System;
using System.Collections.Generic;
using PharmacySystem.Presentation;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakeCashCountView : ICashCountView
    {
        // Counted text keyed by payment method - set these before OnRegister / OnCountedChanged.
        public Dictionary<string, string> CountedTexts { get; } = new Dictionary<string, string>();
        public string Notes { get; set; } = "";

        public (DateTime Start, DateTime End)? ShownPeriod { get; private set; }
        public IReadOnlyList<CashCountRow> ShownLines { get; private set; }
        public (decimal Expected, decimal Counted, decimal Difference)? ShownTotals { get; private set; }
        public List<string> ShownMessages { get; } = new List<string>();
        public bool RegisteredCalled { get; private set; }

        public void ShowPeriod(DateTime start, DateTime end) => ShownPeriod = (start, end);
        public void ShowLines(IReadOnlyList<CashCountRow> lines) => ShownLines = lines;
        public string GetCountedText(string paymentMethod) =>
            CountedTexts.TryGetValue(paymentMethod, out string v) ? v : "";
        public void ShowTotals(decimal expected, decimal counted, decimal difference) =>
            ShownTotals = (expected, counted, difference);
        public void ShowMessage(string message) => ShownMessages.Add(message);
        public void CountRegistered() => RegisteredCalled = true;
    }
}
