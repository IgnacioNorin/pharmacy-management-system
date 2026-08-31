using System;
using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Data
{
    public interface IProductAlertHistoryRepository
    {
        // Currently open (ResolvedAt == null) rows - the set GetActiveAlerts() diffs the live
        // alert list against to decide what to insert/update/resolve.
        List<ProductAlertHistoryEntry> GetOpenAlerts();

        int Insert(int productId, AlertType alertType, AlertSeverity severity, decimal? triggerValue);
        void UpdateSeverity(int historyId, AlertSeverity severity, decimal? triggerValue);
        void Resolve(int historyId);
        bool Acknowledge(int historyId, int personId);
        bool Mute(int historyId, int personId);
        bool Unmute(int historyId);
        List<ProductAlertHistoryEntry> GetHistory(DateTime startDate, DateTime endDate);
    }
}
