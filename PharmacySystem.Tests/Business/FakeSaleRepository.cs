using System;
using System.Collections.Generic;
using PharmacySystem.Data;
using PharmacySystem.Fiscal;
using PharmacySystem.Model;

namespace PharmacySystem.Tests.Business
{
    internal class FakeSaleRepository : ISaleRepository
    {
        public int RegisterResult { get; set; } = 1;
        public Sale RegisteredSale { get; private set; }
        public int SavedFiscalSaleId { get; private set; }
        public FiscalDocumentResult SavedFiscalResult { get; private set; }
        public int SaveFiscalResultCalls { get; private set; }

        public int Register(Sale sale)
        {
            RegisteredSale = sale;
            return RegisterResult;
        }

        public void SaveFiscalResult(int saleId, FiscalDocumentResult result)
        {
            SaveFiscalResultCalls++;
            SavedFiscalSaleId = saleId;
            SavedFiscalResult = result;
        }

        public List<Sale> ListSale() => new List<Sale>();
        public List<SaleDetail> ListSaleDetail() => new List<SaleDetail>();
        public Sale GetById(int saleId) => null;
        public List<SaleDetail> GetDetailsBySaleId(int saleId) => new List<SaleDetail>();
        public List<SalePayment> GetPaymentsBySaleId(int saleId) => new List<SalePayment>();
        public SaleLookup FindByDocument(string documentType, string documentNumber) => null;
        public List<SaleCreditDetail> GetCreditableLines(int saleId) => new List<SaleCreditDetail>();
        public CreditNoteResult CreateCreditNote(int originalSaleId, int userId, string reason,
            IReadOnlyList<CreditNoteLineRequest> lines) => CreditNoteResult.Ok;
        public List<SaleReportRow> ReportSale(DateTime startDate, DateTime endDate, int clientId) => new List<SaleReportRow>();
        public decimal SumTotalPay(DateTime startDate, DateTime endDate) => 0;
        public decimal SumAmountReceived(DateTime startDate, DateTime endDate) => 0;
        public decimal SumChangeAmount(DateTime startDate, DateTime endDate) => 0;
    }
}
