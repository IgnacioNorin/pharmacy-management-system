using System.Collections.Generic;
using PharmacySystem.Business;
using PharmacySystem.Model;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakeCashCountService : ICashCountService
    {
        public CashCount PrepareResult { get; set; } = new CashCount();
        public int RegisterResult { get; set; } = 1;
        public List<CashCount> HistoryResult { get; set; } = new List<CashCount>();

        public CashCount RegisteredWith { get; private set; }

        public CashCount PrepareCurrent() => PrepareResult;

        public int Register(CashCount cashCount)
        {
            RegisteredWith = cashCount;
            return RegisterResult;
        }

        public List<CashCount> History() => HistoryResult;
    }
}
