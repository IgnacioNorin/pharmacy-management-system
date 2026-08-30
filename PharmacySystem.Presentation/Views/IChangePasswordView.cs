namespace PharmacySystem.Presentation
{
    public interface IChangePasswordView
    {
        string CurrentPassword { get; }
        string NewPassword { get; }
        string ConfirmPassword { get; }

        // True when opened from the forced-change flow on login: the dialog cannot be dismissed
        // without a successful change.
        bool Mandatory { get; }

        void ShowError(string message);
        void Close(bool changed);
    }
}
