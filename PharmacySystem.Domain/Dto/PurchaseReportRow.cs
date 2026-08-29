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
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal PurchasePrice { get; set; }
    }
}
