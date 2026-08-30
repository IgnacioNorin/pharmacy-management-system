using System;
using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Data
{
    public interface ICashCountRepository
    {
        // End of the last recorded cash count, or null if there is none yet.
        DateTime? GetLastPeriodEnd();

        // date_registered of the first sale ever, or null if there are no sales. Used as the
        // period start for the very first cash count so it does not silently skip past sales.
        DateTime? GetEarliestSaleDate();

        // Sum of sale.total_amount per payment_method for [start, end). Credit notes (negative
        // amounts) net out on their own. Only methods that had activity are returned.
        List<CashCountLine> GetExpectedTotals(DateTime start, DateTime end);

        // Persists one cash count (header + one line per payment method) in a transaction.
        // Returns the new id, or 0 on failure.
        int Register(CashCount cashCount);

        // Recorded cash counts, newest first.
        List<CashCount> History();
    }
}
