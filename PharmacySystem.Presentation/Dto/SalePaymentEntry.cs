namespace PharmacySystem.Presentation
{
    // One row of the "pago mixto" split the cashier enters: a method and the amount collected
    // with it. The rows must sum to the sale total. Kept separate from the domain SalePayment
    // so the view never depends on PharmacySystem.Model.
    public class SalePaymentEntry
    {
        public string Method { get; set; }
        public decimal Amount { get; set; }

        public SalePaymentEntry() { }

        public SalePaymentEntry(string method, decimal amount)
        {
            Method = method;
            Amount = amount;
        }
    }
}
