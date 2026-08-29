using System;
using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Business;
using PharmacySystem.Helpers;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // Landing screen presenter. Reuses the exact same "active alerts" computation MainForm's bell
    // badge already runs (GetActiveAlerts(), muted excluded) instead of a separate query - the
    // tiles and the badge must always agree on what counts as an open alert.
    public class HomePresenter
    {
        private const int AttentionListSize = 5;

        private readonly IHomeView _view;
        private readonly ISaleService _saleService;
        private readonly INotificationConfigService _notificationService;

        public HomePresenter(IHomeView view, ISaleService saleService, INotificationConfigService notificationService)
        {
            _view = view;
            _saleService = saleService;
            _notificationService = notificationService;
        }

        public void OnLoad()
        {
            DateTime today = DateTime.Today;

            List<ProductAlert> alerts = _notificationService.GetActiveAlerts()
                .Where(a => a.MutedAt == null)
                .ToList();

            var summary = new HomeSummary
            {
                SalesTodayCount = _saleService.ReportSale(today, today, 0).Count,
                SalesTodayTotal = _saleService.SumTotalPay(today, today),
                UrgentAlertsCount = alerts.Count(a => a.Severity == AlertSeverity.Critical || a.Severity == AlertSeverity.Expired),
                OtherAlertsCount = alerts.Count(a => a.Severity == AlertSeverity.Low || a.Severity == AlertSeverity.ExpiringSoon),
                ExpiringSoonCount = alerts.Count(a => a.Severity == AlertSeverity.ExpiringSoon),
                CriticalStockCount = alerts.Count(a => a.Severity == AlertSeverity.Critical),
                AttentionList = alerts.Take(AttentionListSize).ToList()
            };

            _view.SetSummary(summary);
        }
    }
}
