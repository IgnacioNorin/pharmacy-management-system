using PharmacySystem.Business;
using PharmacySystem.Model;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakeStoreService : IStoreService
    {
        public Store ListStoreResult { get; set; } = new Store();
        public bool HasOperationalDataResult { get; set; }
        public bool UpdateStoreResult { get; set; } = true;
        public Store UpdatedWith { get; private set; }

        public Store ListStore() => ListStoreResult;
        public bool HasOperationalData() => HasOperationalDataResult;
        public bool UpdateStore(Store obj)
        {
            UpdatedWith = obj;
            return UpdateStoreResult;
        }
    }
}
