using System;

namespace PharmacySystem.Presentation
{
    public class PurchaseCartLine
    {
        public int ProductId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public DateTime ExpirationDate { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SubTotal { get; set; }
        // Whether this product is subject to VAT (product.tax_affected). Exempt lines feed
        // exempt_amount instead of the taxable base when the invoice breakdown is computed.
        public bool TaxAffected { get; set; } = true;
    }
}
