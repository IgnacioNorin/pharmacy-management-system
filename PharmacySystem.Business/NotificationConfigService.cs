using System.Collections.Generic;
using PharmacySystem.Data;
using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    public class NotificationConfigService : INotificationConfigService
    {
        private readonly INotificationConfigRepository _repository;

        public NotificationConfigService(INotificationConfigRepository repository)
        {
            _repository = repository;
        }

        public List<Product> ListExpirationDate() => _repository.ListExpirationDate();

        public List<Product> ListStock() => _repository.ListStock();

        public int ConfigDay() => _repository.ConfigDay();

        public int ConfigStock() => _repository.ConfigStock();

        public bool ConfigUpdate(NotificationConfig obj) => _repository.ConfigUpdate(obj);
    }
}
