using PharmacySystem.Data;
using PharmacySystem.Model;
using System.Collections.Generic;

namespace PharmacySystem.Logical
{
    // Thin adapter kept for screens not migrated yet (frmSale.cs, PrintSale.cs). Delegates to
    // PharmacySystem.Business.
    public class SaleService
    {
        private static SaleService instance = null;
        private readonly Business.ISaleService _inner;

        public SaleService()
        {
            _inner = new Business.SaleService(new SaleRepository(CompositionRoot.ConnectionFactory));
        }

        public static SaleService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SaleService();
                }
                return instance;
            }
        }

        public List<Sale> ListSale() => _inner.ListSale();

        public List<SaleDetail> ListSaleDetail() => _inner.ListSaleDetail();

        public bool ControlStock(int idproduct, int amount, bool subtract) =>
            _inner.ControlStock(idproduct, amount, subtract);

        public int RegisterSale(Sale obj) => _inner.Register(obj);

        public decimal SumTotalPay(string startDate, string endDate) => _inner.SumTotalPay(startDate, endDate);

        public decimal SumAmountReceived(string startDate, string endDate) => _inner.SumAmountReceived(startDate, endDate);

        public decimal SumChangeAmount(string startDate, string endDate) => _inner.SumChangeAmount(startDate, endDate);
    }
}
