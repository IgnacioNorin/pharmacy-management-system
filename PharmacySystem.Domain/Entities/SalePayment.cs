namespace PharmacySystem.Model
{
    // One line of a sale's payment breakdown: how much was collected with a given method.
    // A single-method sale has exactly one of these; a mixed sale has several that sum to
    // sale.totalPay. See PaymentMethods for the method values.
    public class SalePayment
    {
        public string paymentMethod { get; set; }
        public decimal amount { get; set; }
    }
}
