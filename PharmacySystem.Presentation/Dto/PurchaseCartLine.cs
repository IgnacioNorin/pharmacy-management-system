using System;

namespace PharmacySystem.Presentation
{
    public class PurchaseCartLine
    {
        public int ProductId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public decimal Quantity { get; set; }
        public DateTime ExpirationDate { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SubTotal { get; set; }
    }
}
