using PharmacySystem.Model;

namespace PharmacySystem.Data
{
    // ReportPurchase() has no home here yet, same reason as IProductRepository's Report():
    // it formats currency/dates with the WinForms-side CultureInfoHelper/DateHelper. It stays
    // on the adapter until frmReport itself is migrated.
    public interface IPurchaseRepository
    {
        bool Register(Purchase purchase);
        decimal GetTotalAmount(string idSupplier, string startDate, string endDate);
        decimal GetTotalPurchasePrice(string idSupplier, string startDate, string endDate);
        int GetTotalQuantity(string idSupplier, string startDate, string endDate);
        decimal GetTotalSalesPrice(string idSupplier, string startDate, string endDate);
        decimal GetSubTotal(string idSupplier, string startDate, string endDate);
    }
}
