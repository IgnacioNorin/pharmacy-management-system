using System;

namespace PharmacySystem.Model
{
    // Raw read model for SaleRepository.ReportSale() - typed fields instead of the
    // pre-formatted-string DataTable the original ReportSale() built directly in the data layer.
    public class SaleReportRow
    {
        public DateTime DateRegistered { get; set; }
        public string DocumentType { get; set; }
        public string DocumentNumber { get; set; }
        public string SellerDocument { get; set; }
        public string SellerName { get; set; }
        public string ClientDocument { get; set; }
        public string ClientName { get; set; }
        public decimal NetAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ExemptAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountReceived { get; set; }
        public decimal ChangeAmount { get; set; }
    }
}
