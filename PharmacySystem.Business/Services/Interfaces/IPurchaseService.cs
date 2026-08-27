using System;
using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    public interface IPurchaseService
    {
        bool Register(Purchase purchase);
        List<PurchaseReportRow> ReportPurchase(string idSupplier, DateTime startDate, DateTime endDate);
        decimal GetTotalAmount(string idSupplier, DateTime startDate, DateTime endDate);
        decimal GetTotalPurchasePrice(string idSupplier, DateTime startDate, DateTime endDate);
        int GetTotalQuantity(string idSupplier, DateTime startDate, DateTime endDate);
        decimal GetTotalSalesPrice(string idSupplier, DateTime startDate, DateTime endDate);
        decimal GetSubTotal(string idSupplier, DateTime startDate, DateTime endDate);
    }
}
