using PharmacySystem.Business;
using PharmacySystem.Model;
using System;
using System.Collections.Generic;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakeNotificationConfigService : INotificationConfigService
    {
        public int ConfigDayResult { get; set; }
        public int ConfigStockResult { get; set; }
        public bool ConfigUpdateResult { get; set; } = true;
        public NotificationConfig UpdatedWith { get; private set; }
        public List<Product> ListExpirationDateResult { get; set; } = new List<Product>();
        public List<Product> ListStockResult { get; set; } = new List<Product>();
        public List<ProductAlert> GetActiveAlertsResult { get; set; } = new List<ProductAlert>();

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
        public List<ProductAlert> GetActiveAlerts() => GetActiveAlertsResult;

        public bool ConfigUpdate(NotificationConfig obj)
        {
            UpdatedWith = obj;
            return ConfigUpdateResult;
        }

        public bool AcknowledgeAlertResult { get; set; } = true;
        public (int HistoryId, int PersonId)? AcknowledgedWith { get; private set; }

        public bool AcknowledgeAlert(int historyId, int personId)
        {
            AcknowledgedWith = (historyId, personId);
            return AcknowledgeAlertResult;
        }

        public List<ProductAlertHistoryEntry> GetAlertHistoryResult { get; set; } = new List<ProductAlertHistoryEntry>();

        public List<ProductAlertHistoryEntry> GetAlertHistory(DateTime startDate, DateTime endDate) => GetAlertHistoryResult;
    }
}
