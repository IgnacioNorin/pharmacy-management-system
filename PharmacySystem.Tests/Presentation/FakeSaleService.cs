using System.Collections.Generic;
using PharmacySystem.Business;
using PharmacySystem.Model;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakeSaleService : ISaleService
    {
        public List<Sale> ListSaleResult { get; set; } = new List<Sale>();
        public List<SaleDetail> ListSaleDetailResult { get; set; } = new List<SaleDetail>();
        public bool ControlStockResult { get; set; } = true;
        public int RegisterResult { get; set; } = 1;
        public List<SaleReportRow> ReportResult { get; set; } = new List<SaleReportRow>();
        public decimal SumTotalPayResult { get; set; }
        public decimal SumAmountReceivedResult { get; set; }
        public decimal SumChangeAmountResult { get; set; }

        public List<Sale> ListSale() => ListSaleResult;
        public List<SaleDetail> ListSaleDetail() => ListSaleDetailResult;
        public bool ControlStock(int idproduct, int amount, bool subtract) => ControlStockResult;
        public int Register(Sale sale) => RegisterResult;
        public List<SaleReportRow> ReportSale(string startDate, string endDate) => ReportResult;
        public decimal SumTotalPay(string startDate, string endDate) => SumTotalPayResult;
        public decimal SumAmountReceived(string startDate, string endDate) => SumAmountReceivedResult;
        public decimal SumChangeAmount(string startDate, string endDate) => SumChangeAmountResult;
    }
}
