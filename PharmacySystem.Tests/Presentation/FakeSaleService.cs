using System;
using System.Collections.Generic;
using PharmacySystem.Business;
using PharmacySystem.Model;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakeSaleService : ISaleService
    {
        public List<Sale> ListSaleResult { get; set; } = new List<Sale>();
        public List<SaleDetail> ListSaleDetailResult { get; set; } = new List<SaleDetail>();
        public int RegisterResult { get; set; } = 1;
        public Exception RegisterThrows { get; set; }
        public List<SaleReportRow> ReportResult { get; set; } = new List<SaleReportRow>();
        public decimal SumTotalPayResult { get; set; }
        public Sale RegisteredWith { get; private set; }

        public SaleLookup FindByDocumentResult { get; set; }
        public CreditNoteResult CreateCreditNoteResult { get; set; } = CreditNoteResult.Ok;
        public (string Type, string Number)? FindByDocumentArgs { get; private set; }
        public (int SaleId, int UserId, string Reason)? CreditNoteArgs { get; private set; }

        public List<Sale> ListSale() => ListSaleResult;
        public List<SaleDetail> ListSaleDetail() => ListSaleDetailResult;
        public Sale GetById(int saleId) => ListSaleResult.Find(s => s.idSale == saleId);
        public List<SaleDetail> GetDetailsBySaleId(int saleId) => ListSaleDetailResult.FindAll(d => d.idSale == saleId);
        public List<SalePayment> GetPaymentsBySaleId(int saleId) =>
            ListSaleResult.Find(s => s.idSale == saleId)?.payments ?? new List<SalePayment>();

        public int Register(Sale sale)
        {
            RegisteredWith = sale;
            if (RegisterThrows != null) throw RegisterThrows;
            return RegisterResult;
        }

        public SaleLookup FindByDocument(string documentType, string documentNumber)
        {
            FindByDocumentArgs = (documentType, documentNumber);
            return FindByDocumentResult;
        }

        public CreditNoteResult CreateCreditNote(int originalSaleId, int userId, string reason)
        {
            CreditNoteArgs = (originalSaleId, userId, reason);
            return CreateCreditNoteResult;
        }
        public List<SaleReportRow> ReportSale(DateTime startDate, DateTime endDate, int clientId) { ReportClientId = clientId; return ReportResult; }
        public int ReportClientId { get; private set; }
        public decimal SumTotalPay(DateTime startDate, DateTime endDate) => SumTotalPayResult;
    }
}
