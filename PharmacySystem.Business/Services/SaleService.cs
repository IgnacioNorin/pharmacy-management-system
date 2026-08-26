using System.Collections.Generic;
using PharmacySystem.Data;
using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    // Thin: same reasoning as PurchaseService - the sale + detail rows transaction is a
    // persistence concern, not a branching business rule.
    public class SaleService : ISaleService
    {
        private readonly ISaleRepository _repository;

        public SaleService(ISaleRepository repository)
        {
            _repository = repository;
        }

        public List<Sale> ListSale() => _repository.ListSale();

        public List<SaleDetail> ListSaleDetail() => _repository.ListSaleDetail();

        public bool ControlStock(int idproduct, int amount, bool subtract) =>
            _repository.ControlStock(idproduct, amount, subtract);

        public int Register(Sale sale) => _repository.Register(sale);

        public List<SaleReportRow> ReportSale(string startDate, string endDate) => _repository.ReportSale(startDate, endDate);

        public decimal SumTotalPay(string startDate, string endDate) => _repository.SumTotalPay(startDate, endDate);

        public decimal SumAmountReceived(string startDate, string endDate) => _repository.SumAmountReceived(startDate, endDate);

        public decimal SumChangeAmount(string startDate, string endDate) => _repository.SumChangeAmount(startDate, endDate);
    }
}
