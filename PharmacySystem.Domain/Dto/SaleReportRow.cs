using System;

namespace PharmacySystem.Model
{
    // Raw read model for SaleRepository.ReportSale() - typed fields instead of the
    // pre-formatted-string DataTable the original ReportSale() built directly in the data layer.
    public class SaleReportRow
    {
        public DateTime DateRegistered { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public string SellerDocument { get; set; } = string.Empty;
        public string SellerName { get; set; } = string.Empty;
        // Client or, on a Factura, the receptor - the repository merges the two so there is one
        // pair of columns, not two.
        public string ClientDocument { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public decimal NetAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ExemptAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountReceived { get; set; }
        public decimal ChangeAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
    }
}
