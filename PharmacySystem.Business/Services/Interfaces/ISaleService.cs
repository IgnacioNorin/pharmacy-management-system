using System;
using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    public interface ISaleService
    {
        List<Sale> ListSale();
        List<SaleDetail> ListSaleDetail();
        Sale GetById(int saleId);
        List<SaleDetail> GetDetailsBySaleId(int saleId);
        List<SalePayment> GetPaymentsBySaleId(int saleId);
        int Register(Sale sale);
        SaleLookup FindByDocument(string documentType, string documentNumber);
        CreditNoteResult CreateCreditNote(int originalSaleId, int userId, string reason);
        List<SaleReportRow> ReportSale(DateTime startDate, DateTime endDate, int clientId);
        decimal SumTotalPay(DateTime startDate, DateTime endDate);
    }
}
