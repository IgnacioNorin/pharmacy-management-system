using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Data
{
    // ReportSale() has no home here yet, same reason as the other Report()/ReportPurchase()
    // methods - it formats currency/dates with WinForms-side helpers. It stays on the adapter
    // until frmReport is migrated.
    public interface ISaleRepository
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
