using PharmacySystem.Fiscal;
using PharmacySystem.Model;
using Xunit;
using BusinessSaleService = PharmacySystem.Business.SaleService;

namespace PharmacySystem.Tests.Business
{
    // The fiscal-issuer orchestration in SaleService.Register, isolated from SQL Server.
    public class SaleServiceTests
    {
        private static Sale AnySale() => new Sale { oPerson = new Person { idPerson = 1 } };

        [Fact]
        public void Register_PersistsSaleThenHandsItToTheIssuerAndStoresTheResult()
        {
            var repository = new FakeSaleRepository { RegisterResult = 42 };
            var issuer = new FakeFiscalDocumentIssuer
            {
                ResultToReturn = new FiscalDocumentResult { Status = FiscalStatuses.Interno }
            };
            var service = new BusinessSaleService(repository, issuer);

            int id = service.Register(AnySale());

            Assert.Equal(42, id);
            Assert.Equal(1, issuer.IssueCalls);
            Assert.Equal(42, issuer.LastSaleId);
            Assert.Equal(1, repository.SaveFiscalResultCalls);
            Assert.Equal(42, repository.SavedFiscalSaleId);
            Assert.Equal(FiscalStatuses.Interno, repository.SavedFiscalResult.Status);
        }

        [Fact]
        public void Register_WhenPersistenceFails_DoesNotCallTheIssuer()
        {
            var repository = new FakeSaleRepository { RegisterResult = 0 };
            var issuer = new FakeFiscalDocumentIssuer();
            var service = new BusinessSaleService(repository, issuer);

            int id = service.Register(AnySale());

            Assert.Equal(0, id);
            Assert.Equal(0, issuer.IssueCalls);
            Assert.Equal(0, repository.SaveFiscalResultCalls);
        }

        [Fact]
        public void Register_WhenIssuerReturnsNull_DoesNotWriteAFiscalResult()
        {
            var repository = new FakeSaleRepository { RegisterResult = 7 };
            var issuer = new FakeFiscalDocumentIssuer { ResultToReturn = null };
            var service = new BusinessSaleService(repository, issuer);

            int id = service.Register(AnySale());

            Assert.Equal(7, id);
            Assert.Equal(1, issuer.IssueCalls);
            Assert.Equal(0, repository.SaveFiscalResultCalls);
        }
    }
}
