using System;
using System.Collections.Generic;
using PharmacySystem.Data;
using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    // Thin: same reasoning as PurchaseService - the sale + detail rows transaction is a
    // persistence concern, not a branching business rule. Stock is now discounted inside that
    // same transaction (SaleRepository.Register), so there is no separate ControlStock step.
    public class SaleService : ISaleService
    {
        private readonly ISaleRepository _repository;

        public SaleService(ISaleRepository repository)
        {
            _repository = repository;
        }

        public List<Sale> ListSale() => _repository.ListSale();

        public List<SaleDetail> ListSaleDetail() => _repository.ListSaleDetail();

        public int Register(Sale sale) => _repository.Register(sale);

        public List<SaleReportRow> ReportSale(DateTime startDate, DateTime endDate) => _repository.ReportSale(startDate, endDate);

        public decimal SumTotalPay(DateTime startDate, DateTime endDate) => _repository.SumTotalPay(startDate, endDate);

        public decimal SumAmountReceived(DateTime startDate, DateTime endDate) => _repository.SumAmountReceived(startDate, endDate);

        public decimal SumChangeAmount(DateTime startDate, DateTime endDate) => _repository.SumChangeAmount(startDate, endDate);
    }
}
