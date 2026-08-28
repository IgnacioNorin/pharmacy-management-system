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
        private readonly IFiscalDocumentIssuer _issuer;

        public SaleService(ISaleRepository repository, IFiscalDocumentIssuer issuer)
        {
            _repository = repository;
            _issuer = issuer;
        }

        public List<Sale> ListSale() => _repository.ListSale();

        public List<SaleDetail> ListSaleDetail() => _repository.ListSaleDetail();

        public int Register(Sale sale)
        {
            int id = _repository.Register(sale);
            if (id == 0)
                return 0;

            // Hand the persisted sale to the fiscal issuer and store back whatever it resolves
            // (status, and for a real DTE also the tracking id / barcode / assigned folio).
            var fiscal = _issuer.Issue(id, sale);
            if (fiscal != null)
                _repository.SaveFiscalResult(id, fiscal);

            return id;
        }

        public SaleLookup FindByDocument(string documentType, string documentNumber) =>
            _repository.FindByDocument(documentType, documentNumber);

        public CreditNoteResult CreateCreditNote(int originalSaleId, int userId, string reason) =>
            _repository.CreateCreditNote(originalSaleId, userId, reason);

        public List<SaleReportRow> ReportSale(DateTime startDate, DateTime endDate) => _repository.ReportSale(startDate, endDate);

        public decimal SumTotalPay(DateTime startDate, DateTime endDate) => _repository.SumTotalPay(startDate, endDate);

        public decimal SumAmountReceived(DateTime startDate, DateTime endDate) => _repository.SumAmountReceived(startDate, endDate);

        public decimal SumChangeAmount(DateTime startDate, DateTime endDate) => _repository.SumChangeAmount(startDate, endDate);
    }
}
