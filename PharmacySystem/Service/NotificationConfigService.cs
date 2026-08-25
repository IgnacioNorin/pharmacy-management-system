using System.Collections.Generic;
using PharmacySystem.Data;
using PharmacySystem.Model;

namespace PharmacySystem.Logical
{
    // Thin adapter kept for screens not migrated yet (MainForm.cs, ModalConfignotification.cs),
    // which construct this with `new NotificationConfigService()` - no singleton here, same as
    // the original. Delegates to PharmacySystem.Business; delete once nothing calls it directly.
    public class NotificationConfigService
    {
        private readonly Business.INotificationConfigService _inner;

        public NotificationConfigService()
        {
            _inner = new Business.NotificationConfigService(new NotificationConfigRepository(CompositionRoot.ConnectionFactory));
        }

        public List<Product> ListExpirationDate() => _inner.ListExpirationDate();

        public List<Product> ListStock() => _inner.ListStock();

        public int ConfigDay() => _inner.ConfigDay();

        public int ConfigStock() => _inner.ConfigStock();

        public bool ConfigUpdate(NotificationConfig obj) => _inner.ConfigUpdate(obj);
    }
}
