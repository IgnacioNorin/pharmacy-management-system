using System;
using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    public interface INotificationConfigService
    {
        List<Product> ListExpirationDate(int days);
        List<Product> ListStock(int criticalStock);
        List<ProductAlert> GetActiveAlerts();
        bool AcknowledgeAlert(int historyId, int personId);
        bool MuteAlert(int historyId, int personId);
        bool UnmuteAlert(int historyId);
        List<ProductAlertHistoryEntry> GetAlertHistory(DateTime startDate, DateTime endDate);
        int ConfigDay();
        int ConfigStock();
        bool ConfigUpdate(NotificationConfig obj);
    }
}
