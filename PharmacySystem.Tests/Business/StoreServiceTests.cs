using PharmacySystem.Model;
using Xunit;
using BusinessStoreService = PharmacySystem.Business.StoreService;

namespace PharmacySystem.Tests.Business
{
    // The currency-lock rule itself, isolated from SQL Server entirely via FakeStoreRepository.
    // This is what splitting Data from Business actually buys: before this migration, the only
    // way to exercise this rule was the DB-backed StoreServiceTests.UpdateStore_* tests (still
    // kept, in Integration/StoreRepositoryTests.cs, to prove the real round trip) - these run
    // without a database at all.
    public class StoreServiceTests
    {
        [Fact]
        public void UpdateStore_ChangingCurrencyWithNoOperationalData_Succeeds()
        {
            var repository = new FakeStoreRepository
            {
                StoredStore = new Store { currencyCulture = "es-EC" },
                HasOperationalDataResult = false
            };
            var service = new BusinessStoreService(repository);

            bool result = service.UpdateStore(new Store { currencyCulture = "es-CL" });

            Assert.True(result);
            Assert.Equal("es-CL", repository.UpdatedWith.currencyCulture);
        }

        [Fact]
        public void UpdateStore_ChangingCurrencyWithOperationalData_IsRejectedWithoutWritingAnything()
        {
            var repository = new FakeStoreRepository
            {
                StoredStore = new Store { currencyCulture = "es-EC" },
                HasOperationalDataResult = true
            };
            var service = new BusinessStoreService(repository);

            bool result = service.UpdateStore(new Store { currencyCulture = "es-CL", address = "New address" });

            Assert.False(result);
            Assert.Null(repository.UpdatedWith); // the whole update is skipped, not just the currency field
        }

        [Fact]
        public void UpdateStore_SameCurrencyWithOperationalData_StillWritesOtherFields()
        {
            // The lock only blocks an actual currency change; unrelated store edits (address,
            // phone, etc.) must keep working after the pharmacy has real sales/purchases.
            var repository = new FakeStoreRepository
            {
                StoredStore = new Store { currencyCulture = "es-EC" },
                HasOperationalDataResult = true
            };
            var service = new BusinessStoreService(repository);

            bool result = service.UpdateStore(new Store { currencyCulture = "es-EC", address = "New address" });

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
