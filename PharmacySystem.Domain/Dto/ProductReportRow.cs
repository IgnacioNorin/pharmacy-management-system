using System;

namespace PharmacySystem.Model
{
    public class ProductReportRow
    {
        public DateTime DateCreated { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CategoryDescription { get; set; }
        public int Stock { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public DateTime DateExpired { get; set; }
        public string StatusName { get; set; }
    }
}
