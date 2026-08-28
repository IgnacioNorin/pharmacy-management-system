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
        int Register(Sale sale);
        void SaveFiscalResult(int saleId, FiscalDocumentResult result);
        SaleLookup FindByDocument(string documentType, string documentNumber);
        CreditNoteResult CreateCreditNote(int originalSaleId, int userId, string reason);
        List<SaleReportRow> ReportSale(DateTime startDate, DateTime endDate, int clientId);
        decimal SumTotalPay(DateTime startDate, DateTime endDate);
    }
}
