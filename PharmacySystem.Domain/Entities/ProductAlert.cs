using System;

namespace PharmacySystem.Model
{
    // One row for the notification center (Fase 3 of the alerts rework): a product that is
    // either critically/low on stock, expired, or expiring soon, with the specific detail text
    // already formatted - the View never has to know how a severity was derived.
    public class ProductAlert
    {
        public int ProductId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public AlertSeverity Severity { get; set; }
        public string Detail { get; set; } = string.Empty;

        // Fase 4 (traceability): the open product_alert_history row this alert is backed by, so
        // the notification center can let the user acknowledge it. Null only if the history write
        // itself failed (fail-open: a DB hiccup on the history table must never hide a real alert).
        public int? HistoryId { get; set; }
        public decimal? TriggerValue { get; set; }

        // Fase 5 (mute): mirrors the backing history row's state so the notification center can
        // show a status (Pendiente/Leída/Muteada) and MainForm can exclude muted alerts from the
        // header summary without a second round-trip.
        public DateTime? AcknowledgedAt { get; set; }
        public DateTime? MutedAt { get; set; }
    }
}
