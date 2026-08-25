using System;
using System.Collections.Generic;
using System.Linq;
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

        // Fase 3 of the alerts rework: turns the two raw product lists into one itemized,
        // severity-ranked list the notification center can render directly, instead of a single
        // generic "revise stock" sentence.
        public List<ProductAlert> GetActiveAlerts()
        {
            var alerts = new List<ProductAlert>();

            foreach (Product p in _repository.ListStock(ConfigStock()))
            {
                bool outOfStock = p.stock <= 0;
                alerts.Add(new ProductAlert
                {
                    ProductId = p.idProduct,
                    Code = p.code,
                    Name = p.name,
                    Severity = outOfStock ? AlertSeverity.Critical : AlertSeverity.Low,
                    Detail = outOfStock ? "Sin stock" : $"Stock: {p.stock}"
                });
            }

            foreach (Product p in _repository.ListExpirationDate(ConfigDay()))
            {
                bool expired = p.expirationDate.Date < DateTime.Today;
                alerts.Add(new ProductAlert
                {
                    ProductId = p.idProduct,
                    Code = p.code,
                    Name = p.name,
                    Severity = expired ? AlertSeverity.Expired : AlertSeverity.ExpiringSoon,
                    Detail = (expired ? "Venció el " : "Vence el ") + p.expirationDate.ToString("dd/MM/yyyy")
                });
            }

            return alerts.OrderBy(a => a.Severity).ToList();
        }
    }
}
