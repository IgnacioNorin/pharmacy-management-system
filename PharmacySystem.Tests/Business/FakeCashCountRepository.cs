using System;
using System.Collections.Generic;
using PharmacySystem.Data;
using PharmacySystem.Model;

namespace PharmacySystem.Tests.Business
{
    internal class FakeCashCountRepository : ICashCountRepository
    {
        public DateTime? LastPeriodEnd { get; set; }
        public DateTime? EarliestSaleDate { get; set; }
        public List<CashCountLine> ExpectedTotals { get; set; } = new List<CashCountLine>();
        public int RegisterResult { get; set; } = 1;
        public List<CashCount> HistoryResult { get; set; } = new List<CashCount>();

        public CashCount RegisteredWith { get; private set; }
        public (DateTime Start, DateTime End)? ExpectedTotalsRequestedFor { get; private set; }

        public DateTime? GetLastPeriodEnd() => LastPeriodEnd;
        public DateTime? GetEarliestSaleDate() => EarliestSaleDate;

        public List<CashCountLine> GetExpectedTotals(DateTime start, DateTime end)
        {
            ExpectedTotalsRequestedFor = (start, end);
            return ExpectedTotals;
        }

        public int Register(CashCount cashCount)
        {
            RegisteredWith = cashCount;
            return RegisterResult;
        }

        public List<CashCount> History() => HistoryResult;
    }
}
