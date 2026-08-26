using System.Collections.Generic;

namespace PharmacySystem.Model
{
    // View-facing snapshot for the landing screen (frmHome): the day's sales plus the same active
    // alert list MainForm's bell already computes, broken down into the counts the dashboard tiles
    // need. Nothing here is persisted - it's recomputed on every load.
    public class HomeSummary
    {
        public int SalesTodayCount { get; set; }
        public decimal SalesTodayTotal { get; set; }

        // Critical/Expired vs Low/ExpiringSoon - the same two-tier urgency split ModalAlerts uses.
        public int UrgentAlertsCount { get; set; }
        public int OtherAlertsCount { get; set; }

        public int ExpiringSoonCount { get; set; }
        public int CriticalStockCount { get; set; }

        public List<ProductAlert> AttentionList { get; set; } = new List<ProductAlert>();
    }
}
