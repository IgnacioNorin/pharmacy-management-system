using System;

namespace PharmacySystem.Model
{
    public class PurchaseReportRow
    {
        public DateTime DateRegistered { get; set; }
        public string SupplierDocument { get; set; }
        public string CompanyName { get; set; }
        public string DocumentType { get; set; }
        public string DocumentNumber { get; set; }
        public decimal TotalAmount { get; set; }
        // Invoice-header VAT breakdown, repeated across the purchase's detail lines (like TotalAmount).
        public decimal NetAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ExemptAmount { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal PurchasePrice { get; set; }
        // Line total for this product: Quantity * PurchasePrice.
        public decimal LineTotal { get; set; }
    }
}
