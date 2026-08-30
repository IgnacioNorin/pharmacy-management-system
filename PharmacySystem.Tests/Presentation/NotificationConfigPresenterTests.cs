using Xunit;

namespace PharmacySystem.Tests.Presentation
{
    public class NotificationConfigPresenterTests
    {
        private static NotificationConfigPresenterFixture Create() => new NotificationConfigPresenterFixture();

        private class NotificationConfigPresenterFixture
        {
            public readonly FakeNotificationConfigView View = new FakeNotificationConfigView();
            public readonly FakeNotificationConfigService Service = new FakeNotificationConfigService();
            public readonly FakeSecurityAudit Audit = new FakeSecurityAudit();
            public PharmacySystem.Presentation.CurrentUser User = TestUser.With("alertas.configurar");
            public PharmacySystem.Presentation.NotificationConfigPresenter Presenter =>
                new PharmacySystem.Presentation.NotificationConfigPresenter(View, Service, User, Audit);
        }

        [Fact]
        public void OnSave_ValidValues_AuditsTheChange()
        {
            var f = Create();
            f.View.DaysText = "7";
            f.View.StockText = "12";
            f.Service.ConfigUpdateResult = true;

            f.Presenter.OnSave();

            var evt = Assert.Single(f.Audit.Recorded);
            Assert.Equal("alert_config.update", evt.Action);
            Assert.Contains("7 día", evt.Summary);
            Assert.Contains("12", evt.Summary);
        }

        [Fact]
        public void OnSave_Rejected_DoesNotAudit()
        {
            var f = Create();
            f.View.DaysText = "5";
            f.View.StockText = "20";
            f.Service.ConfigUpdateResult = false;

            f.Presenter.OnSave();

            Assert.Empty(f.Audit.Recorded);
        }

        [Fact]
        public void OnSave_WithoutConfigurePermission_ShowsDeniedAndDoesNotSave()
        {
            var f = Create();
            f.User = TestUser.With();
            f.View.DaysText = "10";
            f.View.StockText = "5";

            f.Presenter.OnSave();

            Assert.Contains("No tiene permiso", f.View.ShownMessage ?? "");
            Assert.Equal(0, f.View.SaveSucceededCount);
        }

        [Fact]
        public void OnLoad_PopulatesViewFromService()
        {
            var f = Create();
            f.Service.ConfigDayResult = 7;
            f.Service.ConfigStockResult = 15;

            f.Presenter.OnLoad();

            Assert.Equal("7", f.View.SetDaysValue);
            Assert.Equal("15", f.View.SetStockValue);
        }

        [Theory]
        [InlineData("abc", "10")]
        [InlineData("10", "xyz")]
        public void OnSave_NonNumericValue_ShowsInvalidValueError(string days, string stock)
        {
            var f = Create();
            f.View.DaysText = days;
            f.View.StockText = stock;

            f.Presenter.OnSave();

            Assert.Equal(1, f.View.InvalidValueErrorCount);
            Assert.Null(f.Service.UpdatedWith);
        }

        [Theory]
        [InlineData("", "10")]
        [InlineData("10", "")]
        [InlineData("", "")]
        public void OnSave_EmptyField_ShowsEmptyFieldsError(string days, string stock)
        {
            var f = Create();
            f.View.DaysText = days;
            f.View.StockText = stock;

            f.Presenter.OnSave();

            Assert.Equal(1, f.View.EmptyFieldsErrorCount);
            Assert.Null(f.Service.UpdatedWith);
        }

        [Theory]
        [InlineData("0", "5")]       // days must be at least 1
        [InlineData("4000", "5")]    // days over ~10 years
        [InlineData("10", "-1")]     // a negative threshold never fires
        [InlineData("10", "999999")] // a huge threshold flags the whole catalogue
        public void OnSave_OutOfRangeValue_ShowsInvalidValueError_AndDoesNotSave(string days, string stock)
        {
            var f = Create();
            f.View.DaysText = days;
            f.View.StockText = stock;

            f.Presenter.OnSave();

            Assert.Equal(1, f.View.InvalidValueErrorCount);
            Assert.Null(f.Service.UpdatedWith);
        }

        [Theory]
        [InlineData("1", "0")]                 // lower bounds
        [InlineData("3650", "100000")]         // upper bounds
        public void OnSave_ValuesAtTheBounds_AreAccepted(string days, string stock)
        {
            var f = Create();
            f.View.DaysText = days;
            f.View.StockText = stock;
            f.Service.ConfigUpdateResult = true;

            f.Presenter.OnSave();

            Assert.NotNull(f.Service.UpdatedWith);
            Assert.Equal(1, f.View.SaveSucceededCount);
        }

        [Fact]
        public void OnSave_ValidValues_UpdatesAndShowsSuccess()
        {
            var f = Create();
            f.View.DaysText = "5";
            f.View.StockText = "20";
            f.Service.ConfigUpdateResult = true;

            f.Presenter.OnSave();

            Assert.Equal(5, f.Service.UpdatedWith.days);
            Assert.Equal(20, f.Service.UpdatedWith.criticalStock);
            Assert.Equal(1, f.View.SaveSucceededCount);
            Assert.Equal(0, f.View.SaveFailedCount);
        }

        [Fact]
        public void OnSave_ServiceRejectsUpdate_ShowsSaveFailed()
        {
            var f = Create();
            f.View.DaysText = "5";
            f.View.StockText = "20";
            f.Service.ConfigUpdateResult = false;

            f.Presenter.OnSave();

            Assert.Equal(1, f.View.SaveFailedCount);
            Assert.Equal(0, f.View.SaveSucceededCount);
        }
    }
}
