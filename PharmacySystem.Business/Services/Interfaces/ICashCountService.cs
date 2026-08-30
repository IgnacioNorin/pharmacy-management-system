using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    public interface ICashCountService
    {
        // Builds a not-yet-saved CashCount for the current open period: period start is the end
        // of the last recorded count (or the first sale, or today), period end is "now", and one
        // line per selectable payment method with its expected total filled in and counted 0.
        CashCount PrepareCurrent();

        // Persists a completed cash count. Returns the new id, or 0 on failure.
        int Register(CashCount cashCount);

        List<CashCount> History();
    }
}
