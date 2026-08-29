namespace PharmacySystem.Model
{
    // How a sale was paid. Stored as a plain string in sale.payment_method; these constants keep
    // the values consistent across the presenter, the ticket and the report. One method per sale
    // for now - mixed payments would need a sale_payment table.
    public static class PaymentMethods
    {
        public const string Efectivo = "Efectivo";
        public const string Tarjeta = "Tarjeta";
        public const string Transferencia = "Transferencia";

        public static readonly string[] Selectable = { Efectivo, Tarjeta, Transferencia };

        public const string Default = Efectivo;
    }
}
