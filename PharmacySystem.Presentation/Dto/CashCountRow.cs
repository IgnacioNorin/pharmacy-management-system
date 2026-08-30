namespace PharmacySystem.Presentation
{
    // One payment-method row on the arqueo screen: the system's expected total and, once the
    // user has entered it, what was physically counted, plus their difference.
    public class CashCountRow
    {
        public string PaymentMethod { get; set; }
        public decimal Expected { get; set; }
        public decimal Counted { get; set; }
        public decimal Difference => Counted - Expected;
    }
}
