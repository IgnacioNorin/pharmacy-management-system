using PharmacySystem.Presentation;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakeMainFormView : IMainFormView
    {
        public string UserName { get; private set; }
        public bool? AdministrativeMenusVisible { get; private set; }
        public bool ExpirationWarningVisible { get; private set; }
        public string ExpirationWarningMessage { get; private set; }
        public bool StockWarningVisible { get; private set; }
        public string StockWarningMessage { get; private set; }

        public void SetUserName(string name) => UserName = name;
        public void SetAdministrativeMenusVisible(bool visible) => AdministrativeMenusVisible = visible;

        public void ShowExpirationWarning(bool visible, string message)
        {
            ExpirationWarningVisible = visible;
            ExpirationWarningMessage = message;
        }

        public void ShowStockWarning(bool visible, string message)
        {
            StockWarningVisible = visible;
            StockWarningMessage = message;
        }
    }
}
