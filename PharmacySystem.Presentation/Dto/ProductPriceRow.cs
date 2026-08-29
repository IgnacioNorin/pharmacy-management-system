namespace PharmacySystem.Presentation
{
    // One product line in the Prices screen, in either the "to release" grid or the
    // "commercialized" grid.
    public class ProductPriceRow
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int Stock { get; set; }
        public decimal Cost { get; set; }
        // null until the product has been released (given a sale price).
        public decimal? SalePrice { get; set; }
        // (SalePrice - Cost) / SalePrice * 100, or null when there is no price or it is zero.
        public decimal? MarginPercent { get; set; }
        public bool TaxAffected { get; set; }
    }
}
