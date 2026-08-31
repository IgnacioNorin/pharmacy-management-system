using System;
using System.Collections.Generic;
using PharmacySystem.Data;
using PharmacySystem.Model;

namespace PharmacySystem.Tests.Business
{
    internal class FakeProductAlertHistoryRepository : IProductAlertHistoryRepository
    {
        public List<ProductAlertHistoryEntry> OpenAlerts { get; set; } = new List<ProductAlertHistoryEntry>();
        public List<ProductAlertHistoryEntry> HistoryResult { get; set; } = new List<ProductAlertHistoryEntry>();
        public int NextInsertedId { get; set; } = 1;
        public bool AcknowledgeResult { get; set; } = true;

        public List<(int ProductId, AlertType AlertType, AlertSeverity Severity, decimal? TriggerValue)> Inserted { get; } = new List<(int, AlertType, AlertSeverity, decimal?)>();
        public List<(int HistoryId, AlertSeverity Severity, decimal? TriggerValue)> SeverityUpdates { get; } = new List<(int, AlertSeverity, decimal?)>();
        public List<int> Resolved { get; } = new List<int>();
        public (int HistoryId, int PersonId)? AcknowledgedWith { get; private set; }
        public List<int> Muted { get; } = new List<int>();
        public (int HistoryId, int PersonId)? MutedWith { get; private set; }
        public List<int> Unmuted { get; } = new List<int>();
        public bool MuteResult { get; set; } = true;
        public bool UnmuteResult { get; set; } = true;

        public List<ProductAlertHistoryEntry> GetOpenAlerts() => OpenAlerts;

        public int Insert(int productId, AlertType alertType, AlertSeverity severity, decimal? triggerValue)
        {
            Inserted.Add((productId, alertType, severity, triggerValue));
            return NextInsertedId;
        }

        public void UpdateSeverity(int historyId, AlertSeverity severity, decimal? triggerValue) =>
            SeverityUpdates.Add((historyId, severity, triggerValue));

        public void Resolve(int historyId) => Resolved.Add(historyId);

        public bool Acknowledge(int historyId, int personId)
        {
            AcknowledgedWith = (historyId, personId);
            return AcknowledgeResult;
        }

        public List<ProductAlertHistoryEntry> GetHistory(DateTime startDate, DateTime endDate) => HistoryResult;

        public bool Mute(int historyId, int personId)
        {
            Muted.Add(historyId);
            MutedWith = (historyId, personId);
            return MuteResult;
        }

        public bool Unmute(int historyId)
        {
            Unmuted.Add(historyId);
            return UnmuteResult;
        }
    }
}
