using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    public interface INotificationConfigService
    {
        List<Product> ListExpirationDate();
        List<Product> ListStock();
        int ConfigDay();
        int ConfigStock();
        bool ConfigUpdate(NotificationConfig obj);
    }
}
