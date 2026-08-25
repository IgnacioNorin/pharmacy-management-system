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

        public List<Product> ListExpirationDate(int days) => _repository.ListExpirationDate(days);

        public List<Product> ListStock(int criticalStock) => _repository.ListStock(criticalStock);

        public int ConfigDay() => _repository.ConfigDay();

        public int ConfigStock() => _repository.ConfigStock();

        public bool ConfigUpdate(NotificationConfig obj) => _repository.ConfigUpdate(obj);
    }
}
