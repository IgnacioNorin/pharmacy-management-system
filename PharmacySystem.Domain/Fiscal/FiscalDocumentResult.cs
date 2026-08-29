namespace PharmacySystem.Fiscal
{
    // Outcome of handing a sale to a fiscal document issuer. The default issuer only sets
    // Status; a DTE-provider issuer also returns the authority's tracking id, the stamp
    // barcode, and (when the provider assigns the folio itself) an overriding DocumentNumber.
    public class FiscalDocumentResult
    {
        // Non-null only when the issuer assigns the folio (e.g. from a CAF range). Null keeps
        // the number already assigned by the local sequence.
        public string DocumentNumber { get; set; }
        public string Status { get; set; } = FiscalStatuses.Interno;
        public string TrackId { get; set; }
        public string Barcode { get; set; }
    }

    public static class FiscalStatuses
    {
        // Internal receipt, numbered locally, no tax authority contacted.
        public const string Interno = "interno";
        // Sent to the provider / authority, awaiting acceptance.
        public const string Pendiente = "pendiente";
        public const string Aceptado = "aceptado";
        public const string Rechazado = "rechazado";
    }
}
