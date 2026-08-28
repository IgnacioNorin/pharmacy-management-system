using PharmacySystem.Fiscal;
using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    // Default issuer: the sale keeps the folio already assigned by the local DB sequence and
    // is treated as an internal, non-fiscal receipt. No external service is contacted.
    //
    // To emit legally valid electronic documents, implement IFiscalDocumentIssuer against a
    // DTE provider's API (it returns the tracking id, the stamp barcode and, if it assigns
    // the folio, an overriding DocumentNumber) and register it in CompositionRoot instead.
    public class LocalSequenceIssuer : IFiscalDocumentIssuer
    {
        public FiscalDocumentResult Issue(int saleId, Sale sale) =>
            new FiscalDocumentResult { Status = FiscalStatuses.Interno };
    }
}
