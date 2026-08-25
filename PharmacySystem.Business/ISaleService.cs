using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    public interface ISaleService
    {
        List<Sale> ListSale();
        List<SaleDetail> ListSaleDetail();
        bool ControlStock(int idproduct, int amount, bool subtract);
        int Register(Sale sale);
        decimal SumTotalPay(string startDate, string endDate);
        decimal SumAmountReceived(string startDate, string endDate);
        decimal SumChangeAmount(string startDate, string endDate);
    }
}
