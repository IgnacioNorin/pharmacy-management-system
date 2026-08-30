using System;
using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Data;
using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    // Thin over the repository. The only real logic here is assembling the "current open period"
    // shape the arqueo screen needs: the period bounds and one line per selectable payment
    // method, so the view always shows the same rows even when a method had no activity.
    public class CashCountService : ICashCountService
    {
        private readonly ICashCountRepository _repository;

        public CashCountService(ICashCountRepository repository)
        {
            _repository = repository;
        }

        public CashCount PrepareCurrent()
        {
            DateTime end = DateTime.Now;
            DateTime start = _repository.GetLastPeriodEnd()
                             ?? _repository.GetEarliestSaleDate()
                             ?? DateTime.Today;

            if (start > end)
            {
                start = end;
            }

            Dictionary<string, decimal> expected = _repository.GetExpectedTotals(start, end)
                .Where(l => l.paymentMethod != null)
                .ToDictionary(l => l.paymentMethod, l => l.expectedAmount, StringComparer.OrdinalIgnoreCase);

            var lines = PaymentMethods.Selectable.Select(method => new CashCountLine
            {
                paymentMethod = method,
                expectedAmount = expected.TryGetValue(method, out decimal amount) ? amount : 0m,
                countedAmount = 0m
            }).ToList();

            return new CashCount
            {
                periodStart = start,
                periodEnd = end,
                lines = lines
            };
        }

        public int Register(CashCount cashCount) => _repository.Register(cashCount);

        public List<CashCount> History() => _repository.History();
    }
}
