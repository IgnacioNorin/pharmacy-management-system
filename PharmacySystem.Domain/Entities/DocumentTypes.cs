namespace PharmacySystem.Model
{
    // Sale document types. Country-neutral wording ("Boleta" = consumer receipt, "Factura" =
    // tax invoice). Stored as plain strings in sale.document_type; these constants keep the
    // values consistent across the presenter, the folio sequences and the store default.
    public static class DocumentTypes
    {
        public const string Boleta = "Boleta";
        public const string Factura = "Factura";
        public const string NotaCredito = "Nota de Credito";

        // Types a cashier can pick when making a sale (Nota de Credito is issued from its own flow).
        public static readonly string[] Selectable = { Boleta, Factura };
    }
}
