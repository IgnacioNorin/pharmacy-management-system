using System.Collections.Generic;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using Xunit;

namespace PharmacySystem.Tests.Presentation
{
    public class HomePresenterTests
    {
        private static (HomePresenter Presenter, FakeHomeView View, FakeSaleService Sales, FakeNotificationConfigService Notifications) Create()
        {
            var view = new FakeHomeView();
            var sales = new FakeSaleService();
            var notifications = new FakeNotificationConfigService();
            var presenter = new HomePresenter(view, sales, notifications);
            return (presenter, view, sales, notifications);
        }

        [Fact]
        public void OnLoad_ReadsTodaysSalesCountAndTotalFromTheSaleService()
        {
            var f = Create();
            f.Sales.ReportResult = new List<SaleReportRow> { new SaleReportRow(), new SaleReportRow() };
            f.Sales.SumTotalPayResult = 250.75m;

            f.Presenter.OnLoad();

            Assert.Equal(2, f.View.Summary.SalesTodayCount);
            Assert.Equal(250.75m, f.View.Summary.SalesTodayTotal);
        }

        [Fact]
        public void OnLoad_SplitsAlertsIntoUrgentAndOtherByLevel()
        {
            var f = Create();
            f.Notifications.GetActiveAlertsResult = new List<ProductAlert>
            {
                new ProductAlert { Severity = AlertSeverity.Critical },
                new ProductAlert { Severity = AlertSeverity.Expired },
                new ProductAlert { Severity = AlertSeverity.Low },
                new ProductAlert { Severity = AlertSeverity.ExpiringSoon }
            };

            f.Presenter.OnLoad();

            Assert.Equal(2, f.View.Summary.UrgentAlertsCount);
            Assert.Equal(2, f.View.Summary.OtherAlertsCount);
        }

        [Fact]
        public void OnLoad_MutedAlerts_AreExcludedFromEveryCount()
        {
            var f = Create();
            f.Notifications.GetActiveAlertsResult = new List<ProductAlert>
            {
                new ProductAlert { Severity = AlertSeverity.Critical, MutedAt = null },
                new ProductAlert { Severity = AlertSeverity.Critical, MutedAt = System.DateTime.Today }
            };

            f.Presenter.OnLoad();

            Assert.Equal(1, f.View.Summary.UrgentAlertsCount);
            Assert.Equal(1, f.View.Summary.CriticalStockCount);
            Assert.Single(f.View.Summary.AttentionList);
        }

        [Fact]
        public void OnLoad_CountsExpiringSoonAndCriticalStockSeparately()
        {
            var f = Create();
            f.Notifications.GetActiveAlertsResult = new List<ProductAlert>
            {
                new ProductAlert { Severity = AlertSeverity.ExpiringSoon },
                new ProductAlert { Severity = AlertSeverity.ExpiringSoon },
                new ProductAlert { Severity = AlertSeverity.Critical }
            };

            f.Presenter.OnLoad();

            Assert.Equal(2, f.View.Summary.ExpiringSoonCount);
            Assert.Equal(1, f.View.Summary.CriticalStockCount);
        }

        [Fact]
        public void OnLoad_AttentionList_KeepsOnlyTheFirstFiveAlerts()
        {
            var f = Create();
            var alerts = new List<ProductAlert>();
            for (int i = 0; i < 8; i++)
            {
                alerts.Add(new ProductAlert { Severity = AlertSeverity.Low, Name = $"P{i}" });
            }
            f.Notifications.GetActiveAlertsResult = alerts;

            f.Presenter.OnLoad();

            Assert.Equal(5, f.View.Summary.AttentionList.Count);
            Assert.Equal("P0", f.View.Summary.AttentionList[0].Name);
        }
    }
}
