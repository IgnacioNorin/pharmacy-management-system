using System;
using PharmacySystem.Data;
using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    // The currency lock is the actual business rule this migration is meant to surface: it
    // decides *whether* to persist, using a raw data fact (HasOperationalData) the repository
    // exposes but does not itself act on. This is the shape every other still-thin service in
    // this phase (Category, Notification, Supplier) doesn't need, because they don't have a rule
    // like it today.
    public class StoreService : IStoreService
    {
        private readonly IStoreRepository _repository;

        public StoreService(IStoreRepository repository)
        {
            _repository = repository;
        }

        public Store ListStore() => _repository.ListStore();

        public bool HasOperationalData() => _repository.HasOperationalData();

        public bool UpdateStore(Store obj)
        {
            Store currentStore = _repository.ListStore();
            bool isChangingCurrency = !string.Equals(currentStore?.currencyCulture, obj.currencyCulture, StringComparison.OrdinalIgnoreCase);
            if (isChangingCurrency && _repository.HasOperationalData())
            {
                return false;
            }

            return _repository.UpdateStoreRow(obj);
        }
    }
}
