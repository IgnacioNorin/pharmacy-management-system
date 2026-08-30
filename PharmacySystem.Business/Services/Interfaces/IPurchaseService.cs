using System;
using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    public interface IPurchaseService
    {
        bool Register(Purchase purchase);
        List<PurchaseReportRow> ReportPurchase(string idSupplier, DateTime startDate, DateTime endDate);
        PurchaseReportTotals GetTotals(string idSupplier, DateTime startDate, DateTime endDate);
    }
}
