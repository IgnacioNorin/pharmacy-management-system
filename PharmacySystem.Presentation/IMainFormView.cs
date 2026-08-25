namespace PharmacySystem.Presentation
{
    public interface IMainFormView
    {
        void SetUserName(string name);
        void SetAdministrativeMenusVisible(bool visible);
        void ShowExpirationWarning(bool visible, string message);
        void ShowStockWarning(bool visible, string message);
    }
}
