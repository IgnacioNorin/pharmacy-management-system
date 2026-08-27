using System;
using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Data
{
    public interface ISaleRepository
    {
        List<Sale> ListSale();
        List<SaleDetail> ListSaleDetail();
        int Register(Sale sale);
        List<SaleReportRow> ReportSale(DateTime startDate, DateTime endDate);
        decimal SumTotalPay(DateTime startDate, DateTime endDate);
        decimal SumAmountReceived(DateTime startDate, DateTime endDate);
        decimal SumChangeAmount(DateTime startDate, DateTime endDate);
    }
}
