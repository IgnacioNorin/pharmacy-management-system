namespace PharmacySystem.Presentation
{
    // One row of a product's price timeline as shown in the Prices screen.
    public class ProductPriceHistoryRow
    {
        public string DateText { get; set; }
        public string EventText { get; set; }
        public string SalePriceText { get; set; }
        public string CostText { get; set; }
        public string UserName { get; set; }
        public string Reason { get; set; }
    }
}
