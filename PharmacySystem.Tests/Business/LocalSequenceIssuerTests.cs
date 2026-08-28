using PharmacySystem.Business;
using PharmacySystem.Fiscal;
using PharmacySystem.Model;
using Xunit;

namespace PharmacySystem.Tests.Business
{
    public class LocalSequenceIssuerTests
    {
        [Fact]
        public void Issue_LeavesTheReceiptInternalAndOverridesNothing()
        {
            var result = new LocalSequenceIssuer().Issue(10, new Sale { typeDocument = "Boleta" });

            Assert.Equal(FiscalStatuses.Interno, result.Status);
            Assert.Null(result.DocumentNumber);
            Assert.Null(result.TrackId);
            Assert.Null(result.Barcode);
        }
    }
}
