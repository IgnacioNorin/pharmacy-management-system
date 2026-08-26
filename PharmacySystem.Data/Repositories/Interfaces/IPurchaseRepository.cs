using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Data
{
    public interface IPurchaseRepository
    {
        bool Register(Purchase purchase);
        List<PurchaseReportRow> ReportPurchase(string idSupplier, string startDate, string endDate);
        decimal GetTotalAmount(string idSupplier, string startDate, string endDate);
        decimal GetTotalPurchasePrice(string idSupplier, string startDate, string endDate);
        int GetTotalQuantity(string idSupplier, string startDate, string endDate);
        decimal GetTotalSalesPrice(string idSupplier, string startDate, string endDate);
        decimal GetSubTotal(string idSupplier, string startDate, string endDate);
    }
}
