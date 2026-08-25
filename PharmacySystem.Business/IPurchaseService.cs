using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    public interface IPurchaseService
    {
        bool Register(Purchase purchase);
        decimal GetTotalAmount(string idSupplier, string startDate, string endDate);
        decimal GetTotalPurchasePrice(string idSupplier, string startDate, string endDate);
        int GetTotalQuantity(string idSupplier, string startDate, string endDate);
        decimal GetTotalSalesPrice(string idSupplier, string startDate, string endDate);
        decimal GetSubTotal(string idSupplier, string startDate, string endDate);
    }
}
