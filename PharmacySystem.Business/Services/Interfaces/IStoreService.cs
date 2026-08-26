using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    public interface IStoreService
    {
        Store ListStore();
        bool HasOperationalData();
        bool UpdateStore(Store obj);
    }
}
