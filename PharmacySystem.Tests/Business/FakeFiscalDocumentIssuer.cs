using PharmacySystem.Business;
using PharmacySystem.Fiscal;
using PharmacySystem.Model;

namespace PharmacySystem.Tests.Business
{
    internal class FakeFiscalDocumentIssuer : IFiscalDocumentIssuer
    {
        public FiscalDocumentResult ResultToReturn { get; set; } = new FiscalDocumentResult();
        public int IssueCalls { get; private set; }
        public int LastSaleId { get; private set; }
        public Sale LastSale { get; private set; }

        public FiscalDocumentResult Issue(int saleId, Sale sale)
        {
            IssueCalls++;
            LastSaleId = saleId;
            LastSale = sale;
            return ResultToReturn;
        }
    }
}
