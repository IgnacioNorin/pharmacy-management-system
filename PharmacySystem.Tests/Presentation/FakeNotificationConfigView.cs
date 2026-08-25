using PharmacySystem.Presentation;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakeNotificationConfigView : INotificationConfigView
    {
        public string DaysText { get; set; } = "";
        public string StockText { get; set; } = "";

        public string SetDaysValue { get; private set; }
        public string SetStockValue { get; private set; }
        public int InvalidValueErrorCount { get; private set; }
        public int EmptyFieldsErrorCount { get; private set; }
        public int SaveSucceededCount { get; private set; }
        public int SaveFailedCount { get; private set; }

        public void SetDays(string value) => SetDaysValue = value;
        public void SetStock(string value) => SetStockValue = value;
        public void ShowInvalidValueError() => InvalidValueErrorCount++;
        public void ShowEmptyFieldsError() => EmptyFieldsErrorCount++;
        public void ShowSaveSucceeded() => SaveSucceededCount++;
        public void ShowSaveFailed() => SaveFailedCount++;
    }
}
