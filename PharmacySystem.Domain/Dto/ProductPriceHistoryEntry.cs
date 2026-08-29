using System;

namespace PharmacySystem.Model
{
    // One entry in a product's price timeline: a release, a re-price or a withdrawal from sale.
    public class ProductPriceHistoryEntry
    {
        public DateTime ChangedAt { get; set; }
        // "liberacion" | "cambio" | "retiro"
        public string EventType { get; set; }
        public decimal SalePrice { get; set; }
        public decimal? Cost { get; set; }
        public string UserName { get; set; }
        public string Reason { get; set; }
    }
}
