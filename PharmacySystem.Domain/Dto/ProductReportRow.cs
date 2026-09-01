using System;

namespace PharmacySystem.Model
{
    public class ProductReportRow
    {
        public DateTime DateCreated { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CategoryDescription { get; set; } = string.Empty;
        public int Stock { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SalePrice { get; set; }
        // Stock valued at each lot's own purchase cost: SUM(product_lot.quantity * unit_cost).
        // Falls back to Stock * average/purchase cost for a product with no lots.
        public decimal StockCostValue { get; set; }
        public DateTime DateExpired { get; set; }
        public string StatusName { get; set; } = string.Empty;
        // product.status = 1. Delisted products still appear in the report, but the totals row
        // (inventory valuation) only counts active ones (DEF-40).
        public bool Active { get; set; }
    }
}
