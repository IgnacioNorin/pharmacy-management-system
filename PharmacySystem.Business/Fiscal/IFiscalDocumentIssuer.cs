using PharmacySystem.Fiscal;
using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    // Seam for electronic tax document emission. The registered sale is handed here right
    // after it is persisted; the implementation decides whether it is just an internal
    // receipt or a real DTE sent to a provider / the tax authority, and returns the result
    // to store back on the sale. Swap the registered implementation in CompositionRoot to
    // switch providers - nothing else changes.
    public interface IFiscalDocumentIssuer
    {
        FiscalDocumentResult Issue(int saleId, Sale sale);
    }
}
