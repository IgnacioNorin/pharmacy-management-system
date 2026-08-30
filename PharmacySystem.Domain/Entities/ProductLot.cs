using System;

namespace PharmacySystem.Model
{
    // One received batch of a product: how many units of it are still on hand, when it expires
    // and what it cost. A purchase creates one per line; a sale draws down the earliest-expiring
    // lots first (FEFO). product.stock is the cached sum of these quantities.
    public class ProductLot
    {
        public int id { get; set; }
        public int productId { get; set; }
        public int? purchaseDetailId { get; set; }
        public int quantity { get; set; }
        public DateTime? dateExpired { get; set; }
        public decimal? unitCost { get; set; }
        public DateTime receivedAt { get; set; }
    }
}
