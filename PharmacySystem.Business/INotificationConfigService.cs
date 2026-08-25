using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    public interface INotificationConfigService
    {
        List<Product> ListExpirationDate(int days);
        List<Product> ListStock(int criticalStock);
        List<ProductAlert> GetActiveAlerts();
        int ConfigDay();
        int ConfigStock();
        bool ConfigUpdate(NotificationConfig obj);
    }
}
