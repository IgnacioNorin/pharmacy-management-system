using System;
using System.Collections.Generic;

namespace PharmacySystem.Model
{
    // A cash-drawer reconciliation ("arqueo de caja"): at the end of a shift the cashier counts
    // what is physically in the drawer and the system shows what it expected, per payment method,
    // for the period since the previous reconciliation. The difference (counted - expected) is
    // recorded for audit; sales are never modified.
    public class CashCount
    {
        public int id { get; set; }
        public DateTime periodStart { get; set; }
        public DateTime periodEnd { get; set; }
        public int? userId { get; set; }
        public string userName { get; set; }
        public string notes { get; set; }
        public DateTime createdAt { get; set; }

        public List<CashCountLine> lines { get; set; } = new List<CashCountLine>();

        public decimal ExpectedTotal
        {
            get { decimal t = 0; foreach (var l in lines) t += l.expectedAmount; return t; }
        }

        public decimal CountedTotal
        {
            get { decimal t = 0; foreach (var l in lines) t += l.countedAmount; return t; }
        }

        public decimal Difference => CountedTotal - ExpectedTotal;
    }

    public class CashCountLine
    {
        public string paymentMethod { get; set; }
        public decimal expectedAmount { get; set; }
        public decimal countedAmount { get; set; }

        public decimal Difference => countedAmount - expectedAmount;
    }
}
