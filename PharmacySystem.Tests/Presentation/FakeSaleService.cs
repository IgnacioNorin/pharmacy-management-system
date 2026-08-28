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
        public List<SaleReportRow> ReportResult { get; set; } = new List<SaleReportRow>();
        public decimal SumTotalPayResult { get; set; }
        public decimal SumAmountReceivedResult { get; set; }
        public decimal SumChangeAmountResult { get; set; }
        public Sale RegisteredWith { get; private set; }

        public SaleLookup FindByDocumentResult { get; set; }
        public CreditNoteResult CreateCreditNoteResult { get; set; } = CreditNoteResult.Ok;
        public (string Type, string Number)? FindByDocumentArgs { get; private set; }
        public (int SaleId, int UserId, string Reason)? CreditNoteArgs { get; private set; }

        public List<Sale> ListSale() => ListSaleResult;
        public List<SaleDetail> ListSaleDetail() => ListSaleDetailResult;

        public int Register(Sale sale)
        {
            RegisteredWith = sale;
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
        public List<SaleReportRow> ReportSale(DateTime startDate, DateTime endDate) => ReportResult;
        public decimal SumTotalPay(DateTime startDate, DateTime endDate) => SumTotalPayResult;
        public decimal SumAmountReceived(DateTime startDate, DateTime endDate) => SumAmountReceivedResult;
        public decimal SumChangeAmount(DateTime startDate, DateTime endDate) => SumChangeAmountResult;
    }
}
