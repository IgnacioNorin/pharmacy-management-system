namespace PharmacySystem.Model
{
    // One row for the notification center (Fase 3 of the alerts rework): a product that is
    // either critically/low on stock, expired, or expiring soon, with the specific detail text
    // already formatted - the View never has to know how a severity was derived.
    public class ProductAlert
    {
        public int ProductId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public AlertSeverity Severity { get; set; }
        public string Detail { get; set; }
    }
}
