using PharmacySystem.Data;
using PharmacySystem.Model;

namespace PharmacySystem.Tests.Business
{
    internal class FakeStoreRepository : IStoreRepository
    {
        public Store StoredStore { get; set; } = new Store();
        public bool HasOperationalDataResult { get; set; }
        public bool UpdateStoreRowResult { get; set; } = true;
        public Store UpdatedWith { get; private set; }

        public Store ListStore() => StoredStore;

        public bool HasOperationalData() => HasOperationalDataResult;

        public bool UpdateStoreRow(Store obj)
        {
            UpdatedWith = obj;
            return UpdateStoreRowResult;
        }
    }
}
