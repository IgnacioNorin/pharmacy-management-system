using PharmacySystem.Model;
using Xunit;
using BusinessStoreService = PharmacySystem.Business.StoreService;

namespace PharmacySystem.Tests.Business
{
    public class StoreServiceTests
    {
        [Fact]
        public void UpdateStore_DelegatesToRepository()
        {
            var repository = new FakeStoreRepository { UpdateStoreRowResult = true };
            var service = new BusinessStoreService(repository);

            bool result = service.UpdateStore(new Store { address = "New address" });

            Assert.True(result);
            Assert.Equal("New address", repository.UpdatedWith.address);
        }

        [Fact]
        public void HasOperationalData_DelegatesToRepository()
        {
            var repository = new FakeStoreRepository { HasOperationalDataResult = true };
            var service = new BusinessStoreService(repository);

            Assert.True(service.HasOperationalData());
        }
    }
}
