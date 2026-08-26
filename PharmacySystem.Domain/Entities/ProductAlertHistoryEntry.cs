using System;

namespace PharmacySystem.Model
{
    // One row of product_alert_history (Fase 4 of the alerts rework: traceability). Doubles as
    // both the "currently open alerts" projection (ResolvedAt == null) used to diff against the
    // live alert list, and the row shape for the history report.
    public class ProductAlertHistoryEntry
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public AlertType AlertType { get; set; }
        public AlertSeverity Severity { get; set; }
        public decimal? TriggerValue { get; set; }
        public DateTime DetectedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public int? AcknowledgedBy { get; set; }
        public string AcknowledgedByName { get; set; }
        public DateTime? AcknowledgedAt { get; set; }

        // Fase 5 (mute): silences this specific open row - same product, same type, same
        // severity - from the notification center summary without resolving it. Cleared
        // automatically the moment its severity changes (see NotificationConfigService.SyncAlertHistory).
        public DateTime? MutedAt { get; set; }
    }
}
