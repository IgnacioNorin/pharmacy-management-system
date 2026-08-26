using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Data
{
    public interface INotificationConfigRepository
    {
        List<Product> ListExpirationDate(int days);
        List<Product> ListStock(int criticalStock);
        int ConfigDay();
        int ConfigStock();
        bool ConfigUpdate(NotificationConfig obj);
    }
}
