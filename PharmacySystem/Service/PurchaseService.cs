using PharmacySystem.Data;
using PharmacySystem.Model;

namespace PharmacySystem.Logical
{
    // Thin adapter kept for screens not migrated yet (frmPurchase.cs). Delegates to
    // PharmacySystem.Business.
    public class PurchaseService
    {
        private static PurchaseService _instance = null;
        private readonly Business.IPurchaseService _inner;

        public PurchaseService()
        {
            _inner = new Business.PurchaseService(new PurchaseRepository(CompositionRoot.ConnectionFactory));
        }

        public static PurchaseService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PurchaseService();
                }

                return _instance;
            }
        }

        public bool RegisterPurchase(Purchase purchase) => _inner.Register(purchase);

        public decimal GetTotalAmount(string idSupplier, string startDate, string endDate) =>
            _inner.GetTotalAmount(idSupplier, startDate, endDate);

        public decimal GetTotalPurchasePrice(string idSupplier, string startDate, string endDate) =>
            _inner.GetTotalPurchasePrice(idSupplier, startDate, endDate);

        public int GetTotalQuantity(string idSupplier, string startDate, string endDate) =>
            _inner.GetTotalQuantity(idSupplier, startDate, endDate);

        public decimal GetTotalSalesPrice(string idSupplier, string startDate, string endDate) =>
            _inner.GetTotalSalesPrice(idSupplier, startDate, endDate);

        public decimal GetSubTotal(string idSupplier, string startDate, string endDate) =>
            _inner.GetSubTotal(idSupplier, startDate, endDate);
    }
}
