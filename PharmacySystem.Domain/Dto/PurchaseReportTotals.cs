namespace PharmacySystem.Model
{
    // Purchase-header aggregates for the purchases report. These columns live on the purchase
    // header and are repeated across its detail lines, so they cannot be summed from the report
    // rows without multiplying by the line count - they come from a single header-only query.
    public class PurchaseReportTotals
    {
        public decimal TotalAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ExemptAmount { get; set; }
    }
}
