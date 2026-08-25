using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Data
{
    public interface INotificationConfigRepository
    {
        List<Product> ListExpirationDate();
        List<Product> ListStock();
        int ConfigDay();
        int ConfigStock();
        bool ConfigUpdate(NotificationConfig obj);
    }
}
