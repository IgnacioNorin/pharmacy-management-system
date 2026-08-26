using PharmacySystem.Model;

namespace PharmacySystem.Data
{
    public interface IStoreRepository
    {
        Store ListStore();

        // Raw data fact: true once a sale or purchase row exists. The decision of what to do
        // with that fact (block a currency change) belongs to Business.StoreService, not here.
        bool HasOperationalData();

        // Unconditional persistence - no currency-lock check. That rule lives in
        // Business.StoreService.UpdateStore, which is the only intended caller.
        bool UpdateStoreRow(Store obj);
    }
}
