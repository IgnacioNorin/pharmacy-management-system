using System.Collections.Generic;
using PharmacySystem.Data;
using PharmacySystem.Model;

namespace PharmacySystem.Tests.Business
{
    internal class FakeNotificationConfigRepository : INotificationConfigRepository
    {
        public List<Product> ListExpirationDateResult { get; set; } = new List<Product>();
        public List<Product> ListStockResult { get; set; } = new List<Product>();
        public int ConfigDayResult { get; set; }
        public int ConfigStockResult { get; set; }
        public bool ConfigUpdateResult { get; set; } = true;
        public NotificationConfig UpdatedWith { get; private set; }
        public int? RequestedDays { get; private set; }
        public int? RequestedCriticalStock { get; private set; }

        public List<Product> ListExpirationDate(int days)
        {
            RequestedDays = days;
            return ListExpirationDateResult;
        }

        public List<Product> ListStock(int criticalStock)
        {
            RequestedCriticalStock = criticalStock;
            return ListStockResult;
        }
        public int ConfigDay() => ConfigDayResult;
        public int ConfigStock() => ConfigStockResult;

        public bool ConfigUpdate(NotificationConfig obj)
        {
            UpdatedWith = obj;
            return ConfigUpdateResult;
        }
    }
}
