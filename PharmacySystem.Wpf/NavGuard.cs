namespace PharmacySystem.Ui
{
    // A view hosted in MainWindow's content area implements this when it may hold unsaved work
    // (a sale in progress, an unsaved form). The shell calls CanNavigateAway() before switching
    // sections and cancels the navigation if it returns false.
    public interface INavGuard
    {
        bool CanNavigateAway();
    }
}
