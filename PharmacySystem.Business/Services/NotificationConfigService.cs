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

        // NotificationConfigService is a process-wide singleton (see CompositionRoot) and
        // RefreshAlerts() is fired from several places that can overlap in practice (a menu click
        // calling checkNotifications() directly and again through ShowForm, the 5-minute timer
        // tick, the StockChanged event) - each becomes a Task.Run on a threadpool thread. Without
        // this lock, two overlapping calls both read "no open row yet" in SyncAlertHistory before
        // either finishes inserting, and both insert the same (ProductId, AlertType) row, which
        // then crashes the *next* call's ToDictionary with a duplicate-key ArgumentException.
        private static readonly object _syncLock = new object();

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
                // p.stock here is the units in the lots that are expiring (set by the repository),
                // not the product's total stock.
                string units = p.stock > 0 ? $" ({p.stock} u.)" : "";
                alerts.Add(new ProductAlert
                {
                    ProductId = p.idProduct,
                    Code = p.code,
                    Name = p.name,
                    Severity = expired ? AlertSeverity.Expired : AlertSeverity.ExpiringSoon,
                    Detail = (expired ? "Venció el " : "Vence el ") + p.expirationDate.ToString("dd/MM/yyyy") + units,
                    TriggerValue = p.stock
                });
            }

            alerts = alerts.OrderBy(a => a.Severity).ToList();
            SyncAlertHistory(alerts);
            return alerts;
        }

        public bool AcknowledgeAlert(int historyId, int personId) => _historyRepository.Acknowledge(historyId, personId);

        public bool MuteAlert(int historyId) => _historyRepository.Mute(historyId);

        public bool UnmuteAlert(int historyId) => _historyRepository.Unmute(historyId);

        public List<ProductAlertHistoryEntry> GetAlertHistory(DateTime startDate, DateTime endDate) =>
            _historyRepository.GetHistory(startDate, endDate);

        // Writes only real transitions: a product entering an alert state (insert), one already
        // open changing severity (e.g. Low -> Critical, update), or one that no longer qualifies
        // (resolve). A poll that finds nothing new touches product_alert_history zero times.
        private void SyncAlertHistory(List<ProductAlert> alerts)
        {
            lock (_syncLock)
            {
                List<ProductAlertHistoryEntry> open = _historyRepository.GetOpenAlerts();
                // GroupBy + take-first instead of ToDictionary: tolerates a duplicate open row left
                // over from before this lock existed instead of crashing every future call.
                var openByKey = open
                    .GroupBy(o => (o.ProductId, o.AlertType))
                    .ToDictionary(g => g.Key, g => g.First());
                var currentKeys = new HashSet<(int ProductId, AlertType AlertType)>();

                foreach (ProductAlert alert in alerts)
                {
                    AlertType type = TypeOf(alert.Severity);
                    currentKeys.Add((alert.ProductId, type));

                    if (openByKey.TryGetValue((alert.ProductId, type), out ProductAlertHistoryEntry existing))
                    {
                        alert.HistoryId = existing.Id;
                        alert.AcknowledgedAt = existing.AcknowledgedAt;

                        if (existing.Severity != alert.Severity)
                        {
                            _historyRepository.UpdateSeverity(existing.Id, alert.Severity, alert.TriggerValue);
                            // The mute applied to the old severity - a worsened or improved alert
                            // is a different situation the user hasn't seen yet, so it un-mutes.
                            alert.MutedAt = null;
                        }
                        else
                        {
                            alert.MutedAt = existing.MutedAt;
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
        }

        private static AlertType TypeOf(AlertSeverity severity) =>
            severity == AlertSeverity.Critical || severity == AlertSeverity.Low ? AlertType.Stock : AlertType.Expiration;
    }
}
