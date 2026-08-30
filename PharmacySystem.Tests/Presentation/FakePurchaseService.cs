using System;
using System.Collections.Generic;
using PharmacySystem.Business;
using PharmacySystem.Model;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakePurchaseService : IPurchaseService
    {
        public bool RegisterResult { get; set; } = true;
        public Exception RegisterThrows { get; set; }
        public List<PurchaseReportRow> ReportResult { get; set; } = new List<PurchaseReportRow>();
        public PurchaseReportTotals TotalsResult { get; set; } = new PurchaseReportTotals();
        public Purchase RegisteredWith { get; private set; }

        public bool Register(Purchase purchase)
        {
            RegisteredWith = purchase;
            if (RegisterThrows != null) throw RegisterThrows;
            return RegisterResult;
        }
        public List<PurchaseReportRow> ReportPurchase(string idSupplier, DateTime startDate, DateTime endDate) => ReportResult;
        public PurchaseReportTotals GetTotals(string idSupplier, DateTime startDate, DateTime endDate) => TotalsResult;
    }
}
