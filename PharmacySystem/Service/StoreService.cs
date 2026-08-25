using PharmacySystem.Data;
using PharmacySystem.Model;

namespace PharmacySystem.Logical
{
    // Thin adapter kept for screens not migrated yet (frmManagement.cs, MainForm.cs,
    // PrintSale.cs). Delegates to PharmacySystem.Business, which now owns the currency-lock
    // rule; delete this class once nothing calls .Instance anymore.
    class StoreService
    {
        private static StoreService instance = null;
        private readonly Business.IStoreService _inner;

        public StoreService()
        {
            _inner = new Business.StoreService(new StoreRepository(CompositionRoot.ConnectionFactory));
        }

        public static StoreService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new StoreService();
                }

                return instance;
            }
        }

        public Store ListStore() => _inner.ListStore();

        public bool HasOperationalData() => _inner.HasOperationalData();

        public bool UpdateStore(Store obj) => _inner.UpdateStore(obj);
    }
}
