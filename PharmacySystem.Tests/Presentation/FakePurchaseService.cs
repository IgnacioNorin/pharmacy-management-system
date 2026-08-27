using System;
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
        public Purchase RegisteredWith { get; private set; }

        public bool Register(Purchase purchase)
        {
            RegisteredWith = purchase;
            return RegisterResult;
        }
        public List<PurchaseReportRow> ReportPurchase(string idSupplier, DateTime startDate, DateTime endDate) => ReportResult;
        public decimal GetTotalAmount(string idSupplier, DateTime startDate, DateTime endDate) => TotalAmountResult;
        public decimal GetTotalPurchasePrice(string idSupplier, DateTime startDate, DateTime endDate) => TotalPurchasePriceResult;
        public int GetTotalQuantity(string idSupplier, DateTime startDate, DateTime endDate) => TotalQuantityResult;
        public decimal GetTotalSalesPrice(string idSupplier, DateTime startDate, DateTime endDate) => TotalSalesPriceResult;
        public decimal GetSubTotal(string idSupplier, DateTime startDate, DateTime endDate) => SubTotalResult;
    }
}
