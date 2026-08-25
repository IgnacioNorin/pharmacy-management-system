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
        private readonly IProductAlertHistoryRepository _historyRepository;

        public NotificationConfigService(INotificationConfigRepository repository, IProductAlertHistoryRepository historyRepository)
        {
            _repository = repository;
            _historyRepository = historyRepository;
        }

        public List<Product> ListExpirationDate(int days) => _repository.ListExpirationDate(days);

        public List<Product> ListStock(int criticalStock) => _repository.ListStock(criticalStock);

        public int ConfigDay() => _repository.ConfigDay();

        public int ConfigStock() => _repository.ConfigStock();

        public bool ConfigUpdate(NotificationConfig obj) => _repository.ConfigUpdate(obj);

        // Fase 3 of the alerts rework: turns the two raw product lists into one itemized,
        // severity-ranked list the notification center can render directly, instead of a single
        // generic "revise stock" sentence. Fase 4 layers traceability on top: every call diffs the
        // current list against product_alert_history's open rows and writes only the transitions
        // (new alert, severity change, or resolved) - see SyncAlertHistory.
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
                    Detail = outOfStock ? "Sin stock" : $"Stock: {p.stock}",
                    TriggerValue = p.stock
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

            alerts = alerts.OrderBy(a => a.Severity).ToList();
            SyncAlertHistory(alerts);
            return alerts;
        }

        public bool AcknowledgeAlert(int historyId, int personId) => _historyRepository.Acknowledge(historyId, personId);

        public List<ProductAlertHistoryEntry> GetAlertHistory(DateTime startDate, DateTime endDate) =>
            _historyRepository.GetHistory(startDate, endDate);

        // Writes only real transitions: a product entering an alert state (insert), one already
        // open changing severity (e.g. Low -> Critical, update), or one that no longer qualifies
        // (resolve). A poll that finds nothing new touches product_alert_history zero times.
        private void SyncAlertHistory(List<ProductAlert> alerts)
        {
            List<ProductAlertHistoryEntry> open = _historyRepository.GetOpenAlerts();
            var openByKey = open.ToDictionary(o => (o.ProductId, o.AlertType));
            var currentKeys = new HashSet<(int ProductId, AlertType AlertType)>();

            foreach (ProductAlert alert in alerts)
            {
                AlertType type = TypeOf(alert.Severity);
                currentKeys.Add((alert.ProductId, type));

                if (openByKey.TryGetValue((alert.ProductId, type), out ProductAlertHistoryEntry existing))
                {
                    alert.HistoryId = existing.Id;
                    if (existing.Severity != alert.Severity)
                    {
                        _historyRepository.UpdateSeverity(existing.Id, alert.Severity, alert.TriggerValue);
                    }
                }
                else
                {
                    int newId = _historyRepository.Insert(alert.ProductId, type, alert.Severity, alert.TriggerValue);
                    alert.HistoryId = newId != 0 ? newId : (int?)null;
                }
            }

            foreach (ProductAlertHistoryEntry entry in open)
            {
                if (!currentKeys.Contains((entry.ProductId, entry.AlertType)))
                {
                    _historyRepository.Resolve(entry.Id);
                }
            }
        }

        private static AlertType TypeOf(AlertSeverity severity) =>
            severity == AlertSeverity.Critical || severity == AlertSeverity.Low ? AlertType.Stock : AlertType.Expiration;
    }
}
