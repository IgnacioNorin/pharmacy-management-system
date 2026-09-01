namespace PharmacySystem.Presentation
{
    // One row of a product's price timeline as shown in the Prices screen.
    public class ProductPriceHistoryRow
    {
        public string DateText { get; set; } = string.Empty;
        public string EventText { get; set; } = string.Empty;
        public string SalePriceText { get; set; } = string.Empty;
        public string CostText { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}
