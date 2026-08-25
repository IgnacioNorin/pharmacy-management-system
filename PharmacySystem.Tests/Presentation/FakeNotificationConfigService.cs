using PharmacySystem.Business;
using PharmacySystem.Model;
using System.Collections.Generic;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakeNotificationConfigService : INotificationConfigService
    {
        public int ConfigDayResult { get; set; }
        public int ConfigStockResult { get; set; }
        public bool ConfigUpdateResult { get; set; } = true;
        public NotificationConfig UpdatedWith { get; private set; }

        public List<Product> ListExpirationDate() => new List<Product>();
        public List<Product> ListStock() => new List<Product>();
        public int ConfigDay() => ConfigDayResult;
        public int ConfigStock() => ConfigStockResult;

        public bool ConfigUpdate(NotificationConfig obj)
        {
            UpdatedWith = obj;
            return ConfigUpdateResult;
        }
    }
}
