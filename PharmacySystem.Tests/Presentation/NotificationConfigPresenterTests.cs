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
            public PharmacySystem.Presentation.NotificationConfigPresenter Presenter =>
                new PharmacySystem.Presentation.NotificationConfigPresenter(View, Service);
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
