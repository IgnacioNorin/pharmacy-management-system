using System;
using System.Collections.Generic;
using PharmacySystem.Fiscal;
using PharmacySystem.Model;

namespace PharmacySystem.Data
{
    public interface ISaleRepository
    {
        List<Sale> ListSale();
        List<SaleDetail> ListSaleDetail();
        Sale GetById(int saleId);
        List<SaleDetail> GetDetailsBySaleId(int saleId);
        List<SalePayment> GetPaymentsBySaleId(int saleId);
        int Register(Sale sale);
        void SaveFiscalResult(int saleId, FiscalDocumentResult result);
        SaleLookup FindByDocument(string documentType, string documentNumber);
        // The lines of a sale with how many units of each are still creditable.
        System.Collections.Generic.List<SaleCreditDetail> GetCreditableLines(int saleId);
        // Issues a Nota de Credito that credits the requested quantity of each given original line.
        CreditNoteResult CreateCreditNote(int originalSaleId, int userId, string reason,
            System.Collections.Generic.IReadOnlyList<CreditNoteLineRequest> lines);
        List<SaleReportRow> ReportSale(DateTime startDate, DateTime endDate, int clientId);
        decimal SumTotalPay(DateTime startDate, DateTime endDate);
    }
}
