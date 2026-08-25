using System.Collections.Generic;
using PharmacySystem.Business;
using PharmacySystem.Model;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakePurchaseService : IPurchaseService
    {
        public bool RegisterResult { get; set; } = true;
        public List<PurchaseReportRow> ReportResult { get; set; } = new List<PurchaseReportRow>();
        public decimal TotalAmountResult { get; set; }
        public decimal TotalPurchasePriceResult { get; set; }
        public int TotalQuantityResult { get; set; }
        public decimal TotalSalesPriceResult { get; set; }
        public decimal SubTotalResult { get; set; }

        public bool Register(Purchase purchase) => RegisterResult;
        public List<PurchaseReportRow> ReportPurchase(string idSupplier, string startDate, string endDate) => ReportResult;
        public decimal GetTotalAmount(string idSupplier, string startDate, string endDate) => TotalAmountResult;
        public decimal GetTotalPurchasePrice(string idSupplier, string startDate, string endDate) => TotalPurchasePriceResult;
        public int GetTotalQuantity(string idSupplier, string startDate, string endDate) => TotalQuantityResult;
        public decimal GetTotalSalesPrice(string idSupplier, string startDate, string endDate) => TotalSalesPriceResult;
        public decimal GetSubTotal(string idSupplier, string startDate, string endDate) => SubTotalResult;
    }
}
